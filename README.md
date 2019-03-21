# Acadian.FSharp

This library contains some useful utility functions that feel missing from the F# core library. It also contains
computation expression builders for Option and Result workflows.

The library is small, in a single file, and documented, so you could just [read the source code](Acadian.FSharp/FSharp.fs),
or keep reading for an overview of the additions and an in-depth look at the new workflows.

## Top-level Functions

- `cnst` returns a function that returns the given constant value. Equivalent to `(fun _ -> a)`.
- `flip` flips the order of two arguments of a function. `flip f a b = f b a`.
  Useful in pipelining when your piped value is the first argument to a function.
  Example: `list1 |> flip List.append list2`. With the flip, the lists will occur in the resulting list in the order
  in which they appear in the code even though we're using a forward pipe. Without the flip, the elements of `list1`
  would occur _after_ the elements of `list2` in the resulting list.
- `tryResult` calls a function and returns `Ok` of the result if no exception was thrown or `Error` of the thrown
  exception.
- `tryCast<'a>` returns `Some` if the object is the given type; otherwise returns `None`.
- `nil` value is the null `System.Nullable<T>` value. This is purely to make it easier to refer to.
- `|?` operator is `defaultArg` and equivalent to `|> Option.defaultValue`.

## Module Extensions

This library extends several core modules:

- `String` is filled out with almost all System.String member functions as module functions, as well as a few new
  functions such as `ifEmpty` and `ifWhitespace`.
- `Seq` has additions such as `isNotEmpty` and `tryMax`.
- `Option` has additions such as `iter2`, `iter3`, and `ofCond`.
- `Result` has additions to assist in validation workflows such as `isOk`, `isError`, `okIf`, `errorIf`, `ofCond`,
  `ofOption`, `ofRegex`, `accumulate` and more.
- `Async` has additions to fill gaps in the base library: `map`, `mapAsync`, and `AwaitPlainTask`.

## New Modules

The following new modules are introduced:

- `Parse` contains functions to parse strings into various types or check for parsability.
- `Patterns` contains active patterns to parse strings.
- `Tuple` contains some functions to construct tuples from arguments.
- `Reflection` contains utilities that use reflection such as `unionCaseName`.


## Workflows / Computation Expression Builders

### Option Builder

The `option` workflow is appropriate when you need to compute an option value from a chain of computations that return
an option.

- `let!` (Bind) calls Option.bind.
- `return` wraps the value in `Some`.
- Zero returns `None`.
- Combine calls `Option.orElseWith` which means that if you have more than one expression, the first `Some` value is
  returned or `None` is returned if all expressions evaluate to `None`.
- Expression evaluation is delayed. If a bind expression returns `None` or if a non-last expression returns `Some` then
  the following code is not executed.

#### Refactoring Binds to Option Builder

Let's look at an example where we perform a series of lookups. Using pattern matching, we might write:

```fsharp
// using pattern matching
let getCustomerNameForInvoice db invoiceId =
    match db.GetInvoice invoiceId with
    | Some inv ->
        // an invoice might not have a customer ID
        match inv.CustomerId with
        | Some custId ->
            match db.GetCustomer custId with
            | Some cust -> Some cust.FullName
            | None -> None
        | None -> None
    | None -> None
```

This function does a series of database lookups that return an option. The computation can only continue when each
lookup succeeds. 

Pattern matching is a bit cumbersome here. Every match against an option returns None when the input is None. Let's
replace the `match` statements with `Option.bind` to write our computation as a series of continuations.

```fsharp
// using Option.bind
let getCustomerNameForInvoice db invoiceId =
    db.GetInvoice invoiceId
    |> Option.bind (fun inv ->
        inv.CustomerId
        |> Option.bind (fun custId ->
            db.GetCustomer custId
            |> Option.map (fun cust ->
                cust.FullName
            )
        )
    )
```

Any time that you can write a computation as a series of continuations like this, a computation expression could make
it easier to read and write. Let's refactor again using the `option` computation expression:

```fsharp
// using option builder
let getCustomerNameForInvoice db invoiceId = option {
    let! inv = db.GetInvoice invoiceId
    let! custId = inv.CustomerId
    let! cust = db.GetCustomer custId
    return cust.FullName
}
```

We've eliminated the nesting and made the code a straightforward series of bind statements with the `let!` syntax.

#### Short-circuiting with If

The `option` computation expression implements Combine with `Option.orElseWith`, which means that multiple expressions
will evaluate to the first `Some` value. We can use this to write code with "early returns" for `Some` values.

Here's a contrived example of getting a shipping address where we can immediately return a value if a condition is met:

```fsharp
let getShippingAddress db order =
    if not order.UseBillingAddress then
        Some order.ShippingAddress
    else
        let pmt = db.GetPaymentInfo order.Id
        pmt |> Option.map (fun p -> p.BillingAddress)
```

This isn't too bad, but what if that `else` case was more complicated and also had these if-else structures inside of
it? The indentation involved could get quite ugly. Using the `option` computation expression, we can eliminate the
indentation for the else case, and replace that map while we're at it:

```fsharp
let getShippingAddress db order = option {
    if not order.UseBillingAddress then
        return order.ShippingAddress

    let! pmt = db.GetPaymentInfo order.Id
    return pmt.BillingAddress
}
```

Here we have two top-level expressions: the `if` expression and the code that follows.

Option Builder's Combine will evaluate the first expression and if it is `Some`, it returns that value and does not
evaluate the following expressions. If it is None, it evaluates the next expression and repeats.

The way computation expressions work is that an `if` with no `else` evaluates to the Zero value when the `if` condition
is not met. Option Builder's Zero value is None, so it's like an implicit `else return! None`.

This allows us to write the almost C#-style code you see here that looks like an early return. The catch is that it only
short-circuits for `Some` values, so writing `if condition then return! None` would have no effect at all.


### Result Builder

The `result` workflow is appropriate when you need to compute a `Result` value from a chain of computations that return
a `Result`, such as validation.

- `let!` (Bind) calls Result.bind.
- `return` wraps the value in `Ok`.
- Zero returns `Ok ()`.
- Combine restricts the first expression to `Result<unit, _>`. If the first expression evaluates to an `Error` value,
  that is returned as the value of the computation expression, but if it is `Ok ()` then the next expression is
  evaluated.
- Expression evaluation is delayed. If a bind expression returns an `Error` or if a non-last expression returns `Error`
  then the following code is not executed.

#### Refactoring Validation to Result Builder

As an example, let's validate a date range from text user inputs. This could be written with pattern matching:

```fsharp
let validateDates beginText endText =
    match Parse.Date beginText with // Parse.Date is an Acadian.FSharp function
    | None -> Error "Begin date is not in a valid format"
    | Some beginDate ->
        match Parse.Date endText with
        | None -> Error "End date is not in a valid format"
        | Some endDate ->
            if endDate < beginDate then
                Error "End date cannot be before start date"
            else
                Ok (beginDate, endDate)
```

We can refactor this to use bind with the help of this library's `Result.ofOption`:

```fsharp
let validateDates beginText endText =
    Parse.Date beginText |> Result.ofOption "Begin date is not in a valid format"
    |> Result.bind (fun beginDate ->
        Parse.Date endText |> Result.ofOption "End date is not in a valid format"
        |> Result.bind (fun endDate ->
            if endDate < beginDate then
                Error "End date cannot be before start date"
            else
                Ok (beginDate, endDate)
        )
    )
```

From here we can easily refactor to eliminate the nesting with the `result` computation expression:

```fsharp
let validateDates beginText endText = result {
    let! beginDate = Parse.Date beginText |> Result.ofOption "Begin date is not in a valid format"
    let! endDate = Parse.Date endText |> Result.ofOption "End date is not in a valid format"
    if endDate < beginDate then
        return! Error "End date cannot be before start date"
    else
        return (beginDate, endDate)
}
```

#### Short-circuiting with If

Taking the previous example, we can tweak it to remove the nesting on the return by simply taking it out of the `else`:

```fsharp
let validateDates beginText endText = result {
    let! beginDate = Parse.Date beginText |> Result.ofOption "Begin date is not in a valid format"
    let! endDate = Parse.Date endText |> Result.ofOption "End date is not in a valid format"
    if endDate < beginDate then
        return! Error "End date cannot be before start date"
    return (beginDate, endDate)
}
```

This works similarly to the `option` example, except the short-circuiting value is `Error` instead of `Some`.

This is more advantageous if you have multiple short-circuiting checks with calculations in-between where it could
remove multiple levels of indentation.

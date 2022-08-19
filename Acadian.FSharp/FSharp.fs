namespace Acadian.FSharp

[<AutoOpen>]
module Prelude =
    /// Returns a function that returns the given constant value. `(fun _ -> a)`
    let inline cnst a = (fun _ -> a)
    /// Returns a function that returns the given constant value. `(fun _ _ -> a)`
    let inline cnst2 a = (fun _ _ -> a)

    /// Flips the order of two arguments of a function. `flip f a b = f b a`
    let inline flip f a b = f b a

    /// Applies a side-effect function to a value, ignoring the result, and returns the original value.
    let inline tee f x = f x |> ignore; x

    /// Applies a side-effect function to a value, ignoring the result, and returns the original value.
    let inline (|>!) x fn = tee fn x

    /// Transforms an uncurried function to a curried function.
    let inline curry f a b = f(a,b)

    /// Transforms an uncurried function to a curried function.
    let inline uncurry f (a,b) = f a b

    /// Transforms an uncurried function to a triple-curried function.
    let inline curry3 f a b c = f (a, b, c)

    /// Transforms an uncurried function to a triple-curried function.
    let inline uncurry3 f (a,b,c) = f a b c

    /// Returns Some if the object is the given type; otherwise returns None.
    let inline tryCast<'a> (o: obj) =
        match o with
        | :? 'a as a -> Some a
        | _ -> None

    /// Calls f and returns the result in Ok if it did not throw; otherwise catches the exception and returns it in an Error.
    let inline tryResult f =
        try Ok (f ())
        with e -> Error e

    /// The null System.Nullable value.
    let nil = System.Nullable()

    /// defaultArg operator, equivalent to `|> Option.defaultValue`
    let (|?) = defaultArg

    /// Builds an Option using computation expression syntax.
    /// Combine returns the first Some value, so an `if` without an else that returns a Some value will return that value
    /// without executing the following code, while a None will return the result of the following code.
    let option = OptionBuilder()

    /// Builds an Async Option using computation expression syntax.
    /// Combine returns the first Some value, so an `if` without an else that returns a Some value will return that value
    /// without executing the following code, while a None will return the result of the following code.
    let asyncOption = AsyncOptionBuilder()

    /// Builds a Result using computation expression syntax.
    /// Combine returns upon encountering an Error value, so an `if` without an else that returns an Error value will return
    /// that value without executing the following code, while an Ok () will return the result of the following code.
    let result = ResultBuilder()

    /// Builds an Async Result using computation expression syntax.
    /// Combine returns upon encountering an Error value, so an `if` without an else that returns an Error value will return
    /// that value without executing the following code, while an Ok () will return the result of the following code.
    let asyncResult = AsyncResultBuilder()


module Parse =
    let inline private tryToOption (s, v) = if s then Some v else None

    /// Attempts to parse a string into an int and returns Some upon success and None upon failure.
    let Int s = System.Int32.TryParse(s) |> tryToOption
    /// Attempts to parse a string into a double and returns Some upon success and None upon failure.
    let Double s = System.Double.TryParse(s) |> tryToOption
    /// Attempts to parse a string into a decimal and returns Some upon success and None upon failure.
    let Decimal s = System.Decimal.TryParse(s) |> tryToOption
    /// Attempts to parse a string into a DateTime and returns Some upon success and None upon failure.
    let Date s = System.DateTime.TryParse(s) |> tryToOption

    /// Returns true if the string is parsable into an int.
    let IsInt s = System.Int32.TryParse(s) |> fst
    /// Returns true if the string is parsable into a double.
    let IsDouble s = System.Double.TryParse(s) |> fst
    /// Returns true if the string is parsable into a decimal.
    let IsDecimal s = System.Decimal.TryParse(s) |> fst
    /// Returns true if the string is parsable into a DateTime.
    let IsDate s = System.DateTime.TryParse(s) |> fst

module Patterns =
    /// Matches strings that are parsable to an int.
    let (|Int|_|) = Parse.Int
    /// Matches strings that are parsable to a double.
    let (|Double|_|) = Parse.Double
    /// Matches strings that are parsable to a decimal.
    let (|Decimal|_|) = Parse.Decimal
    /// Matches strings that are parsable to a DateTime.
    let (|Date|_|) = Parse.Date

module String =
    open System

    /// Returns true if the string is null or empty.
    let isEmpty = String.IsNullOrEmpty

    /// Returns true if the string is not null nor empty.
    let isNotEmpty = not << isEmpty

    /// Returns true if the string is null or whitespace.
    let isWhiteSpace = String.IsNullOrWhiteSpace

    /// Returns true if the string is not null nor whitespace.
    let isNotWhiteSpace = not << isWhiteSpace

    /// Returns fallback if the string is null or empty; the input string otherwise.
    let inline ifEmpty fallback s = if isEmpty s then fallback else s

    /// Returns fallback if the string is null or whitespace; the input string otherwise.
    let inline ifWhiteSpace fallback s = if isEmpty s then fallback else s

    /// Applies f to the string if it is not empty or null, otherwise returns the same string.
    let inline ifNotEmpty f s = if s |> isEmpty then s else f s

    /// Applies f to the string if it is not whitespace or null, otherwise returns the same string.
    let inline ifNotWhiteSpace f s = if s |> isWhiteSpace then s else f s

    let inline contains value (s: string) = s.Contains(value)
    let inline containsIgnoreCase value (s: string) = s.IndexOf(value, StringComparison.CurrentCultureIgnoreCase) >= 0
    let inline startsWith value (s: string) = s.StartsWith(value)
    let inline startsWithIgnoreCase value (s: string) = s.StartsWith(value, StringComparison.CurrentCultureIgnoreCase)
    let inline endsWith value (s: string) = s.EndsWith(value)
    let inline endsWithIgnoreCase value (s: string) = s.EndsWith(value, StringComparison.CurrentCultureIgnoreCase)
    let inline indexOf (value: string) (s: string) = s.IndexOf(value)
    let inline lastIndexOf (value: string) (s: string) = s.LastIndexOf(value)
    let inline equalsIgnoreCase (s1: string) (s2: string) = s1.Equals(s2, StringComparison.CurrentCultureIgnoreCase)

    let inline toLower (s: string) = s.ToLower()
    let inline toUpper (s: string) = s.ToUpper()
    let inline trim (s: string) = s.Trim()
    let inline trimChars chars (s: string) = s.Trim(chars)
    let inline trimStart chars (s: string) = s.TrimStart(chars)
    let inline trimEnd chars (s: string) = s.TrimEnd(chars)
    let inline insert i value (s: string) = s.Insert(i, value)
    let inline replace (oldValue: string) newValue (str: string) = str.Replace(oldValue, newValue)
    let inline padLeft width c (s: string) = s.PadLeft(width, c)
    let inline padRight width c (s: string) = s.PadRight(width, c)
    let inline substring start length (s: string) = s.Substring(start, length)
    let inline substringFrom start (s: string) = s.Substring(start)

    /// Splits the string on the given character, removing empty entries.
    let inline split (splitChar: char) (s: string) =
        s.Split([| splitChar |], StringSplitOptions.RemoveEmptyEntries)

    /// Converts the object to string with the given formatting string.
    let inline format fmt (x: 'a) = String.Format(sprintf "{0:%s}" fmt, x)

module Tuple =
    let pack2 a b = (a, b)
    let pack3 a b c = (a, b, c)
    let pack4 a b c d = (a, b, c, d)

    let swap (a, b) = (b, a)

module Seq =
    /// Returns true if the sequence contains any elements; false otherwise.
    let inline isNotEmpty s = not <| Seq.isEmpty s

    /// Returns fallback if the sequence is empty; the input sequence otherwise.
    let inline ifEmpty fallback s = if Seq.isEmpty s then fallback else s

    /// Returns true if the sequence has an element equal to the value. Equivalent to `flip Seq.contains`.
    let inline containedIn sequence value = Seq.contains value sequence

    /// Returns Some (maximum value) if the sequence is not empty; otherwise returns None.
    let inline tryMax s = if Seq.isEmpty s then None else Some <| Seq.max s
    /// Returns Some (maximum value) if the sequence is not empty; otherwise returns None.
    let inline tryMaxBy projection s = if Seq.isEmpty s then None else Some <| Seq.maxBy projection s

module Option =
    /// Returns Some value when `predicate value` is true; otherwise returns None.
    let inline ofCond predicate value = Some value |> Option.filter predicate

    /// Returns Some s when s is not whitespace; otherwise returns None.
    let inline ofString s = if String.isWhiteSpace s then None else Some s
    /// Returns empty string when input is None; otherwise returns the Some value of the input.
    let inline toString s = Option.defaultValue "" s

    /// Executes fn on the values of the options if both options are Some; otherwise does nothing.
    let inline iter2 fn a b =
        match a, b with
        | Some x, Some y -> fn x y
        | _, _ -> ()

    /// Executes fn on the values of the options if all options are Some; otherwise does nothing.
    let inline iter3 fn a b c =
        match a, b, c with
        | Some x, Some y, Some z -> fn x y z
        | _ -> ()

    /// Like `Option.ofObj` but works on types that are normally not nullable, such as record types.
    let inline ofObjForce (r: 'a) =
        r |> box |> Option.ofObj |> Option.map unbox<'a>


module Result =
    /// Returns true if res is an Ok value; false otherwise.
    let inline isOk res =
        match res with
        | Ok _ -> true
        | Error _ -> false

    /// Returns true if res is an Error value; false otherwise.
    let inline isError res = not <| isOk res

    /// Returns `Error err` if res is Ok v and `predicate v` returns false; otherwise returns res.
    let inline okIf predicate err res =
        res |> Result.bind (fun v -> if predicate v then Ok v else Error err)

    /// Returns `Error err` if res is Ok v and `predicate v` returns true; otherwise returns res.
    let inline errorIf predicate err res =
        okIf (not << predicate) err res

    /// Returns `Ok opt` if opt is None or if it is Some and `predicate opt.Value` is true; otherwise returns `Error err`.
    let inline okIfNoneOr predicate err opt =
        match opt with
        | Some v when not <| predicate v -> Error err
        | o -> Ok o

    /// Returns `Ok value` if `predicate value` is true; otherwise returns `Error err`.
    let inline ofCond predicate err value = Ok value |> okIf predicate err

    /// Returns `Ok opt.Value` if opt is Some; otherwise returns `Error err`.
    let inline ofOption err opt =
        match opt with
        | Some x -> Ok x
        | None -> Error err

    /// Returns `Ok input.Value` if input is not null; otherwise returns `Error err`.
    let inline ofNullable err input = input |> Option.ofNullable |> ofOption err
    /// Returns `Ok input` if input is not null; otherwise returns `Error err`.
    let inline ofObj err input = input |> Option.ofObj |> ofOption err

    /// Returns `Ok input` if input matches the given regular expression pattern; otherwise returns `Error err`.
    let inline ofRegex pattern err input =
        if System.Text.RegularExpressions.Regex.IsMatch(input, pattern) then
            Ok input
        else Error err

    /// If res is `Ok v`, returns `Some v`; otherwise returns None.
    let inline toOption res =
        match res with
        | Ok x -> Some x
        | Error _ -> None

    /// If res is `Error e`, returns `Some e`; otherwise returns None.
    let inline toErrorOption res =
        match res with
        | Error e -> Some e
        | Ok _ -> None

    /// Accumulates a sequence of Results into a single Result.
    /// Returns Error of the list of errors if at least one Error is present; otherwise returns Ok of the list of Ok values.
    let accumulate rs =
        let folder r state =
            match r, state with
            | Ok v, Ok vs -> Ok (v :: vs)
            | Ok _, Error es -> Error es
            | Error e, Ok _ -> Error [e]
            | Error e, Error es -> Error (e :: es)
        Seq.foldBack folder rs (Ok [])

    /// Partitions a sequence of Results into a list of the Ok values and a list of the Error values.
    let partition rs =
        let folder r (oks, errors) =
            match r with
            | Ok o -> (o :: oks, errors)
            | Error e -> (oks, e :: errors)
        Seq.foldBack folder rs ([], [])

    /// Performs an action on the Ok value
    let inline iter (action: 'a -> unit) res =
        match res with
        | Ok v -> v |> action
        | _ -> ()

    /// Performs an action on the Error value
    let inline iterError (action: 'b -> unit) res =
        match res with
        | Error e -> e |> action
        | _ -> ()

module Async =
    /// Apply an asynchronous transforming function to the result of an asynchronous computation.
    let inline bind f a = async {
        let! x = a
        return! f x
    }

    /// Apply a transforming function to the result of an asynchronous computation.
    let inline map f a = async {
        let! x = a
        return f x
    }

    /// Apply an asynchronous transforming function to the result of an asynchronous computation.
    /// Equivalent to `bind`.
    let inline mapAsync f a = bind f a

    /// Applies a side-effect function to the result of an async computation and returns the async.
    let inline tee f a = map (fun x -> f x; x) a

    // Return an asynchronous computation that will wait for the given task to complete.
    let inline AwaitPlainTask (task: System.Threading.Tasks.Task) =
        task |> Async.AwaitIAsyncResult |> Async.Ignore

module Reflection =
    open FSharp.Reflection

    /// Given an instance of a union case, returns the name of the union case.
    let unionCaseName (x: 'a) =
        let case, _ = FSharpValue.GetUnionFields(x, typedefof<'a>)
        case.Name

    /// Parse a string into a value-less union case by name (case insensitive).
    let parseUnionCase<'a> name =
        FSharpType.GetUnionCases typeof<'a>
        |> Array.tryFind (fun c -> String.equalsIgnoreCase c.Name name && c.GetFields().Length = 0)
        |> Option.map (fun c -> FSharpValue.MakeUnion(c, [||]) :?> 'a)

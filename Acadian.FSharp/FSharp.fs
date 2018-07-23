module Acadian.FSharp

/// Returns a function that returns the given constant value. `(fun _ -> a)`
let cnst a = (fun _ -> a)

/// Flips the order of two arguments of a function. `flip f a b = f b a`
let flip f a b = f b a

/// Returns Some if the object is the given type; otherwise returns None.
let tryCast<'a> (o: obj) =
    match o with
    | :? 'a as a -> Some a
    | _ -> None

/// The null System.Nullable value.
let nil = System.Nullable()

/// defaultArg operator, equivalent to `|> Option.defaultValue`
let (|?) = defaultArg

module Parse =
    let private tryToOption (s, v) = if s then Some v else None

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

    let contains value (s: string) = s.Contains(value)
    let startsWith value (s: string) = s.StartsWith(value)
    let endsWith value (s: string) = s.EndsWith(value)
    let indexOf (value: string) (s: string) = s.IndexOf(value)
    let lastIndexOf (value: string) (s: string) = s.LastIndexOf(value)
    let equalsIgnoreCase (s1: string) (s2: string) = s1.Equals(s2, StringComparison.CurrentCultureIgnoreCase)

    let toLower (s: string) = s.ToLower()
    let toUpper (s: string) = s.ToUpper()
    let trim (s: string) = s.Trim()
    let insert i value (s: string) = s.Insert(i, value)
    let replace (oldValue: string) newValue (str: string) = str.Replace(oldValue, newValue)
    let padLeft width c (s: string) = s.PadLeft(width, c)
    let padRight width c (s: string) = s.PadRight(width, c)
    let substring start length (s: string) = s.Substring(start, length)
    let substringFrom start (s: string) = s.Substring(start)

    /// Splits the string on the given character, removing empty entries.
    let split (splitChar: char) (s: string) =
        s.Split([| splitChar |], System.StringSplitOptions.RemoveEmptyEntries)

    /// Converts the object to string with the given formatting string.
    let format fmt (x: 'a) = String.Format(sprintf "{0:%s}" fmt, x)

module Tuple =
    let pack2 a b = (a, b)
    let pack3 a b c = (a, b, c)
    let pack4 a b c d = (a, b, c, d)

module Seq =
    /// Returns true if the sequence contains any elements; false otherwise
    let isNotEmpty s = not <| Seq.isEmpty s

    /// Returns true if the sequence has an element equal to the value. Equivalent to `flip Seq.contains`.
    let containedIn sequence value = Seq.contains value sequence

    /// Returns Some <maximum value> if the sequence is not empty; otherwise returns None.
    let tryMax s = if Seq.isEmpty s then None else Some <| Seq.max s
    /// Returns Some <maximum value> if the sequence is not empty; otherwise returns None.
    let tryMaxBy projection s = if Seq.isEmpty s then None else Some <| Seq.maxBy projection s

module Option =
    /// Returns Some value when `predicate value` is true; otherwise returns None.
    let ofCond predicate value = Some value |> Option.filter predicate

    /// Returns Some s when s is not whitespace; otherwise returns None.
    let ofString s = if String.isWhiteSpace s then None else Some s
    /// Returns empty string when input is None; otherwise returns the Some value of the input.
    let toString = Option.defaultValue ""

    /// Executes fn on the values of the options if both options are Some; otherwise does nothing.
    let iter2 fn a b =
        match a, b with
        | Some x, Some y -> fn x y
        | _, _ -> ()

    /// Executes fn on the values of the options if all options are Some; otherwise does nothing.
    let iter3 fn a b c =
        match a, b, c with
        | Some x, Some y, Some z -> fn x y z
        | _ -> ()

type OptionBuilder() =
    member this.Bind (x, f) = Option.bind f x
    member this.Return x = Some x
    member this.ReturnFrom (x: Option<_>) = x
    member this.Zero () = None
    member this.Delay f = f
    member this.Run f = f ()
    member this.Combine (x, f) = Option.orElseWith f x

    member this.Using (disposable: #System.IDisposable, body) =
        try body disposable
        finally disposable.Dispose()

/// Builds an Option using computation expression syntax.
/// Combine returns the first Some value, so an `if` without an else that returns a Some value will return that value
/// without executing the following code, while a None will return the result of the following code.
let option = OptionBuilder()

module Result =
    /// Returns `Error err` if res is Ok v and `predicate v` returns false; otherwise returns res.
    let okIf predicate err res =
        res |> Result.bind (fun v -> if predicate v then Ok v else Error err)

    /// Returns `Error err` if res is Ok v and `predicate v` returns true; otherwise returns res.
    let errorIf predicate err res =
        okIf (not << predicate) err res

    /// Returns `Ok opt` if opt is None or if it is Some and `predicate opt.Value` is true; otherwise returns `Error err`.
    let okIfNoneOr predicate err opt =
        match opt with
        | Some v when not <| predicate v -> Error err
        | o -> Ok o

    /// Returns `Ok value` if `predicate value` is true; otherwise returns `Error err`.
    let ofCond predicate err value = Ok value |> okIf predicate err

    /// Returns `Ok opt.Value` if opt is Some; otherwise returns `Error err`.
    let ofOption err opt =
        match opt with
        | Some x -> Ok x
        | None -> Error err

    /// Returns `Ok input.Value` if input is not null; otherwise returns `Error err`.
    let ofNullable err input = input |> Option.ofNullable |> ofOption err
    /// Returns `Ok input` if input is not null; otherwise returns `Error err`.
    let ofObj err input = input |> Option.ofObj |> ofOption err

    /// Returns `Ok input` if input matches the given regular expression pattern; otherwise returns `Error err`.
    let ofRegex pattern err input =
        if System.Text.RegularExpressions.Regex.IsMatch(input, pattern) then
            Ok input
        else Error err

    /// If res is `Ok v`, returns `Some v`; otherwise returns None.
    let toOption res =
        match res with
        | Ok x -> Some x
        | Error _ -> None

type ResultBuilder() =
    member this.Bind (x, f) = Result.bind f x
    member this.Return x = Ok x
    member this.ReturnFrom (x: Result<_,_>) = x
    member this.Zero () = Ok ()
    member this.Delay f = f
    member this.Run f = f ()

    member this.Combine (x, f) =
        match x with
        | Ok () -> f ()
        | Error e -> Error e

    member this.Using (disposable: #System.IDisposable, body) =
        try body disposable
        finally disposable.Dispose()

/// Builds a Result using computation expression syntax.
/// Combine returns upon encountering an Error value, so an `if` without an else that returns an Error value will return
/// that value without executing the following code, while an Ok () will return the result of the following code.
let result = ResultBuilder()

module Reflection =
    /// Given an instance of a union case, returns the name of the union case.
    let unionCaseName (x: 'a) =
        let case, _ = Reflection.FSharpValue.GetUnionFields(x, typedefof<'a>)
        case.Name

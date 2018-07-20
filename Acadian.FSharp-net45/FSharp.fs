module Acadian.FSharp

let cnst a = (fun _ -> a)
let flip f a b = f b a

let nil = System.Nullable()

let (|?) = defaultArg

module Parse =
    let private tryToOption (s, v) = if s then Some v else None

    let Int s = System.Int32.TryParse(s) |> tryToOption
    let Double s = System.Double.TryParse(s) |> tryToOption
    let Decimal s = System.Decimal.TryParse(s) |> tryToOption
    let Date s = System.DateTime.TryParse(s) |> tryToOption

    let IsInt s = System.Int32.TryParse(s) |> fst
    let IsDouble s = System.Double.TryParse(s) |> fst
    let IsDecimal s = System.Decimal.TryParse(s) |> fst
    let IsDate s = System.DateTime.TryParse(s) |> fst

module Patterns =
    let (|Int|_|) = Parse.Int
    let (|Double|_|) = Parse.Double
    let (|Decimal|_|) = Parse.Decimal
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
    let isNotEmpty s = not <| Seq.isEmpty s

    let containedIn s a = Seq.contains a s

    let tryMax s = if Seq.isEmpty s then None else Some <| Seq.max s
    let tryMaxBy f s = if Seq.isEmpty s then None else Some <| Seq.maxBy f s

module Option =
    let ofCond f = Some >> Option.filter f

    let ofString s = if String.isWhiteSpace s then None else Some s
    let toString = Option.defaultValue ""

    let iter2 fn a b =
        match a, b with
        | Some x, Some y -> fn x y
        | _, _ -> ()

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

let option = OptionBuilder()

module Result =
    let okIf pred err =
        Result.bind (fun v -> if pred v then Ok v else Error err)

    let errorIf pred =
        okIf (not << pred)

    let okIfNoneOr pred err = function
        | Some v when not <| pred v -> Error err
        | o -> Ok o

    let ofCond pred err = Ok >> okIf pred err

    let ofOption err opt =
        match opt with
        | Some x -> Ok x
        | None -> Error err

    let ofNullable err = Option.ofNullable >> ofOption err
    let ofObj err = Option.ofObj >> ofOption err

    let ofRegex pattern input err =
        if System.Text.RegularExpressions.Regex.IsMatch(input, pattern) then
            Ok input
        else Error err

    let toOption = function
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

let result = ResultBuilder()

module Reflection =
    let unionCaseName (x: 'a) =
        let case, _ = Reflection.FSharpValue.GetUnionFields(x, typedefof<'a>)
        case.Name

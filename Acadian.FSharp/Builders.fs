namespace Acadian.FSharp

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
    member this.TryWith (body, handler) =
        try this.ReturnFrom (body ())
        with e -> handler e
    member this.TryFinally (body, compensation) =
        try this.ReturnFrom (body ())
        finally compensation ()

type AsyncOptionBuilder() =
    member this.Bind (a, f) = async.Bind(a, f)
    member this.Bind (o, f) = async {
        match o with
        | Some x -> return! f x
        | None -> return None
    }
    member this.Return x = x |> Some |> async.Return
    member this.Return (_: unit) = this.Zero ()
    member this.ReturnFrom (x: Option<_>) = x |> async.Return
    member this.ReturnFrom (x: Async<Option<_>>) = async.ReturnFrom x
    member this.Zero () = None |> async.Return
    member this.Delay f = async.Delay f
    member this.Combine (x, y) = async {
        match! x with
        | Some a -> return Some a
        | None -> return! y
    }
    member this.Using (x, f) = async.Using (x, f)
    member this.TryWith (body: unit -> Async<Option<_>>, handler) =
        try this.ReturnFrom (body ())
        with e -> handler e
    member this.TryFinally (body: unit -> Async<Option<_>>, compensation) =
        try this.ReturnFrom (body ())
        finally compensation ()

type ResultBuilder() =
    member this.Bind (x, f) = Result.bind f x
    member this.Return x = Ok x
    member this.ReturnFrom (x: Result<_,_>) = x
    member this.Zero () = Ok ()
    member this.Delay f = f
    member this.Run f = f ()
    member this.Combine (x: Result<unit, _>, f) = Result.bind f x
    member this.Using (disposable: #System.IDisposable, body) =
        try body disposable
        finally disposable.Dispose()
    member this.TryWith (body, handler) =
        try this.ReturnFrom (body ())
        with e -> handler e
    member this.TryFinally (body, compensation) =
        try this.ReturnFrom (body ())
        finally compensation ()

type AsyncResultBuilder() =
    member this.Bind (a, f) = async.Bind(a, f)
    member this.Bind (r, f) = async {
        match r with
        | Ok x -> return! f x
        | Error e -> return Error e
    }
    member this.Return x = x |> Ok |> async.Return
    member this.Return (_: unit) = this.Zero ()
    member this.ReturnFrom (x: Result<_,_>) = ResultBuilder().ReturnFrom x |> async.Return
    member this.ReturnFrom (x: Async<Result<_,_>>) = async.ReturnFrom x
    member this.Zero () = Ok () |> async.Return
    member this.Delay f = async.Delay f
    member this.Combine (x, y) = async {
        match! x with
        | Ok () -> return! y
        | Error e -> return Error e
    }
    member this.Using (x, f) = async.Using (x, f)
    member this.TryWith (body: unit -> Async<Result<_,_>>, handler) =
        try this.ReturnFrom (body ())
        with e -> handler e
    member this.TryFinally (body: unit -> Async<Result<_,_>>, compensation) =
        try this.ReturnFrom (body ())
        finally compensation ()

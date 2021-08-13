module BuilderTests

open Expecto
open Expecto.Flip
open Acadian.FSharp

[<Tests>]
let tests = testList "CE Builder Tests" [
    testList "ResultBuilder" [
        test "Bind in For short-circuits on Error values" {
            let results = [Ok 1; Error "oops"; Ok 2]
            let mutable seen = []
            let res =
                result {
                    for r in results do
                        let! num = r
                        seen <- num :: seen
                }
            res |> Expect.equal "CE should return error value" (Error "oops")
            seen |> Expect.equal "CE should not continue after getting error" [1]
        }
    ]
    testList "AsyncResultBuilder" [
        testAsync "Bind in For short-circuits on Error values" {
            let results =
                [Ok 1; Error "oops"; Ok 2]
                |> List.map (fun r -> async { return r })
            let mutable seen = []
            let! res =
                asyncResult {
                    for asyncRes in results do
                        let! res = asyncRes
                        let! num = res
                        seen <- num :: seen
                }
            res |> Expect.equal "CE should return error value" (Error "oops")
            seen |> Expect.equal "CE should not continue after getting error" [1]
        }
    ]
]
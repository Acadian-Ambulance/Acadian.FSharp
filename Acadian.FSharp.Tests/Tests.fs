module Tests

open Expecto
open Expecto.Flip
open Acadian.FSharp

[<Tests>]
let tests = testList "Tests" [
  testList "Result.accumulate" [
    test "Empty list returns Ok []" {
      Result.accumulate []
      |> Expect.equal "" (Ok [])
    }
    test "Ok values return Ok list" {
      Result.accumulate [
        Ok 1
        Ok 2
      ] |> Expect.equal "" (Ok [1; 2])
    }
    test "Error values return Error list" {
      Result.accumulate [
        Error 1
        Error 2
      ] |> Expect.equal "" (Error [1; 2])
    }
    test "Mixed values return Error list" {
      Result.accumulate [
        Ok 1
        Error 2
        Ok 3
        Error 4
      ] |> Expect.equal "" (Error [2; 4])
    }
  ]

  testList "Result.partition" [
    test "Empty list returns empty lists" {
      Result.partition []
      |> Expect.equal "" ([], [])
    }
    test "Ok values return Ok list and empty list" {
      Result.partition [
        Ok 1
        Ok 2
      ]
      |> Expect.equal "" ([1; 2], [])
    }
    test "Error values return empty list and Error list" {
      Result.partition [
        Error 1
        Error 2
      ]
      |> Expect.equal "" ([], [1; 2])
    }
    test "Mixed values return Ok list and Error list" {
      Result.partition [
        Ok 1
        Error 2
        Ok 3
        Error 4
      ]
      |> Expect.equal "" ([1; 3], [2; 4])
    }
  ]

  testList "Result.iter" [
    test "Should gather Ok values into a list through side effects" {
      let resList = [Ok 1; Error 2 ; Ok 3]
      let mutable l: int list = []
      resList
      |> List.iter (Result.iter (fun o -> l <- o::l))
      l |> Expect.equal "" [3; 1]
    }
  ]

  testList "Result.iterError" [
    test "Should gather Ok values into a list through side effects" {
      let resList = [Ok 1; Error 2 ; Ok 3]
      let mutable l: int list = []
      resList
      |> List.iter (Result.iterError (fun o -> l <- o::l))
      l |> Expect.equal "" [2]
    }
  ]
]

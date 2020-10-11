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
]

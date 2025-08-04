module TestRun

open System

open NUnit.Framework


let runATest (fn: unit -> Result<bool,string>) =
    try 
        fn()
    with | ex -> Error $"Failed {nameof fn} {ex.Message} {ex.StackTrace}"


let myfunc () =
    let a = 1
    let b = 2
    raise (Exception "error")
    
    
    
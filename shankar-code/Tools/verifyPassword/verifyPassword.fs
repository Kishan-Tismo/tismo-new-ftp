
// Verifies a password given the hash, salt and the plaintext
open System
open Passwords

let printColor color (s:string) =
    Console.ForegroundColor <- color
    printfn $"{s}"
    Console.ResetColor()
        
let printOk = printColor ConsoleColor.Green
let printInfo = printColor ConsoleColor.Cyan
let printWarn = printColor ConsoleColor.Yellow
let printError = printColor ConsoleColor.Red


let printUsage()=
    printOk "Usage: verifyPassword hash salt plainTextPassword "
    printOk "\nCopyright (c) 2023 Tismo Technology Solutions (P) Ltd."
    Environment.Exit -1

let main () =
     printfn $"Verifies a given plain text password, along with its hash and salt."
     let args = Environment.GetCommandLineArgs() |> Array.toList
     match args with
        | _::hash::salt::[plainTextpwd] ->
               if VerifyHashPassword hash salt plainTextpwd then
                   printOk "Password matches its hash and salt."
               else
                   printError "Password does NOT match its hash and salt."
        | _ -> printUsage()
               Environment.Exit 2
               
    
main()             

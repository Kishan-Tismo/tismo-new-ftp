// Creates passwords for both client and Tismo
// generates the appsettings.production.json file


// #r "nuget: Microsoft.AspNetCore.Cryptography.KeyDerivation, 6.0.3"

open System
open System.IO
open System.Text.Json
open Passwords
open settings

type command =
     | Encrypt of string
     | Decrypt of string * string * string
     | None

let printColor color (s:string) =
    Console.ForegroundColor <- color
    printfn $"{s}"
    Console.ResetColor()
        
let printOk = printColor ConsoleColor.Green
let printInfo = printColor ConsoleColor.Cyan
let printWarn = printColor ConsoleColor.Yellow
let printError = printColor ConsoleColor.Red

let printUsage()  =   
     printOk $"Creates passwords for both client and tismo. Puts the salts and hashes in appsettings.production.json file."
     printOk $"Usage: createPasswords.fsx  tcaXXX appsettings.production.json "
     printOk "\nCopyright (c) 2022 Tismo Technology Solutions (P) Ltd."


let checkIfOutputFileExists outputFile =
    if File.Exists (outputFile) then
        printError $"File {outputFile} already exists."
        printError $"To recreate passwords, delete it and run this again."
        Environment.Exit(2)
    

let readAllArgs () =
     let args = Environment.GetCommandLineArgs() |> Array.toList
     match args with
        | _::customercode::[ outputFile ] -> customercode, outputFile
        | _ -> printUsage()
               Environment.Exit 2
               String.Empty,String.Empty


let createSettings customercode =
    let client = "client"
    let clientPassword = createPlainTextPassword()
    let tismo = "tismo"
    let tismoPassword = createPlainTextPassword()
    let clientHash, clientSalt = HashPassword clientPassword
    let tismoHash, tismoSalt = HashPassword tismoPassword
    let settings = {
        CustomerCode = customercode
        ClientUserName  = client 
        ClientHashedPassword = clientHash
        ClientSalt = clientSalt
        TismoUserName = tismo 
        TismoHashedPassword = tismoHash
        TismoSalt = tismoSalt
    }
    clientPassword, tismoPassword, settings
    

let settingsToJson settings =
    let jso = JsonSerializerOptions();
    jso.Encoder <- System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    let jsonstr = $"{JsonSerializer.Serialize(settings, jso)}"
    let contents = $"{{  \"Settings\" : {jsonstr}  }}"
                        .Replace("{", "{\n")
                        .Replace("}", "\n}")
                        .Replace(",", ",\n")
    contents
       
let main() =
    eprintfn $"Create Passwords for Tismo File Transfer"
    let customercode, outputFile = readAllArgs()
    checkIfOutputFileExists outputFile
    let clientPassword, tismoPassword, settings = createSettings customercode
    let contents = settingsToJson settings
    
    File.WriteAllText(outputFile, contents)
    printfn   $"Created {outputFile}"
    printWarn $"Please save this, it can be changed but not retrieved later."
    printWarn $"Please do not save this in this machine."
    printInfo $"Client username -  {settings.ClientUserName}"
    printInfo $"Client password -  {clientPassword}"
    printInfo $"Tismo username -   {settings.TismoUserName}"
    printInfo $"Tismo password -   {tismoPassword}"
    
main()             

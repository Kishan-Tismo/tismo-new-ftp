
// Creates passwords for both client and Tismo
// generates the appsettings.production.json file


// #r "nuget: Microsoft.AspNetCore.Cryptography.KeyDerivation, 6.0.3"

open System
open System.IO

open System.Text.Json

open FSharp.Data
open Passwords
open settings

type plainText = string

[<Literal>]
let sample = """
{
    "Settings" : {
        "CustomerCode":"tca035",
        "ClientUserName":"client",
        "ClientHashedPassword":"hs7zmqO37wtSuQytdxuASdO1RVpyAbGjy2H4Yn7EqYE=",
        "ClientSalt":"TG5ibf0mxAdnzMzL9HrlAA==",
        "TismoUserName":"tismo",
        "TismoHashedPassword":"YxUO1+B7Su6hmwLZdXuh+iLkCuFZKRJOR2E3eeUyykk=",
        "TismoSalt":"oGUVg/Kmp5o+CdlHyvywuw=="
    }  
}
"""

type ReadSettings = JsonProvider<sample>
let clientUserName = "client"
let tismoUserName = "tismo"

type Password = {
    userName : string
    plainTextPwd: string
    hash: string
    salt: string 
}    

let private readSettingsFile (fileNameArg:string) =
    let fileName = Path.GetFullPath fileNameArg // ReadSettings.Load needs a proper full path otherwise it opens the wrong file!
    if File.Exists fileName |> not then
        eprintfn $"File not found {fileName}"
        Environment.Exit -3 
    let fs = (ReadSettings.Load fileName).Settings
    {
        CustomerCode = fs.CustomerCode
        ClientUserName = fs.ClientUserName
        ClientHashedPassword = fs.ClientHashedPassword
        ClientSalt = fs.ClientSalt
        TismoUserName = fs.TismoUserName
        TismoHashedPassword = fs.TismoHashedPassword
        TismoSalt = fs.TismoSalt        
    }

let printColor color (s:string) =
    Console.ForegroundColor <- color
    printfn $"{s}"
    Console.ResetColor()
        
let printOk = printColor ConsoleColor.Green
let printInfo = printColor ConsoleColor.Cyan
let printWarn = printColor ConsoleColor.Yellow
let printError = printColor ConsoleColor.Red

let printUsage()  =   
     printOk $"Changes passwords for client or tismo or both. Puts the salts and hashes in appsettings.production.json file."
     printOk $"Usage: changePasswords client | tismo | both  appsettings.production.json "
     printOk "\nCopyright (c) 2023 Tismo Technology Solutions (P) Ltd."


let checkIfOutputFileExists outputFile =
    if File.Exists outputFile then
        printError $"File {outputFile} already exists."
        printError $"To recreate passwords, delete it and run this again."
        Environment.Exit(2)
    
   
    
let createNewPassword userName =
    let plainTextPwd = createPlainTextPassword()
    let hash, salt = HashPassword plainTextPwd
    { userName = userName; plainTextPwd = plainTextPwd; hash = hash; salt= salt }
    
     
let updateSettingsWithClientPwd clientPwd (oldSettings:Settings) =   
    {
      oldSettings with ClientUserName = clientPwd.userName
                       ClientSalt = clientPwd.salt
                       ClientHashedPassword = clientPwd.hash
    }
    
let updateSettingsWithTismoPwd tismoPwd (oldSettings:Settings) =   
    {
        oldSettings with TismoUserName = tismoPwd.userName
                         TismoSalt = tismoPwd.salt
                         TismoHashedPassword = tismoPwd.hash
    }
     
    
let settingsToJson (settings:Settings) =
    let jso = JsonSerializerOptions();
    jso.Encoder <- System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    let jsonstr = $"{JsonSerializer.Serialize(settings, jso)}"
    let contents = $"{{  \"Settings\" : {jsonstr}  }}"
                        .Replace("{", "{\n")
                        .Replace("}", "\n}")
                        .Replace(",", ",\n")
    contents    
    
    
let saveCopyOfOldFile (oldFileName:string) =    
    let oldFileCopy = $"old_{genPwd 4}_{Path.GetFileName oldFileName}"    
    File.Copy (oldFileName, oldFileCopy, true)
    oldFileCopy
    
let updateFile newSettings fileName =
    let oldFileCopy = saveCopyOfOldFile fileName
    let jsonStr = settingsToJson newSettings
    File.WriteAllText(fileName, jsonStr)
    printfn   $"Updated {fileName}. Old settings file copied to {oldFileCopy}"
    oldFileCopy
    
let updateSettingsWithNewClientPassworld oldSettings =    
    let clientPwd = createNewPassword clientUserName
    let newSettings = updateSettingsWithClientPwd clientPwd oldSettings   
    newSettings, clientPwd
    
let updateSettingsWithNewTismoPassworld oldSettings =  
    let tismoPwd = createNewPassword tismoUserName
    let newSettings = updateSettingsWithTismoPwd tismoPwd oldSettings      
    newSettings, tismoPwd
    

let printSaveMsg() =
     printWarn $"Please save this, it can be changed but not retrieved later."
     printWarn $"Please do not save this in this machine."
    
    
let printClientPwd settings pwd =
    printInfo $"Client username -  {settings.ClientUserName}"
    printInfo $"Client password -  {pwd.plainTextPwd}"
    
let printTismoPwd settings pwd =
     printInfo $"Tismo username -  {settings.TismoUserName}"
     printInfo $"Tismo password -  {pwd.plainTextPwd}"
    
let printSavedFileInfo appSettingsFile oldFileCopy =
    printInfo $"Updated {appSettingsFile}. Old settings copied to {oldFileCopy}"
    printInfo $"If you want to discard these changes, then simply copy {oldFileCopy} to {appSettingsFile}"
    printInfo $"Restart docker web app for these changes to take effect."

let main () =
     eprintfn $"Change Passwords for Tismo File Transfer"
     let args = Environment.GetCommandLineArgs() |> Array.toList
     match args with
        | _::"client"::[appSettingsFile] ->
                    
                    let oldSettings = readSettingsFile appSettingsFile
                    let newSettings, pwd = updateSettingsWithNewClientPassworld oldSettings
                    let oldFileCopy = updateFile newSettings appSettingsFile
                    printSaveMsg()
                    printClientPwd newSettings pwd                    
                    printSavedFileInfo appSettingsFile oldFileCopy
        | _::"tismo"::[appSettingsFile] ->
                    
                    let oldSettings = readSettingsFile appSettingsFile
                    let newSettings, pwd = updateSettingsWithNewTismoPassworld oldSettings
                    let oldFileCopy = updateFile newSettings appSettingsFile
                    printSaveMsg()
                    printTismoPwd newSettings pwd                   
                    printSavedFileInfo appSettingsFile oldFileCopy
        | _::"both"::[appSettingsFile] ->
                    
                    let oldSettings = readSettingsFile appSettingsFile
                    let newSettings1, clientPwd = updateSettingsWithNewClientPassworld oldSettings
                    let newSettings, tismoPwd = updateSettingsWithNewTismoPassworld newSettings1
                    let oldFileCopy = updateFile newSettings appSettingsFile
                    printSaveMsg()                    
                    printClientPwd newSettings clientPwd
                    printTismoPwd newSettings tismoPwd                       
                    printSavedFileInfo appSettingsFile oldFileCopy
        | _ -> printUsage()
               Environment.Exit 2
               
    
main()             

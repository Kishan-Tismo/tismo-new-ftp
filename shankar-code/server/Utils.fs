module server.Utils

open System.IO
open Microsoft.Extensions.Configuration
open Types


let version = "Dated: 10 Jun 2023"

// Build a config object, using env vars and JSON providers.
let  config = ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true ) // optional
                .AddJsonFile("appsettings.development.json", true ) // optional
                .AddJsonFile("appsettings.staging.json", true) // optional
                .AddJsonFile("appsettings.production.json", true) // optional 
                .AddEnvironmentVariables()
                .Build()

// From Settings
let settings = config.GetRequiredSection("Settings").Get<Settings>()
let customerCode = settings.CustomerCode

let tempFolder = Path.Join (settings.FileFolder, $"___temp__{settings.CustomerCode}")


// Helper methods for Logger
let initLogging() =        
    let logMethods = Logger.getLogger settings.LogFile
    Logger.log <- logMethods.logFunc
    Logger.log "Application Starting...."

// // Helper methods for FileMapper
// let private fileNameForMapStorage = Path.Combine(settings.FileFolder, "TismoFileServerMap.csv")  
// let private fileMapper = FileMapper.createFileMapper fileNameForMapStorage
// let addFileItem userFile guid =
//     FileMapper.addToFileMap fileMapper userFile guid
//     
// let getFileMap() =
//     FileMapper.getFileMap fileMapper
//     
// let deleteFileItem userFile =
//     FileMapper.removeFromFileMap fileMapper userFile 
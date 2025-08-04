module Logger 
open System
open System.IO
open System.IO.Compression

// Logger
//
// Logs to a specified file with path - e.g. ./logs/ftpSync.txt
// Everday the this file is zipped and placed as   ./logs/2022_03_19_01_15_ftpSync.zip
//                                                        date time of creation of the original txt file
// The current logging wil continue on ./logs/ftpSync.txt
// The main program is expected to call to check if log rotation is due regularly

// MailBox is used to implement the log
// The log is sent to the MailBox which is queued so that log is fast
// Later from this queue, the message is extracted, timestamp is added and appended to the log file



type  LogMsg =
    | LogCmd of string // log the message
    | CloseLog of AsyncReplyChannel<unit> // close and exit
    | Rollover  // for log file rotation
    
let mutable (log:string -> unit) = // gets overwritten later
    fun _ -> ()
    
    
// Opens the filestream for the log file    
let private newLogFileStream (logFileName:string) =
    try
        let log_path = Path.GetDirectoryName logFileName
        if not (Directory.Exists(log_path)) then Directory.CreateDirectory(log_path) |> ignore
        let str = File.AppendText logFileName  //new StreamWriter(File.OpenWrite(logFileName()))
        str.AutoFlush <- true
        Some str
    with ex -> printfn $"Error opening log {logFileName} {ex.Message}"; None


let private closeFileStream  (fileStreamOpt:StreamWriter option) =
    try
       fileStreamOpt |> Option.map (fun stream -> stream.Close()) |> ignore
    with ex -> printfn $"Error closing file stream {ex.Message}"
    
    
    
// the current log file would be zipped to this zip file name    
let private getZipFileName (oldLogFileName:string) =   // ./data/Logs/ftpSync.txt
    let creationTime = File.GetCreationTime oldLogFileName 
    let creationTimeStr = creationTime.ToString("yyyy_MM_dd_HH_mm")  // 22_03_14_07_05
    let newFileName =  $"{creationTimeStr}_{Path.GetFileName oldLogFileName}" // 22_03_14_07_05_ftpSync.txt
    let oldLogPath = Path.GetDirectoryName oldLogFileName   // ./data/Logs
    let zipFileName = Path.Combine(oldLogPath, Path.ChangeExtension( newFileName, ".zip")) // ./data/Logs/22_03_14_07_05_ftpSync.zip
    newFileName, zipFileName

// zips up the old log file and then deletes the old log file
let doRotateLogFile oldLogFileName =
    try 
        let newFileName, zipFileName = getZipFileName oldLogFileName
        printfn $"doRotateLogFile {oldLogFileName} to {zipFileName}"
        use zip = ZipFile.Open(zipFileName, ZipArchiveMode.Create)
        zip.CreateEntryFromFile(oldLogFileName, newFileName ) |> ignore
        printfn $"Zipped the current logs to {zipFileName} "   
        File.Delete oldLogFileName
        zipFileName
    with ex -> printfn $"Error : rotate log file {oldLogFileName}"
               ""
    
let private doAppendToLogFile (fileStreamOpt:StreamWriter option) (lineOfText:string) =
     try
        fileStreamOpt |> Option.map (fun stream ->
                                                stream.WriteLine lineOfText
                                                stream.Flush
                                      ) |> ignore                                                                  
     with ex -> printfn $"Error: appending to logfile {ex.Message}"
   
let private createLogger filename = 
    MailboxProcessor<LogMsg>.Start(fun inbox ->
        let rec loop (fileStreamOpt:StreamWriter option) =
            async { 
                let! logMsg = inbox.Receive()
                let fs = match logMsg with
                            | LogCmd lineOfText ->
                                doAppendToLogFile fileStreamOpt lineOfText
                                fileStreamOpt // continue with the same file 
                            | Rollover ->
                                    closeFileStream fileStreamOpt
                                    doRotateLogFile filename |> ignore  // zip the contents of the log file 
                                    newLogFileStream filename // open the same file again                                                  
                            | CloseLog chan ->                            
                                    closeFileStream fileStreamOpt
                                    chan.Reply()
                                    None
                return! loop fs
            }
        loop <| newLogFileStream filename
    )    
//let private createLogger' filename = 
//    MailboxProcessor<LogMsg>.Start(fun inbox ->
//        let mutable (fileStreamOpt:StreamWriter option)  = newLogFileStream filename
//        let rec loop () =
//            async { 
//                let! logMsg = inbox.Receive()
//                match logMsg with
//                    | LogCmd lineOfText -> doAppendToLogFile fileStreamOpt lineOfText                       
//                    | Rollover ->
//                            fileStreamOpt <- closeFileStream fileStreamOpt
//                            doRotateLogFile filename |> ignore  // zip the contents of the log file 
//                            fileStreamOpt <- newLogFileStream filename // open the same file again                                                  
//                    | CloseLog chan ->                            
//                            fileStreamOpt <- closeFileStream fileStreamOpt
//                            chan.Reply()                                                
//                return! loop()
//            }
//        loop () 
//    )
    
// checks if the current log file was created yesterday or earlier
// if it was, then do the rollover of the logs    
let private CheckRotateLogfile (logger:MailboxProcessor<LogMsg>) oldLogFileName =
    let notCreatedToday =
        File.Exists oldLogFileName &&
        (File.GetCreationTime oldLogFileName).Day <> DateTime.Now.Day 
    if notCreatedToday then               
       Rollover |> logger.Post
                               
                               
// Give the main program a set of functions                               
let getLogger filename  =
    let logger = createLogger(filename)
    {|
        logFunc = fun lineOfText -> $"{DateTime.Now} {lineOfText}" |> LogCmd |> logger.Post
        endLog =  fun () -> logger.PostAndReply CloseLog
        rollOver = CheckRotateLogfile logger
    |}
    
    
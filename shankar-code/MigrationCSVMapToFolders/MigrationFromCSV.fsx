#! /usr/bin/dotnet fsi

// The older version of FileTransfer used to store all files
// in the same folder. The filenames were changed to a GUID
// A CSV file contained the mapping between the GUID and the actual filename with full path

// This migration project, migrates this CSV Map based file storage to
// storing in real folders that is suitable for the newer version of fileTransfer app



open System
open System.Diagnostics
open System.IO
open System.Threading.Tasks

type CommandResult = { 
  ExitCode: int; 
  StandardOutput: string;
  StandardError: string 
}

let executeCommand executable args =
  async {
    let startInfo = ProcessStartInfo()
    startInfo.FileName <- executable
    for a in args do
      startInfo.ArgumentList.Add(a)
    startInfo.RedirectStandardOutput <- true
    startInfo.RedirectStandardError <- true
    startInfo.UseShellExecute <- false
    startInfo.CreateNoWindow <- true
    use p = new Process()
    p.StartInfo <- startInfo
    p.Start() |> ignore

    let outTask = Task.WhenAll([|
      p.StandardOutput.ReadToEndAsync();
      p.StandardError.ReadToEndAsync()
    |])

    do! p.WaitForExitAsync() |> Async.AwaitTask
    let! out = outTask |> Async.AwaitTask
    return {
      ExitCode = p.ExitCode;
      StandardOutput = out.[0];
      StandardError = out.[1]
    }
  }

let executeShellCommand command =
  executeCommand  "/bin/bash" [command]


let printColor color text =
    Console.ForegroundColor <- color
    printfn text
    Console.ResetColor()
    
    
    
let appSettingsExists (tcaFolder:string) =
    let appSettingsPath = Path.Join(tcaFolder, "appsettings.json")
    File.Exists (appSettingsPath)
    
let dockerComposeExists (tcaFolder:string) =
    let dockerComposePath = Path.Join(tcaFolder, "docker-compose.yml")
    File.Exists (dockerComposePath)
    
let migrateDockerCompose (tcaFolder:string) =
    let dockerComposePath = Path.Join(tcaFolder, "docker-compose.yml")   
    let allLines = File.ReadAllLines(dockerComposePath)
                   |> Array.map (fun line ->
                            if line.Contains """/app/files""" then
                                 """       - ./files_v2:/app/files_v2 """
                            elif  line.Contains "filetransferdocklinux" then
                                  line.Replace("filetransferdocklinux", "filetransferdock_v2_linux")
                            else  line
                      )
         
    File.WriteAllLines (dockerComposePath, allLines)
    
    
let migrateAppSettings (tcaFolder:string ) =
    let appSettingsPath = Path.Join(tcaFolder, "appsettings.json")         
    let allLines = File.ReadAllLines(appSettingsPath)
                   |> Array.map (fun line ->
                                    if line.Contains """"FileFolder""" then
                                         """  "FileFolder" : "./files_v2",
                             "TempFolder" : "./temp_folder" """
                                    elif line.Contains "TempFolder" then "" 
                                    else line
                                 )
         
    File.WriteAllLines (appSettingsPath, allLines)


let private LoadMap filename =
    let LoadMapFileExists () =
        try 
        let lines =
            File.ReadAllLines filename
            |> Array.map (fun str ->
                let toks =
                    str.Split(",", StringSplitOptions.TrimEntries + StringSplitOptions.RemoveEmptyEntries)

                toks[0], (toks[1] |> Guid))

        let map = Map<string, Guid> []
        lines |> Array.fold (fun (st: Map<string, Guid>) -> st.Add) map
        with ex -> printColor ConsoleColor.Red $"LoadMap : Error loading {filename} - {ex.Message}"
                   Map []
    if File.Exists filename then LoadMapFileExists() else Map []

let minDateTime dt1 dt2 dt3 =
    if dt1 < dt2 then
        if dt1 < dt3 then dt1
        else dt3
    elif dt2 < dt3 then dt2
        else dt3
        
let getParentFolder (path:string) =
    // let pth = if path.EndsWith Path.DirectorySeparatorChar then path[.. path.Length-1] // remove trailing slash
    //           else path
    let pth = path.TrimEnd (Path.DirectorySeparatorChar)
    Path.GetDirectoryName pth

let fileMigrate (csvFolder: string) (targetRootFolder: string) (userFilePath: string) (guid: Guid) =
    try
        let targetServerFilePath = Path.Join(targetRootFolder, userFilePath)
        let srcFilePath = Path.Join(csvFolder, guid.ToString())

        if not <| File.Exists srcFilePath then
            printColor
                ConsoleColor.Red
                $"Error : fileMigrate: Source file '{srcFilePath}' does not exist. '{userFilePath}'"
        else
            printColor ConsoleColor.Green $"Copying '{srcFilePath}' to '{targetServerFilePath}'"
            let folder = Path.GetDirectoryName targetServerFilePath
            
            if not <| Directory.Exists folder then 
                Directory.CreateDirectory folder |> ignore
                
            let folderTimeBeforeCopy = Directory.GetCreationTime folder    
            File.Copy(srcFilePath, targetServerFilePath, true) // copy overwrite, preserves creation date, time
            
            // On linux, folder creation time is actually folder modification time 
            let srcFilePathCreationTime = File.GetCreationTime srcFilePath
            let folderTimeAfterCopy = Directory.GetCreationTime folder            
            Directory.SetCreationTime(folder, (minDateTime folderTimeBeforeCopy folderTimeAfterCopy srcFilePathCreationTime) )
                        
    with ex ->
        printColor ConsoleColor.Red $"Error : fileMigrate {userFilePath} {guid} : {ex.Message}"

let folderMigrate (csvFolder: string) (targetRootFolder: string) (userFilePath: string) (guid:Guid)=
    try
        let targetFolderPath = Path.Join(targetRootFolder, userFilePath)
        let parentFolder = getParentFolder targetFolderPath
        let srcFolderPath = Path.Join(csvFolder, guid.ToString())
        let parentTimeBefore = Directory.GetCreationTime parentFolder 
        if not <| Directory.Exists targetFolderPath then 
            Directory.CreateDirectory targetFolderPath |> ignore
            if File.Exists srcFolderPath then
                let dirDateTime = File.GetCreationTime srcFolderPath
                Directory.SetCreationTime (targetFolderPath, dirDateTime) // set the same date time
                
            // adjust the parent folders time to the earliest possible time    
            let targetFolderTime = Directory.GetCreationTime targetFolderPath
            let parentTimeAfter = Directory.GetCreationTime parentFolder           
            Directory.SetCreationTime (parentFolder, (minDateTime parentTimeBefore parentTimeAfter targetFolderTime))
    with ex ->
        printColor ConsoleColor.Red $"Error : folderMigrate {userFilePath} : {ex.Message}"

let isFolder (userFileFolder: string) =
    userFileFolder.EndsWith(Path.DirectorySeparatorChar)

let doMigrate mapFile csvFolder targetRootFolder =
    let map = LoadMap mapFile

    map
    |> Seq.iter (fun kp ->
        if isFolder kp.Key then
            folderMigrate csvFolder targetRootFolder kp.Key kp.Value
        else
            fileMigrate csvFolder targetRootFolder kp.Key kp.Value)


let mapFilePath csvFolder =
    let mapFileName = "TismoFileServerMap.csv"
    Path.Join(csvFolder, mapFileName)


let migrate csvFolder targetFolder =
    printfn $"Migrating from '{csvFolder}' to '{targetFolder}'"
    let mapFile = mapFilePath csvFolder

    if not <| Directory.Exists csvFolder then
        printColor ConsoleColor.Red $"CSV Map folder '{csvFolder}' not found"
    elif not <| File.Exists mapFile then
        printColor ConsoleColor.Red $"CSV Mapfile '{mapFile}' not found"
    else
        Directory.CreateDirectory targetFolder |> ignore
        doMigrate mapFile csvFolder targetFolder

    ()

let printUsage () =
    printfn $"Run this program from the tcaxxx folder."
    // printfn $"Usage: <program>  folderContainingCSVMapFiles  newFolderForMigration"
    
    
let runShellCmd (cmd:string) =
    let result = executeShellCommand cmd |> Async.RunSynchronously
    if result.ExitCode = 0 then
     printfn $"{result.StandardOutput}"
    else
     printColor ConsoleColor.Red $"{result.StandardError}"      
    

let main () =
    printfn "FileTransfer app: Migration from CSV Maps to folders"
    // let args = fsi.CommandLineArgs[1..] // skip the first arg, it is this executable program
    
    let tcaPath = "./"
    
    if appSettingsExists tcaPath then
        if dockerComposeExists tcaPath then
            printfn "Stopping the current docker instance"
            runShellCmd "./stopFileTransfer.sh"                       
            migrate "./files"  "./files_v2"
            printfn "Migrating docker-compose.yml"
            migrateDockerCompose "./"
            printfn "Migrating appsettings.json"
            migrateAppSettings "./"
            printfn "Starting docker instance"
            runShellCmd "startFileTransfer.sh"
        else
            printColor ConsoleColor.Red "docker-compose.yml not found"
            printUsage()
    else
        printColor ConsoleColor.Red "appsettings.json not found"
        printUsage()
    

main ()



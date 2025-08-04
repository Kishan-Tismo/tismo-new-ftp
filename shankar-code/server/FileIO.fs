module server.FileIO
open System
open System.IO
open System.IO.Compression
open Types
open server.Utils

let  hasTrailingSlash (folderName:string) =
    Path.EndsInDirectorySeparator folderName
let  addTrailingSlash (folderName:string) =
  let fname =  if hasTrailingSlash folderName
               then folderName
               else $"{folderName}/"
  fname.Replace("//","/") // replace any double / with single /
 

let  notPresentIn (str:string) (badCharArray:char [])  =
    badCharArray
        |> Array.exists( str.Contains )
        |> not
    

    
let  invalidChars = Path.GetInvalidFileNameChars()
                            |> Array.filter( fun c -> c <> Path.DirectorySeparatorChar) // we will allow slash
let isValidFileName (fileName: string)  =
    let hasInvalidChars (fname:string) = invalidChars |> Array.exists fname.Contains
    
    (not (hasInvalidChars fileName))  // no invalid chars
    && (not (fileName.EndsWith(".")) ) // does not end with .
    && (not (fileName.EndsWith(" "))) // does not end with space
    
   
let  validPath (path:string) =
      Path.GetInvalidPathChars() |> notPresentIn path
    
               
let  isUserFolderRooted (userPath:UserFolder) =
    let (UserFolder ufold) = userPath
    Path.IsPathRooted ufold  // checks if the path starts with /
    
           
let isServerFolderSecure (serverFolder: ServerFolder) =
    let (ServerFolder sfold) = serverFolder
    sfold.StartsWith settings.FileFolder // all server side folders should be rooted to this
    
let isServerFileSecure (serverFile: ServerFile) =
    let (ServerFile sfile) = serverFile
    sfile.StartsWith settings.FileFolder // all server side files should be rooted to this
    
let toUserFolder (userFolderStr:string) =
    let folderName = addTrailingSlash userFolderStr
    UserFolder folderName
    

let userFolderToServerFolder (userFolder: UserFolder) =
    let (UserFolder ufold) = userFolder
    Path.Join (settings.FileFolder, ufold) |> ServerFolder
    
    
let serverFolderToUserFolder (serverFolder:ServerFolder) =
    let (ServerFolder sfold) = serverFolder
    if sfold.StartsWith settings.FileFolder
    then sfold[settings.FileFolder.Length ..] |> UserFolder
    else String.Empty |> UserFolder
    
    
let userFileToServerFile (userFile: UserFile ) =
    let (UserFile ufile) = userFile
    Path.Join (settings.FileFolder, ufile) |> ServerFile
    
let ServerFileToUserFile (serverFile:ServerFile) =
    let (ServerFile sfile) = serverFile
    if isServerFileSecure serverFile
    then sfile[settings.FileFolder.Length ..] |> UserFile
    else String.Empty |> UserFile
    
    
let serverFolderExists (serverFolder: ServerFolder) =
    let (ServerFolder sfold) = serverFolder
    Directory.Exists sfold
    
let userFolderExists (userFolder: UserFolder) =
    userFolder
        |> userFolderToServerFolder
        |> serverFolderExists
        
let serverFileExists (serverFile: ServerFile) =
    let (ServerFile sfile) = serverFile
    File.Exists sfile
    
    
let userFileExists(userFile: UserFile) =
    userFile
      |> userFileToServerFile
      |> serverFileExists      
    
let isFolder userFolderName =
    let serverFolder = userFolderName
                       |> UserFolder
                       |> userFolderToServerFolder    
    Directory.Exists (serverFolder.ToString())                            
    
let private doCreateServerFolder (serverFolder: ServerFolder) =
    let (ServerFolder sfold) = serverFolder
    Directory.CreateDirectory sfold 
    
let  doCreateUserFolder (userFolder:UserFolder) =    
    if not <| userFolderExists userFolder then       
       userFolder
         |> userFolderToServerFolder
         |> doCreateServerFolder
         |> ignore
    else
        Logger.log $"{userFolder} already exists."
    
    

let  createAllFoldersUptoThisFile (serverFilePath:ServerFile) =
    try
        if isServerFileSecure serverFilePath then 
           
            let (ServerFile sfile) = serverFilePath        
            let serverFolder =  Path.GetDirectoryName  sfile
                                |> ServerFolder
            doCreateServerFolder serverFolder |> ignore
            true
        else
            Logger.log $"Error : createAllFoldersUpToThisFile server file path not rooted {serverFilePath}"
            false
    with ex ->  
       Logger.log $"Error : CreateFolder {ex.Message}"
       false
               
      
let createFile (serverFile: ServerFile) =
    if isServerFileSecure serverFile then 
        let (ServerFile sfile) = serverFile
        Ok <| File.Create sfile
    else Error $"Invalid file {serverFile.ToString}"// will throw an exception
    
    
let doFileDelete (serverFile:ServerFile) =
    if isServerFileSecure serverFile then 
        let (ServerFile sfile) = serverFile
        File.Delete sfile
    
let doFolderDelete (serverFolder:ServerFolder) =
    if isServerFolderSecure serverFolder then // confirm that the given folder does not access anything else
        let (ServerFolder sfold) = serverFolder
        Directory.Delete(sfold,true)
     
let removeAnyTrailingSlash (sfold:string ) =
    if Path.EndsInDirectorySeparator sfold then
        Path.TrimEndingDirectorySeparator sfold
    else sfold
     
     
let getFolderContents   (serverFolder : ServerFolder) =    
    if isServerFolderSecure serverFolder then // confirm that the given folder does not access anything else
        let (ServerFolder sfold) = serverFolder
        let subFolders = Directory.GetDirectories(sfold)
                          |> Array.filter (fun dir -> not <| dir.StartsWith(Utils.tempFolder) )// don't show temp folder
                          |> Array.map (fun dir -> dir
                                                    |> ServerFolder
                                                    |> serverFolderToUserFolder
                                                    
                                        )                                                                                       
        let files = Directory.GetFiles(sfold)
                       |> Array.map(fun f ->
                                            f |> ServerFile
                                              |> ServerFileToUserFile
                                              
                           )
        (subFolders, files)
    else
        ([||],[||]) 
          
let getTempZipServerFilePath fileName =
   Path.Join(Utils.tempFolder, $"{fileName}.zip")
   |> ServerFile
  
    
let createZipOfFolder (serverFolder:ServerFolder) =
    if isServerFolderSecure serverFolder then
        let (ServerFolder sfold') = serverFolder   // /server/home/tca035/ClientProvided/Requirements/
        let sfold = removeAnyTrailingSlash sfold' //  /server/home/tca035/ClientProvided/Requirements
        let folderName = Path.GetFileName sfold   //  Requirements
        
        let zipPath = getTempZipServerFilePath folderName // /server/home/tca035/__temp__tca035/Requirements.zip
        if Directory.Exists Utils.tempFolder |> not then
            Directory.CreateDirectory (Utils.tempFolder) |> ignore
        if File.Exists (zipPath.ToString()) then File.Delete (zipPath.ToString()) // delete the old zip file
               
        ZipFile.CreateFromDirectory(sfold, zipPath.ToString(), CompressionLevel.Optimal, true);
        zipPath            
            |> Ok  // /server/home/tca035/__temp__tca035/Requirements.zip
    else Error $"Error while creating zip of folder {serverFolder}"   
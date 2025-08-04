module FileServer

open System
open System.IO
open System.Security.Claims
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open System.Web
open server.FileIO
open Types

let private stringTrim (s:string) =
    s.Trim()
    
let maxUserFileSize = 20_000_000_000L // 20 G

let private validIFormFile (formFile:IFormFile) =
    isValidFileName formFile.FileName
   && formFile.Length < maxUserFileSize

     
let CreateFolder (user:ClaimsPrincipal) (userFolderName : string) =
    try
        let userFolder = userFolderName
                         |> stringTrim
                         |> System.Net.WebUtility.UrlDecode
                         |> toUserFolder
        Logger.log $"by {user.Identity.Name} - CreateFolder {userFolder}"
                 
        if userFolder |> isUserFolderRooted then           
            doCreateUserFolder userFolder           
            Results.Ok (HttpUtility.HtmlEncode userFolder)
        else
            Logger.log $"Error folderName should start with / instead it is {userFolder}"
            Results.BadRequest("folderName should start with /")
    with ex ->  
               Logger.log $"Error : CreateFolder {ex.Message}"
               Results.Problem()
    
    
 
let private processFormFile userRootPath  formFile =
    task {
        if validIFormFile formFile then
            let userFilePath = Path.Combine( userRootPath, formFile.FileName) |> UserFile
            let serverFilePath = userFileToServerFile userFilePath   //  Path.Combine(serverRootFolder, userFilePath)
            createAllFoldersUptoThisFile serverFilePath |> ignore
            
            Logger.log $"Uploading {formFile.FileName} of size {formFile.Length} to {serverFilePath}"            
            match (createFile serverFilePath) with
             | Ok str ->    use stream = str
                            do! formFile.CopyToAsync(stream)
                            stream.Close()
                            Logger.log $"Uploaded {formFile.FileName} of size {formFile.Length} to {serverFilePath}"
                            return String.Empty
             | Error msg -> Logger.log $"Error : {msg}"
                            return $"Error : {msg}"
                        
        else
            return $"Invalid file {formFile.FileName |> HttpUtility.HtmlEncode}, "              
    }    

let private OnUploadAsync userRootPath (formFiles: IFormFile []) =
    let pformFile = processFormFile  userRootPath 
    task {        
        let! allFiles = formFiles
                        |> Array.map pformFile
                        |> Task.WhenAll
        let errorString = allFiles |> String.concat String.Empty
        
        return if errorString = String.Empty
           then
               Logger.log $"Uploaded {formFiles.Length} files."
               Results.Ok formFiles.Length  //,size) // file count,   total size 
           else
               Logger.log $"Error : {errorString}"
               Results.BadRequest errorString        
    }
    
let private getUserName (hreq:HttpRequest) =
    hreq.HttpContext.User.Identity.Name
    
let private getUserFolder (hreq:HttpRequest) =
    if not hreq.HasFormContentType  then Result.Error("Expected form content type")
    elif hreq.Form.Files.Count = 0 then Result.Error("File count is zero")
    else
        let b, userFolder = hreq.Query.TryGetValue("folderName")
        let upath = if b then (userFolder |> Seq.head |> stringTrim |> System.Net.WebUtility.UrlDecode)
                         else "/"                    
        if not (validPath upath) then
           Result.Error("Invalid folder path")
        else
           Result.Ok upath        

let FileUpload (hreq:HttpRequest) =    
    task {
        try            
            Logger.log $"by {getUserName hreq}, File upload  for file count {hreq.Form.Files.Count}, {hreq.Form.Files[0].Name}"           
           
            match (getUserFolder hreq) with
             | Result.Error  errStr ->
                Logger.log $"Error : Bad request {errStr}"
                return Results.BadRequest errStr
             | Result.Ok userPath ->          
                let files = hreq.Form.Files |> Seq.toArray
                
                return! OnUploadAsync  userPath files
        with ex ->            
            Logger.log $"Error : FileUpload {ex.Message}\n** {ex.TargetSite}\n*** {ex.StackTrace} \n**** {ex.InnerException} \n**** {ex.Source}"
            return Results.Problem()
    }
        
let prepResultsFile (userFile: UserFile) (serverFile:ServerFile)  =
    let contentType = if userFile.ToString().EndsWith(".zip")
                      then "application/zip"
                      else "application/octet-stream"    
    let absServerFilePath = Path.GetFullPath (serverFile.ToString())
    let fileDownloadName = Path.GetFileName(userFile.ToString())
    Logger.log $"download user file {userFile} from server: {absServerFilePath} -- {fileDownloadName}"   
    Results.File(absServerFilePath, contentType , $"""{fileDownloadName}""" )
    
    
let doFileDownload (userFile: UserFile) =  
    prepResultsFile userFile (userFileToServerFile userFile) 


// folders are zipped and then downloaded
let doFolderDownload (userFolder: UserFolder) =
    let serverFolder = userFolderToServerFolder userFolder    
    match   createZipOfFolder serverFolder with
     | Ok zipFile -> prepResultsFile (ServerFileToUserFile zipFile) zipFile 
     | Error msg -> Results.Problem msg
                
let FileDownload (user:ClaimsPrincipal) (userFileNameStr:string)  =
    try
    let userFileName = userFileNameStr
                       |> stringTrim
                       |> System.Net.WebUtility.UrlDecode                  
    Logger.log $"FileDownload by {user.Identity.Name}  {userFileName}"    
    match userFileName with
     | _ when userFolderExists (userFileName |> UserFolder) ->  doFolderDownload (userFileName |> UserFolder) 
     | _ when userFileExists (userFileName |> UserFile) -> doFileDownload (userFileName |> UserFile) 
     | _ ->  Logger.log $"Error: FileDownload: Not found {userFileName}"
             Results.BadRequest($"Error: Not found -> {userFileName}")           
    with ex ->              
               Logger.log $"Error: FileDownload {ex.Message}"
               Results.Problem()
 
let FileDelete (user:ClaimsPrincipal) (userFileName:string) =
    try
        let userFile = userFileName
                        |> stringTrim
                        |> System.Net.WebUtility.UrlDecode
                        |> UserFile
        Logger.log $"by {user.Identity.Name} - Delete File {userFile}"
        if userFileExists userFile then
            userFile
              |> userFileToServerFile
              |> doFileDelete                   
            Results.Ok($"Deleted {userFileName}")
        else                
            Logger.log $"Error: File for deletion not found {userFile}"
            Results.BadRequest("File not found")
    with ex -> 
               Logger.log $"Error: File delete {userFileName} {ex.Message}"
               Results.Problem()
 
    
let private userFolderToDto (userFolder: UserFolder) =
    if userFolderExists userFolder then
        let serverFolder = userFolder                          
                           |> userFolderToServerFolder
        {
            name = userFolder.ToString()
            isFolder = true
            size = 0L
            createdOn = (Directory.GetCreationTime (serverFolder.ToString()))
        }
    else
        {
            name = userFolder.ToString()
            isFolder = false
            size = 0L
            createdOn = DateTime.MinValue
        }
    
let private userFileToDto (userFile: UserFile) =
    if userFileExists userFile then
        let serverFile = userFile                         
                         |> userFileToServerFile 
        let finfo = FileInfo (serverFile.ToString())                      
        let dt = finfo.CreationTime
        let sz = finfo.Length        
        {
            name = userFile.ToString()
            isFolder = false
            size = sz
            createdOn = dt
        }
    else
        {
            name = userFile.ToString()
            isFolder = false
            size = 0L
            createdOn = DateTime.MinValue
        }
    

let DeleteFolder (user:ClaimsPrincipal) (userFolderName:string) =
    let folderName = userFolderName |> System.Net.WebUtility.UrlDecode
                                    |> stringTrim
                                    |> addTrailingSlash // folders should have trailing slash
                                    |> UserFolder
    Logger.log $"by {user.Identity.Name} - DeleteFolder {folderName}"
      
    if String.IsNullOrEmpty (folderName.ToString())
     || not (folderName.ToString().StartsWith("/")) // folder names should start with /
    then
        Results.BadRequest $"Folder name is not valid {userFolderName}"
    else
    try        
        folderName
          |> userFolderToServerFolder
          |> doFolderDelete      
        Results.Ok $"Deleted {userFolderName} "                    
    with ex -> 
               Logger.log $"Error : Delete Folder  {userFolderName} {ex.Message}"
               Results.Problem()
                    
let GetFileList (_user:ClaimsPrincipal) (userFolderName : string) =
   
    let userFolder = userFolderName
                     |> stringTrim
                     |> System.Net.WebUtility.UrlDecode
                     |> stringTrim
                     |> UserFolder
               
    if String.IsNullOrEmpty (userFolder.ToString())
     || not ( isUserFolderRooted userFolder) // folder names should start with /
    then
        Results.BadRequest $"Folder name is not valid {userFolderName}"
    else
    try      
       if isFolder (userFolder.ToString()) then            
            let ufolders, ufiles = userFolder
                                    |> userFolderToServerFolder
                                    |> getFolderContents // files as well as folders in the given folder
                                    
            let ufolderDtos = ufolders |> Array.map userFolderToDto
            let ufileDtos = ufiles |> Array.map userFileToDto
            
            let folderContents = ufolderDtos |> Array.append ufileDtos
                      
            Results.Ok folderContents // array of subfolders, files dto
        else
            Logger.log $"Error : GetFileList Folder {userFolderName} - not a folder"
            Results.BadRequest $"Folder does not exist {userFolderName}"
       
    with ex ->  
               Logger.log $"Error : GetFileList {userFolderName} {ex.Message}"
               Results.Problem()
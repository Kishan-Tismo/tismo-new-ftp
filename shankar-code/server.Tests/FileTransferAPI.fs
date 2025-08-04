module FileTransferAPI

open System


open System.Diagnostics
open System.IO
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.Security.Cryptography
open System.Text.Json
open System.Web
open NUnit.Framework
open FsUnit
open FsToolkit.ErrorHandling
open System.Diagnostics
open System.Runtime.CompilerServices
open System.Runtime.InteropServices


let testSite =   "https://tft997.tismo.tech" // https://tft999.tismo.tech
let customerCode = "tca997" // tca999


let userName = "tismo"
let password = "cKtc-avsn-JB7p-WqHV" //"mH4v-jVhj-jFUB-bnVV"

let run1GBUploadTest = true // make this false to skip this test - it takes 10+ minutes to execute

// type LoginParameters =
//     {
//         user: string
//         pwd: string
//     }
//     
type FileT =
    {
        name:string
        isFolder:bool
        size: int
        createdOn : DateTime
    }
    
let sendRequestAsync (httpClient:HttpClient) (method: HttpMethod) (endpoint: string) (content: HttpContent option) =
        async {
            let request = new HttpRequestMessage(method, endpoint)            
            
            match content with
            | Some c -> request.Content <- c                      
            | None -> ()
            
            let! response = httpClient.SendAsync(request) |> Async.AwaitTask
            let! responseContent = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            let statusCode = response.StatusCode
            // let apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent)

            return statusCode, responseContent
        }    
    
let doLogin (baseUrl:string) (user:string) (pwd:string) =
    async {
            let endpoint = baseUrl + "/login"
            let queryString = HttpUtility.ParseQueryString("")
            queryString.Add("user", user)
            queryString.Add("pwd", pwd)
            use httpClient = new HttpClient()
            let! statusCode, response = sendRequestAsync httpClient HttpMethod.Get (endpoint + "?" + queryString.ToString()) None
           
            return statusCode, response.Trim('"')
        }
    

type ApiClient ( baseUrl:string ,token: string) =
   
    let httpClient = new HttpClient()
    do
        httpClient.DefaultRequestHeaders.Authorization <-  AuthenticationHeaderValue("Bearer", token)
        httpClient.DefaultRequestHeaders.Accept.Add (MediaTypeWithQualityHeaderValue("application/json"))        
        
    member this.SetTimeout timeout =
        httpClient.Timeout <- timeout // TimeSpan.FromMinutes 15
    
    member private this.sendRequestAsync (method: HttpMethod) (endpoint: string) (content: HttpContent option) =
        sendRequestAsync httpClient  method endpoint content

    member this.CreateFolderAsync (folderName: string) =
        async {                       
            let queryString = HttpUtility.ParseQueryString("")
            queryString.Add("folderName", folderName)
            let endpoint = baseUrl + $"/createFolder" + "?" + queryString.ToString()
            // let content = new StringContent(JsonSerializer.Serialize( folderName ), System.Text.Encoding.UTF8, "application/json")
            let! statusCode, response = this.sendRequestAsync HttpMethod.Post endpoint None //(Some content)
            if statusCode <> HttpStatusCode.OK then return Error $"Error: Create folder {folderName} {statusCode}"
            else return Ok response
            // return statusCode, response
        }

    member this.GetCustomerCodeAsync () =
        async {
            let endpoint = baseUrl + "/customerCode"
            let! statusCode, response = this.sendRequestAsync HttpMethod.Get endpoint None
            if statusCode <> HttpStatusCode.OK then return Error $"Error: GetCustomerCode {statusCode}"
            else return Ok (response.Trim('"'))
            // return statusCode, response
        }

    member this.DeleteFileAsync (fileName: string) =
        async {
            let endpoint = baseUrl + $"/deleteFile/{HttpUtility.UrlEncode fileName}"
            let! statusCode, response = this.sendRequestAsync HttpMethod.Delete endpoint None
            if statusCode <> HttpStatusCode.OK then return Error $"Error while deleting file {fileName} {statusCode.ToString()}"
            else return Ok response
            // return statusCode, response
        }
        
    member this.doDownload (uri:string) (localDownloadFileName:string) =
        async {            
            use! responseStream =  httpClient.GetStreamAsync uri |> Async.AwaitTask
            use  fs =  File.Create localDownloadFileName
            do! responseStream.CopyToAsync(fs) |> Async.AwaitTask            
        } 

    member this.DownloadFileAsync (localFileName:string) (remoteFile: string) =
        async {
            let endpoint = baseUrl + "/download"
            let queryString = HttpUtility.ParseQueryString("")
            queryString.Add("filename", remoteFile)
            let uri = endpoint + "?" + queryString.ToString()
            do! this.doDownload uri localFileName
            return Ok localFileName
            // let! statusCode, response = this.sendRequestAsync HttpMethod.Get (endpoint + "?" + queryString.ToString()) None
            // if statusCode <> HttpStatusCode.OK then return Error $"Error downloading file {fileName} {statusCode}"
            // else return Ok response
            // return statusCode, response
        }

    member this.GetFileListAsync (folderName: string) =
        async {
            let endpoint = baseUrl + "/fileList"
            let queryString = HttpUtility.ParseQueryString("")
            queryString.Add("folderName", folderName)
            let! statusCode, response = this.sendRequestAsync HttpMethod.Get (endpoint + "?" + queryString.ToString()) None
            
            let resp =  JsonSerializer.Deserialize<FileT array> response
            if statusCode <> HttpStatusCode.OK
            then return Error $"Error Get file list {folderName} {statusCode}"
            else return Ok resp
            // return statusCode, resp
        }
        
    
    member this.UploadFileAsync (folderName:string) (files: string list) =
        async {
            let endpoint = baseUrl + "/upload"
            let queryString = HttpUtility.ParseQueryString("")
            queryString.Add("folderName", folderName)
            use content = new MultipartFormDataContent()
            files |> List.iter (fun file ->
                let fileStream = File.OpenRead file
                content.Add(new StreamContent(fileStream), file, file)
                )
            let! statusCode, response = this.sendRequestAsync HttpMethod.Post (endpoint + "?" + queryString.ToString()) (Some content)            
            if statusCode <> HttpStatusCode.OK then return Error $"Error: Upload file list - first file {files[0]} {statusCode}"
            else return Ok response
        }

    member this.DeleteFolderAsync (folderName: string) =
        async {
            let endpoint = baseUrl + $"/deleteFolder/{HttpUtility.UrlEncode folderName}"
            let! statusCode, response = this.sendRequestAsync HttpMethod.Delete endpoint None
            if statusCode <> HttpStatusCode.OK then return Error $"Error while deleting folder {folderName } {statusCode.ToString()}"
            else return Ok response
        }
        
        

let createApiClient () =
    async {
        try
            // Call the login method to obtain the JWT token
            let baseUrl = testSite 
            let! loginStatus, loginResponse = doLogin baseUrl userName password

            if loginStatus = HttpStatusCode.OK then
                let token =  loginResponse                 

                // Create an instance of ApiClient with the token
                let apiClient =  ApiClient(baseUrl,token)
                return Ok apiClient           
            else return Error "Could not login"
        with
        | ex ->
            printfn $"An error occurred: %s{ex.Message} {ex.StackTrace} {ex.InnerException}"
            return Error $"Exception while logging in {ex.Message}"
    } 

        
let deleteAll() =
 asyncResult {            
   let! client = createApiClient()     
   let! fileArray = client.GetFileListAsync "/"
   let! responses = fileArray                       
                        |> Array.map (fun item ->
                                if item.isFolder
                                then client.DeleteFolderAsync item.name 
                                else client.DeleteFileAsync item.name
                                )
                        |> Array.toList
                        |> List.sequenceAsyncResultM                                  
   return responses
   
    }
 
let inline getCallerName () =
    let stackTrace = StackTrace()
    let topFrame = stackTrace.GetFrame(1)
    let currentFunction = topFrame.GetMethod()
    currentFunction.Name
    // $"%s{currentFunction.DeclaringType.Name}.%s{currentFunction.Name}" 
 
let createSampleFile() =
     let fileName = Path.GetRandomFileName()
     let data = [for i in [0..1000] -> $"{i}\n"]
     File.WriteAllLines(fileName, data)    
     fileName
     
     // ∑∰⌛☃♎♎⛳
   // ([<CallerMemberName; Optional; DefaultParameterValue("")>] memberName: string)
let runAsyncResult r  =
    r |> Async.RunSynchronously
          |> function
               | Ok t -> printfn $"\u2705 Passed - {getCallerName()}  {t}"
               | Error err -> printfn $"{err}"
                              raise (Exception err)
              
              
let removeAnyTrailingSlash (sfold:string ) =
    if Path.EndsInDirectorySeparator sfold then
        Path.TrimEndingDirectorySeparator sfold
    else sfold
                   
                   
/// Checks if the file is in the specified path on the server                   
let verifyFileInPath (client:ApiClient) (path:string) (file:string) =
    asyncResult {
        let! folderContents = client.GetFileListAsync path
 
        folderContents                      
                       |> Array.exists(fun f -> f.name.Contains file )
                       |> should be True
        
    } |> Async.RunSynchronously
                              
[<Test>]
let ``Create deep nested folders from root`` () = // Creates deep nested folders and verifies if all folders are present
    asyncResult {
      let folders =    [0..128]
                         |> List.map ( fun _ -> Path.GetRandomFileName())
                         |> List.scan ( fun acc item ->  $"{acc}{item}/") "/"
      let! client = createApiClient()
      let! _reps = folders
                 |> List.map client.CreateFolderAsync
                 |> List.sequenceAsyncResultM
      // check if all the folders are created or not           
      folders |> List.iter (fun folder' ->
                        let folder = removeAnyTrailingSlash folder' 
                        let path = Path.GetDirectoryName folder
                        if not (String.IsNullOrEmpty path) then 
                            verifyFileInPath client path (Path.GetFileName path) |> ignore
                        
                    )                 
      return Ok true
                         
    }
    |> runAsyncResult
     
[<Test>]
let ``Create folder in root folder``() =
    asyncResult {
        printfn $"Create folder in root"
        let folderName = Path.GetRandomFileName()
        let! client = createApiClient()
        let! r = client.CreateFolderAsync $"/{folderName}"
        
        let! fileList = client.GetFileListAsync "/" // get list of files 
        fileList |> Array.map( fun f -> Path.GetFileName f.name)               
                 |> should contain folderName
        
        return r
        
    } |> runAsyncResult
    
let SHA256 (fileName:string) =
    use hashFile = File.Open( fileName, FileMode.Open)
    use sha256Hash = SHA256.Create() // SHA256Managed.Create()
    sha256Hash.ComputeHash(hashFile)    
    
let createLargeFile fileName size =
    use fs = new FileStream (fileName, FileMode.Create, FileAccess.Write, FileShare.None)
    fs.SetLength size
    fs.Close()
    
[<Test>]
// [<Ignore "Takes more than 10 mins. Skipping.">]
let ``Upload a large file to test Folder - takes more than 9 mins \u231A ``() =
    asyncResult {
        if not run1GBUploadTest then
            printfn $"Test skipped -- upload a large file, takes about 10mins."
        else 
            printfn $"Large file upload download test  takes a long time"
            let stopWatch = System.Diagnostics.Stopwatch.StartNew()

            let fileName = Path.GetRandomFileName()
            let fileSize:int64 = 1024L*1024L*1024L   // 1 GB
            createLargeFile fileName fileSize
            let originalHash = SHA256 fileName
            printfn $"Original hash %A{originalHash}"
            
            let! client = createApiClient()
            client.SetTimeout (TimeSpan.FromMinutes 60 )
            
            let path = "/test"
            let! _r = client.UploadFileAsync path [fileName]
            let! _verify = verifyFileInPath client path fileName
            let! folderContents = client.GetFileListAsync path
            let fileInfo = folderContents |> Array.tryFind (fun f -> f.name.Contains fileName )
            match fileInfo with
             | Some finfo -> finfo.size |> should equal fileSize
             | None -> raise (Exception $"Upload a large file : {fileName} does not exist on the server")
             
            let fileCopy =  Path.GetRandomFileName()
            let! _lfile = client.DownloadFileAsync fileCopy $"/test/{fileName}"
            let copyHash = SHA256 fileCopy
            originalHash |> should equalSeq copyHash // compare the hash of the downloaded file with original
            File.Delete fileName
            File.Delete fileCopy

            stopWatch.Stop()
            printfn $"Time to execut this test {stopWatch.Elapsed.TotalMinutes} Minutes "
        
    } |> runAsyncResult
    
    
    
    
[<Test>]
let ``Upload one byte file to test Folder``() =
    asyncResult {              
        let fileName = Path.GetRandomFileName()
        let data = [|32uy|] // one unsigned byte
        File.WriteAllBytes(fileName, data) // create a local file here
        let! client = createApiClient()
        let path = "/test"
        let! _r = client.UploadFileAsync path [fileName]
        let! _verify = verifyFileInPath client path fileName
        let! folderContents = client.GetFileListAsync path
        let fileInfo = folderContents |> Array.tryFind (fun f -> f.name.Contains fileName )
        match fileInfo with
         | Some finfo -> finfo.size |> should equal 1
         | None -> raise (Exception $"Upload one byte file - file does not exist on the server")
        File.Delete fileName // delete the local file
        let! _lfile = client.DownloadFileAsync fileName $"/test/{fileName}"
        
        File.Exists fileName |> should be True
        let dataDownloaded = File.ReadAllBytes fileName
        dataDownloaded.Length |> should equal 1
        dataDownloaded[0] |> should equal data[0]
        
        File.Delete fileName
        return String.Empty
    }|> runAsyncResult
     

[<Test>]
let ``Upload one file to root Folder`` () =
    asyncResult {      
        let fileName = createSampleFile()
        let! client = createApiClient()
        let! _r = client.UploadFileAsync "/" [fileName]
        let originalHash = SHA256 fileName
                  
        let! fileList = client.GetFileListAsync "/" // get list of files
        
        
        fileList |> Array.map( fun f -> Path.GetFileName f.name)               
                 |> should contain fileName
                 
        let fileCopy =  Path.GetRandomFileName()
        let! _lfile = client.DownloadFileAsync fileCopy $"/{fileName}"
        let copyHash = SHA256 fileCopy                        
        originalHash |> should equalSeq copyHash // compare the hash of the downloaded file with original
        
        File.Delete fileName // delete local file 
        File.Delete fileCopy // delete downloaded local file
        return String.Empty
    }|> runAsyncResult
        
        
[<Test>]
let ``Unicode filename upload and check``() =
    asyncResult {
        // create file with unicode characters
        let fileName =  "\U0001F47D-SpecialChar-File-ಟಿಸ್ಮೋ-"
        let data = [for i in [0..10000] -> $"{i*2 + 15}\n"]
        File.WriteAllLines(fileName, data)    // create local file
        let originalHash = SHA256 fileName
        
        let! client = createApiClient()
        let! _r = client.UploadFileAsync "/" [fileName]
        
             
        let! fileList = client.GetFileListAsync "/" // get list of files       
        fileList |> Array.map( fun f -> Path.GetFileName f.name)               
                 |> should contain fileName
                 
        let fileCopy =  Path.GetRandomFileName()
        let! _lfile = client.DownloadFileAsync fileCopy $"/{fileName}"
        let copyHash = SHA256 fileCopy                        
        originalHash |> should equalSeq copyHash // compare the hash of the downloaded file with original
        
        File.Delete fileName // delete local file 
        File.Delete fileCopy // delete downloaded local file                 
        
        return String.Empty
    } |> runAsyncResult
        
        
[<Test>]
let ``Upload of multiple files to root Folder`` () =
    asyncResult {      
        let fileNames = [ for _ in [0..100] ->  createSampleFile()]
        let! client = createApiClient()
        let! _r = client.UploadFileAsync "/" fileNames
        fileNames |> List.iter File.Delete                
        let! fileList = client.GetFileListAsync "/" // get list of files
       
        let serverFiles = fileList |> Array.map( fun f -> Path.GetFileName f.name)
        fileNames |> List.iter (fun f -> serverFiles |> should contain f)                         
        return String.Empty
    }|> runAsyncResult
        


[<Test>]
let ``Get customer code test`` () =
    asyncResult {
        let! client = createApiClient()
        let! ccode = client.GetCustomerCodeAsync()
        ccode |> should equal customerCode
    }|> runAsyncResult
        
        
[<Test>]
let ``Delete all files`` () =
        asyncResult {
           let! _k = deleteAll()
           
           return 0           
        } |> runAsyncResult
 
    


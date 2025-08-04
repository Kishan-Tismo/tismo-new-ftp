module Types

open System

type userFile = string
type userFolder = string
type userName = string

[<CLIMutable>]
type Settings = { // from appSettings.json
    CustomerCode : string
    ClientUserName : string
    ClientHashedPassword : string // encoded in base64
    ClientSalt: string // encoded in base64
    TismoUserName : string
    TismoHashedPassword : string // encoded in base64
    TismoSalt:string // encoded in base64
    LogFile : string    // log file name 
    FileFolder : string // folder to be used for saving files
  
}


type FileDto = // file data transfer object
    {
        name : string
        isFolder : bool
        size : int64
        createdOn : DateTime
    }
    
    
// user will specify the folder as /ClientProvided/Reqs/Version1/srs.docx
// this would be actualy at    /home/TCA032/ServerRootFolder/ClientProvided/Reqs/Version1/srs.docx
// the server root path is always hidden from the user
// the server root path is prefixed to what the user provides to access the files
// the server root path is removed to show the name and path of files to the user
    
type ServerFolder =
    | ServerFolder of string // folder that has server root as the rooted path
    override this.ToString() =
             let (ServerFolder str) = this
             str
         
type ServerFile =
    | ServerFile of string   // file that has server root as the rooted path
    override this.ToString() =
             let (ServerFile str) = this
             str
type UserFolder =
    | UserFolder of string // user folder path that has user root as the rooted path
    override this.ToString() =
             let (UserFolder str) = this
             str
type UserFile =
    | UserFile of string // user file that has user root as the rooted path
    override this.ToString() =
             let (UserFile str) = this
             str

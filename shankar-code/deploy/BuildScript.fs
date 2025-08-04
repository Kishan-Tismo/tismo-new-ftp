module BuildScript

open System
open System.IO


type Folders = {
    ServerSources: string
    ClientSources: string
    TargetFolder: string
    TestSources: string 
}


type BuildCommands = {
    angularBuild : string
    dotnetBuild : string
    dockerBuild : string
    dockerTar : string 
    dockerZip : string
    // doTests : string
}



type ResultBuilder() =
    member x.Bind(v,f) =  Result.bind f v  
    member x.Return v = Ok v
    member x.ReturnFrom o = o

let result = ResultBuilder()

// Runs a shell command 
let runShellCmd commandName arguments workingfolder=
    printfn $"{commandName} {arguments}"
    let procStartInfo = 
        Diagnostics.ProcessStartInfo(
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            FileName = commandName,
            Arguments = arguments,
            WorkingDirectory = workingfolder
        )
    use p = new Diagnostics.Process(StartInfo = procStartInfo)
    let printifnotnull (s:Diagnostics.DataReceivedEventArgs) = if not (isNull s || isNull s.Data) then printfn "%A" s.Data
    p.OutputDataReceived.Add(printifnotnull ) // event subscription for output 
    p.ErrorDataReceived.Add(printifnotnull) // event subscription for error
    p.Start() |> ignore
    p.BeginOutputReadLine()
    p.BeginErrorReadLine()
    p.WaitForExit()  // synchronous -- waiting for the command to complete
    if p.ExitCode = 0 then Ok 0 else Error p.ExitCode

// Runs a shell command with the given working folder
let run (command:string) (workingFolderPath:string) =
   match command.Split( ' ', '\t' ) |> Array.toList |> List.filter( fun s -> s.Length > 0) with
    | [] -> Error -1
    | head::tail -> runShellCmd head (tail |> String.concat(" ")) workingFolderPath


// remove the dockerdeploy files
let clean folder =
    try
        IO.Directory.Delete(folder, true)
    with 
        _ ->  4 |> ignore
        
        


// angular build is same for both mac and linux builds
let angularbuild folders =
    $"ng build --verbose=false --build-optimizer --optimization --aot  --output-path {folders.TargetFolder}/wwwroot"
    // $"ng build --verbose=false --prod --build-optimizer --optimization --aot  --output-path {folders.TargetFolder}/wwwroot"


let dockerFile = "filetransfer.Dockerfile"
// let dotnetTest = "dotnet test --nologo -v q"
let version = "_v2_"
let getLinuxBuildCommands (folder:Folders) =
    let dockerImageName = "filetransferdock" + version +  "linux"
    let angBuilder = angularbuild folder
    let dotnetBuilder = $"dotnet publish -v q --configuration Release --no-self-contained --runtime linux-x64  -o {folder.TargetFolder}"
    let dockerBuilder = $"docker build -q --platform linux/amd64   -f {dockerFile} -t {dockerImageName} {folder.TargetFolder}"
    {
        angularBuild = angBuilder
        dotnetBuild = dotnetBuilder
        dockerBuild = dockerBuilder
        dockerTar = $"docker save {dockerImageName} --output {dockerImageName}.tar"
        dockerZip = $"gzip {dockerImageName}.tar "
        // doTests = dotnetTest
    }
    
let getMacM1BuildCommands (folder:Folders) =
    let dockerImageName = "filetransferdock" + version  + "macm1"
    let angBuilder = angularbuild folder
    let dotnetBuilder = $"dotnet publish -v q --configuration Release --no-self-contained --runtime osx-arm64  -o {folder.TargetFolder}"
    let dockerBuilder = $"docker build -q --platform linux/arm64  -f {dockerFile} -t {dockerImageName} {folder.TargetFolder}"
    {
        angularBuild = angBuilder
        dotnetBuild = dotnetBuilder
        dockerBuild = dockerBuilder
        dockerTar = $"docker save {dockerImageName} --output {dockerImageName}.tar"
        dockerZip = $"gzip {dockerImageName}.tar "
        // doTests = dotnetTest
    }
    
// let doTests (folders:Folders) =
//     result {
//         return! run dotnetTest folders.TestSources // run test quietly
//     }

let doBuild (folders:Folders) (buildcommands:BuildCommands)  =
     result {
        // let! _tst = run buildcommands.doTests folders.TestSources
        let! _ang = run buildcommands.angularBuild folders.ClientSources
        let! _dot = run buildcommands.dotnetBuild folders.ServerSources
        let! _dok = run buildcommands.dockerBuild "./"
        let! _tar = run buildcommands.dockerTar "./"
        return! run buildcommands.dockerZip "./"
    }
let buildForLinux (folders:Folders) =
    clean folders.TargetFolder
    Directory.GetFiles("./", "*.tar")|> Array.iter(File.Delete)
    Directory.GetFiles("./", "*.tar.gz")  |> Array.iter(File.Delete)
    folders |> getLinuxBuildCommands
            |> doBuild folders
   

let buildForMacM1 (folders:Folders) =
    clean folders.TargetFolder
    Directory.GetFiles("./", "*.tar")|> Array.iter(File.Delete)
    Directory.GetFiles("./", "*.tar.gz")  |> Array.iter(File.Delete)
    folders |> getMacM1BuildCommands
            |> doBuild folders

let buildAll (folders:Folders) =
    result {
       
        let! _m = buildForMacM1 folders
        let! _l =  buildForLinux folders
        return 0
    }
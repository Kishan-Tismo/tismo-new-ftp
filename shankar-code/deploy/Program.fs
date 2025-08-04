
open BuildScript

let cleanAll folder=
    printfn "cleanAll"
    clean folder
    [| "bin"; "cs"; "de"; "en" ; "es"; "fr"; "it"; "ja"; "ko"; "obj" ;"pl" ;"pt-BR"; "ru"; "tr" ;"zh-Hans"; "zh-Hant" |] 
             |> Array.iter (clean)

let reportResult res =
    match res with
        | Ok _  -> printfn "Production build successful\n\n"
                   Ok 0
        | Error _ -> printfn "Production build  Errors encountered\n\n"
                     Error -1
    
let buildProductionMac folders =
    printfn "Production build for Mac M1 - Docker "
    buildForMacM1 folders |> reportResult               
            
let buildProductionLinux folders =
    printfn "Production build for Linux - Docker"
    buildForLinux folders |> reportResult

let buildProductionAll folders =
    printfn "Production build for Mac and Linux - Docker"
    buildAll folders |> reportResult
            
            

[<EntryPoint>]
let main argv =
// local folder to keep all build files
    printfn "         Builds File Transfer App."
     
    let folders = {
        ServerSources =  "../server"
        ClientSources =  "../client"
        TargetFolder =   "../deploy/dockerdeploy"
        TestSources =    "../server.Tests"
    }
    let usage () = 
        printf "\nUsage : dotnet run [ buildLinux | buildMac | clean | test ] --- from within this folder\n"
        Error -1

    let res =  match argv with 
                | v when v.Length  = 0 -> usage()
                | v when v[0] = "buildAll" -> buildProductionAll folders
                | v when v[0] = "buildMac" -> buildProductionMac folders
                | v when v[0] = "buildLinux" -> buildProductionLinux folders
                | v when v[0] = "clean" -> cleanAll folders.TargetFolder; Ok 0
                // | v when v[0] = "test" -> doTests folders 
                | _ -> printfn $"Unknown command : {argv[0]}\n"; usage()
    match res with 
        | Ok _ -> 0
        | Error e -> e


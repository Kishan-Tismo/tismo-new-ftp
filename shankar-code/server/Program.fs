
open System

open System.IO
open System.Security.Claims
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Http.Features
open Microsoft.AspNetCore.Server.Kestrel.Core
open Microsoft.Extensions.FileProviders
open Microsoft.Extensions.Hosting
open Microsoft.OpenApi.Models
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.IdentityModel.Tokens


open server
open server.Utils


#nowarn "20"  // suppress warning that the return value is not used

let doLogin (hreq:HttpRequest) userName password =
   task {
       printfn $"Login by {userName} "
       let jwt = User.Authenticate Utils.settings userName password
       if jwt = String.Empty then do! Task.Delay(1000) // insert delay for failed authentication
       return if jwt <> String.Empty
             then
                 printfn $"{userName} Logged in"
                 Logger.log $"{userName} logged in."
                 Results.Ok(jwt) 
             else
                 printfn $"Authentication failed for {userName} from {hreq.HttpContext.Connection.RemoteIpAddress}"
                 Logger.log $"Authentication failed for {userName} from {hreq.HttpContext.Connection.RemoteIpAddress}"
                 Results.BadRequest("Invalid credentials")       
   }

let createWebApi version (args:string []) =
    let builder = WebApplication.CreateBuilder(args)
    let myCors = "myCorsPolicy"
    builder.Services.AddCors(
        fun corsoptions -> corsoptions.AddPolicy( myCors,
            fun pb ->
                pb.WithOrigins([|"http://localhost:4200"|])
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  
                |> ignore
        )
    )   

    let swaggerEndPointPrefix = "v1"  
    if builder.Environment.IsDevelopment() then         
        builder.Services.AddEndpointsApiExplorer() // OpenAPI - Swagger 
        let openapiinfo = OpenApiInfo(Version = version,
                                      Title = "Tismo File Server",
                                      Description = "Copyright (c) 2022 Tismo Technology Solutions (P) Ltd.")
          
        builder.Services.AddSwaggerGen(fun options ->  options.SwaggerDoc(swaggerEndPointPrefix, openapiinfo)) |> ignore
        
    builder.Services.Configure<KestrelServerOptions>(
                fun (options:KestrelServerOptions) ->options.Limits.MaxRequestBodySize <- System.Nullable()
            )
    builder.Services.Configure<FormOptions>( // default limits on the form has to be modified
        fun (x:FormOptions) -> 
            x.ValueLengthLimit <- Int32.MaxValue
            x.ValueCountLimit <- Int32.MaxValue;
            x.MultipartBodyLengthLimit <- Int64.MaxValue // In case of multipart                       
        )
       
    builder.Services.AddAuthentication(
        fun x ->    
            x.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
            x.DefaultChallengeScheme <- JwtBearerDefaults.AuthenticationScheme
    ).AddJwtBearer(
        fun x ->               
            x.SaveToken <- true
            x.TokenValidationParameters <-
                  TokenValidationParameters(                        
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = SymmetricSecurityKey(User.jwtKey),
                            ValidateIssuer = false,
                            ValidateAudience = false                          
                        )
    )
    builder.Services.AddAuthorization()
    let app = builder.Build()            
    
    if app.Environment.IsDevelopment() then
        printfn "Enabling Cors"
        app.UseDeveloperExceptionPage()
        app.UseCors(myCors) |> ignore
        
    app.UseDefaultFiles();    
    app.UseStaticFiles();    
    app.UseAuthentication();
    app.UseAuthorization();
   
//    app.MapGet("/ping",     Func<string>(fun () -> "Tismo File Server running.")).WithTags("Ping")
//        .WithTags("Ping")
    
    app.MapGet("/login",
               Func<HttpRequest, string, string, Task<IResult>>(fun hreq user pwd -> doLogin hreq user pwd ))
        .WithTags("Login")
    
    app.MapGet("/customerCode",
               Func<IResult>(fun () -> Results.Ok Utils.customerCode))
       .WithTags("Customer Code")
       .RequireAuthorization()
    
    app.MapPost("/upload",
               Func<HttpRequest, Task<IResult>>(fun hreq -> FileServer.FileUpload hreq ))
       .Accepts<IFormFile[]>("multipart/form-data")
       .WithTags("File Upload")
       .RequireAuthorization()
       
    app.MapGet("/download",
               Func<string,ClaimsPrincipal, IResult>(fun filename user  -> FileServer.FileDownload user filename  ))
       .WithTags("File Download")
       .RequireAuthorization()
       
    app.MapDelete("/deleteFile/{fileName}",
               Func<string,ClaimsPrincipal, IResult>(fun filename user-> FileServer.FileDelete user filename))
       .WithTags("File Delete")
       .RequireAuthorization()
       
    app.MapDelete("/deleteFolder/{folderName}",
               Func<string, ClaimsPrincipal, IResult>(fun folderName user -> FileServer.DeleteFolder user folderName))
        .WithTags("Folder Delete")
        .RequireAuthorization()
       
    app.MapPost("/createFolder",
               Func<string,ClaimsPrincipal, IResult>(fun folderName user -> FileServer.CreateFolder user folderName ))
        .WithTags("Create Folder")
        .RequireAuthorization()
        
    app.MapGet("/fileList",
               Func<string,ClaimsPrincipal, IResult>(fun folderName user -> FileServer.GetFileList user folderName))
        .WithTags("File list")
        .RequireAuthorization()
       
    if app.Environment.IsDevelopment() then   
        app.UseSwagger()
        app.UseSwaggerUI( fun opt ->  opt.SwaggerEndpoint($"{swaggerEndPointPrefix}/swagger.json", version)) |> ignore
         
         
    app.UseSpa(fun spa ->
        spa.Options.DefaultPageStaticFileOptions <- new StaticFileOptions(
            FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
        )
        spa.Options.DefaultPageStaticFileOptions.DefaultContentType <- "text/html"
    ) |> ignore
    
    app
            

let launchWebApiServer (args:string []) =           
    let app = createWebApi version args    
    app.Run()  
                                      
let printAppString() =
    printfn "Tismo File Server"
    printfn "Copyright (c) 2022 Tismo Technology Solutions (P) Ltd."
    printfn ""
    
    
              
[<EntryPoint>]
let main _ =
    server.Utils.initLogging()
    printAppString()
    let args = Environment.GetCommandLineArgs() // args[0] is name of the program
    launchWebApiServer args 
      
    Logger.log $"Tismo File Server  exiting"     
    0 // exit code 0 

#!/bin/bash 

dotnet clean deploy/deploy.fsproj
dotnet clean server/server.fsproj
dotnet clean Tools/changePasswords/changePasswords.fsproj
dotnet clean Tools/createPasswords/createPasswords.fsproj
dotnet clean Tools/verifyPassword/verifyPassword.fsproj
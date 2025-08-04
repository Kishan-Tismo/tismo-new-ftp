#!/usr/bin/env bash

# build for linux

echo Building for linux
dotnet publish -v q --configuration Release --no-self-contained --runtime linux-x64  -p:PublishSingleFile=true -o "./linuxTools" ./createPasswords/createPasswords.fsproj
dotnet publish -v q --configuration Release --no-self-contained --runtime linux-x64  -p:PublishSingleFile=true -o "./linuxTools" ./changePasswords/changePasswords.fsproj
dotnet publish -v q --configuration Release --no-self-contained --runtime linux-x64  -p:PublishSingleFile=true -o "./linuxTools" ./verifyPassword/verifyPassword.fsproj
      
    

echo Building for mac
dotnet publish -v q --configuration Release --no-self-contained --runtime osx.11.0-arm64  -p:PublishSingleFile=true -o "./macTools" ./createPasswords/createPasswords.fsproj
dotnet publish -v q --configuration Release --no-self-contained --runtime osx.11.0-arm64  -p:PublishSingleFile=true -o "./macTools" ./changePasswords/changePasswords.fsproj
dotnet publish -v q --configuration Release --no-self-contained --runtime osx-arm64  -p:PublishSingleFile=true -o "./macTools" ./verifyPassword/verifyPassword.fsproj
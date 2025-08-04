
dotnet run clean
rm *.gz 

dotnet run buildLinux

rsync filetransferdock_v2_linux.tar.gz shankar@sutlej:/home/shankar    

on Sutlej :  
docker load < filetransferdock_v2_linux.tar.gz


For testing run on Mac M1
cd <..>/FileTransfer/deploy
dotnet run buildMac
docker compose up 
Browser -- http://localhost:3999
For username password check the remark section in 
<..>/FileTransfer/deploy/app/appsettings.production.json


To generate passwords for testing run,
cd OnTargetMachine/Template
dotnet fsi ./createPasswords.fsx tca997 appsettings.production.json  
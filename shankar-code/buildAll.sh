#!/bin/bash 


# first build the docker image
cd deploy
buildLinuxAndMac.sh 
cd ..

# build some utilities 
cd Tools
buildTools.sh 
cd ..

cp Tools/linuxTools/*   ./template_v2   # copy changePasswords, createPasswords, verifyPassword 
rm ./template_v2/*.pdb
zip -r template_v2.zip ./template_v2 

echo copy deploy/filetransferdock_v2_linux.tar.gz ........./tismoFileTransfer/template_v2
echo "On Sutlej:  docker load < filetransferdock_v2_linux.tar.gz"
echo copy the folder template_v2  to sutlej on ...../tismoFileTransfer/template_v2

echo the following works only on shankar's mac' -- copy these files manually to sutlej 
rsync template_v2.zip shankar@sutlej:/home/shankar
rsync deploy/filetransferdock_v2_linux.tar.gz shankar@sutlej:/home/shankar    

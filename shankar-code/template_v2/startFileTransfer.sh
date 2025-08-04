#!/bin/bash


# change the folder to the folder containing this script
cd "${0%/*}"

# get the name of the mantis docker container
foldername=${PWD##*/}  
echo Starting Tismo File Transfer for client $foldername
docker-compose up -d 

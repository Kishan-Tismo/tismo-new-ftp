#!/usr/bin/python3

# Creates a new Tismo File Transfer docker for a client
# usage:  createClient  tcaXXX 
# This script, 
#       - creates a folder /home/tismoFileTransfer/tcaXXX 
#       - creates startFileTransfer.sh and stopFileTransfer.sh scripts
#       - create appsettings.production.yml - contains client, tismo password hashes and salts
#       - creates the docker-compose.yml which is used by startFtp.sh
#       - port 3XXX is mapped to the docker image port 80 

import sys
import os
import shutil
import string
import random 
import subprocess

basedir = "/home/tismoFileTransfer";

def printUsage() :
    print("\nSets up a Tismo File Transfer installation for a client");
    print("\nUsage :", sys.argv[0], " <clientCode> " );

def copyFiles(srcFiles, targetFolder):
    [shutil.copy2(fname, targetFolder) for fname in srcFiles];

def get_immediate_subdirectories(a_dir):
    return [name for name in os.listdir(a_dir) if os.path.isdir(os.path.join(a_dir, name))]

def createUser(username, uid, gid):
    subprocess.call([ 'groupadd', '-g'+ gid,  username ]);
    subprocess.call( "adduser " + " --quiet --system --uid "+ uid + " --gid "+ gid + ' --disabled-password ' + username ,shell=True );

def writeNginxConfFile (confTemplate, templateFolder, clientFolder, clientNumber):
    with open (templateFolder + "/" + confTemplate) as infile:
        with open(clientFolder + "/tft" + str(clientNumber) + ".conf", 'w'  ) as outfile:
            for line in infile:
                line = line.replace("tftXXX", "tft"+str(clientNumber));
                line = line.replace("3XXX", "3"+str(clientNumber));
                outfile.write(line);


def writeDockerComposeFile(dockerComposeFile, templateFolder, clientFolder, clientNumber):
    with open(templateFolder + "/" + dockerComposeFile ) as infile:
        with open(clientFolder + "/" + dockerComposeFile, 'w' ) as outfile:
            for line in infile:
                line = line.replace( "3XXX", "3" + str(clientNumber) )
                outfile.write(line);


#  /home/tismoFileTransfer/tca035/appsettings.production.json
#  create the client and user password hashes
def writeConfigFile(clientcode, configFile2, clientFolder):
    subprocess.call(["./createPasswords" ,clientcode,  clientFolder + "/"  + configFile2] );

# assumes client name is of format tca097 
def getClientNumber(client):
    try:
        return client[-3:];
    except:
        print("\nError while parsing portnumber \n");
        print(client);
        return -1;
    return -1;

def getPortNumber(client):
    clientNumber = getClientNumber(client); # 097 
    portNumber = "3" + str(clientNumber);
    return portNumber;

#def createResetPasswordScript (templateFolder, targetFolder, clientCode) :
#    script = f"""#!/bin/bash
#    echo creates a new set of passwords for tismo and client {clientCode}
#    {templateFolder}/createPasswords.fsx {clientCode} {targetFolder}/appsettings.production.json
#    echo restart the docker instance.
#    """;
#    with open(targetFolder + "/resetPassword.sh", "w") as outfile:
#        outfile.write(script);
    

def main() : 
    if sys.version_info[0] < 3:
            raise Exception("Python 3 or a more recent version is required.")
    if len(sys.argv) < 2 :
        printUsage();
        return;
    client = sys.argv[1].lower();
    if not client.startswith("tca"):
        print("Error: Client codes start with tca");
        return -1;
    clientFolder = basedir + "/" +  client;
    clientShareFolder = clientFolder + "/files_v2";
    templateFolder = basedir + "/template_v2";
    print("\nClient code ", client);
    print("\nTarget folder ", clientFolder);


    clientNumber = getClientNumber(client); # 097 
    portNumber = getPortNumber(client);
    configFile = "appsettings.json";
    configFile2 = "appsettings.production.json"; # this is generated

    # make the target folder
    if not os.path.exists(clientFolder):
            os.makedirs(clientFolder)
    if not os.path.exists(clientShareFolder):
            os.umask(0);
            os.makedirs(clientShareFolder,0o700 );

    os.chown(clientShareFolder, int(portNumber), int(portNumber));

    # copy files that are not to be modified from template folder to client folder
    filenames = [ "startFileTransfer.sh", 
                  "stopFileTransfer.sh",
                  "appsettings.json",
                  "TismoFileServer.log"
                  ];
    files = [ templateFolder + "/" +  fname  for fname  in filenames ];
    copyFiles(files, clientFolder);

    # create appsettings.production.json - containing password hashes and salts
    configFileWithPath = clientFolder + "/" + configFile2;
    if not os.path.isfile(configFileWithPath) :   # Target config file exists ? avoid over writing the password hashes if it is already present
         portNumber = getPortNumber(client); # 3035 
         uid = portNumber; # uid = gid = portNumber
         gid = portNumber;
         createUser(client,uid, gid); 
         writeConfigFile(client, configFile2, clientFolder); # create appsettings.production.json for client, tismo password salt and hashe
    else:
        print("appsettings.production.json file already exists.\nDelete it and run this again to regenerate new passwords.");


    # copy docker-compose.yml
    dockerComposeFile = "docker-compose.yml";
    writeDockerComposeFile(dockerComposeFile, templateFolder, clientFolder, clientNumber);

    # copy nginx configuration file
    confTemplate = "tftXXX.conf";
    writeNginxConfFile (confTemplate, templateFolder, clientFolder, clientNumber);

    # create reset password script file
  #    createResetPasswordScript (templateFolder, clientFolder, client);


    print("Create DNS record tft" + str(clientNumber)+".tismo.tech to direct to this machine");
    print("On the router - forward the port" + portNumber + " to this machine");

    print("NGINX configuration file tft" + str(clientNumber) +  ".conf is created in " + clientFolder) 


main()


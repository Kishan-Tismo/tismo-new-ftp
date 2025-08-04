
Tismo File Transfer web application
-----------------------------------


----------------------------------------------------
Create a new instance of Tismo File Transfer for a client

Tismo has allocated a customer code for every customer.
This is of the form tca234.
Hence, to create a File transfer web application for this client,
just run 

./createClient.py tca234


This will create a folder /home/tismoFileTransfer/tca234   
The folder path can be modified in the creatClient.py file, by changing the value of 'basedir'
All the files required would be present in tca234 folder. 
Copy the client and tismo passwords and keep it safely. 
Send the tismo passwords to the Tismo team and client passwords to the client.

The ftp site would be https://tft234.tismo.tech    [ Note : It is tftxxx and not tcaxxx ]

The port 3234 would be mapped to this web app.
The NGINX configuration file tft234.conf should be copied to NGINX folder
   cp tft234  /etc/nginx/sites-enabled 
Then reload NGINX as follows,
   sudo /etc/init.d/nginx reload  
Or restart NGINX as follows
   sudo /etc/init.d/nginx restart 


On Google Cloud domain - add a DNS entry to
map  tft234.tismo.tech  -->   static IP address that comes to this server (sutlej)

----------------------------------------------
Start / Stop Tismo File Transfer web app

To start the web application run, 
    startFileTransfer.sh
This launches the docker container for this web application
docker ps ---> will list the all the running docker containers

After starting the web app, open a browser and check if https://tft234.tismo.tech is accessible.
Check login credentials for both client and tismo
Create folders,
     /FromTismo
     /From<ClientName>    
     e.g  /FromOwlet   -- better to put the client name here so Tismo users will know that the ftp site is correct


To stop the web application run,
    stopFileTransfer.sh 


-----------------------------------

Change Passwords

To change client password, go to the folder tcaXXX
../template_v2/changePasswords  client  appsettings.production.json 
Note down the client password and send it to the client


To change tismo password, go to the folder tcaXXX
../template_v2/changePasswords tismo appsettings.production.json

To change both the passwords, go to the folder tcaXXX
../template_v2/changePasswords both appsettings.production.json

----------------------------------------

Verify Passwords

To check if a given password is valid or not.
The password is stored in an encrypted form in 'hash' and 'salt' in appsettings.production.json
example:
"ClientHashedPassword":"F3zxrOatV6QBi3vX7HTVJyb4tJQQLyL9y+hCEub/JIc=",
"ClientSalt":"wJmE+QQk8tY08A6EYMlczw==",

So, to check say if the client password of "abcd-efgh-ijkl-mnop" is correct or not,
you can check it on the ftp web app Or use verifyPassword as follows,

verifyPassword  hash salt plainTextPassword

It is better to put all of them within quotes

../template_v2/verifyPassword  "F3zxrOatV6QBi3vX7HTVJyb4tJQQLyL9y+hCEub/JIc="   "wJmE+QQk8tY08A6EYMlczw=="   "abcd-efgh-ijkl-mnop"


--------------------------------------



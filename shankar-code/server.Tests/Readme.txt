
The server.Tests contain automated tests to verify the server side only.
The Angular GUI front end is not tested. 

It tests only the web API.

To run the test,

Ensure that https://tft999.tismo.tech is up and running.
The URL, username and passwords are hardcoded inside the file "FileTransferAPI.fs"
Change it if required.

One of the tests uploads and downloads a 1 GB file. That can take about 10+ minutes. 
      you can skip this test by changing a flag 'run1GBUploadTest' in "FileTransferAPI.fs"

// To run all the tests 
dotnet test 





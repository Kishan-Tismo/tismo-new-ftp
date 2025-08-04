FROM mcr.microsoft.com/dotnet/aspnet:8.0  
WORKDIR /app
COPY  .   .
ENTRYPOINT ["dotnet",  "server.dll" ] 

RUN cp /usr/share/zoneinfo/Asia/Kolkata /etc/localtime && \
    echo "Asia/Kolkata" > /etc/timezone 

    

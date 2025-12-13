FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY ./bin/publish ./
ENTRYPOINT ["dotnet", "./EventSourcedPM.dll"]

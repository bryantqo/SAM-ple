FROM mcr.microsoft.com/dotnet/aspnet:5.0 as base

WORKDIR /usr/app

COPY ./api /usr/app
COPY ./static /usr/app/static

EXPOSE 80

#TODO: The ssm path should just be a full path passed in
ENTRYPOINT ls; ls database; cd database; dotnet DatabaseDeployment.dll "${ssmPath}" All || echo "Database failed to deploy, still hosting..."; cd -; while [ true ]; do echo "Starting ULTRA API for ${env}"; dotnet API.dll "${ssmPath}"; echo "Application Exited. Restarting in 3s"; sleep 3s; done; 
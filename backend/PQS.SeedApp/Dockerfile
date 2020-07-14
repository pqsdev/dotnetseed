#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

### STAGE 1: BACK END BUILD ###
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["./PQS.SeedApp.Api/PQS.SeedApp.Api.csproj", "PQS.SeedApp.Api/"]
COPY ["./PQS.SeedApp.Business/PQS.SeedApp.Business.csproj", "PQS.SeedApp.Business/"]
RUN dotnet restore "PQS.SeedApp.Api/PQS.SeedApp.Api.csproj"
## en el dockerignore estan los directorios que no se copian
COPY . .
WORKDIR "/src/PQS.SeedApp.Api"
RUN dotnet build "PQS.SeedApp.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PQS.SeedApp.Api.csproj" -c Release -o /app/publish

### STAGE 3: FRONT END BUILD ###

## como es temporal para compilar usamos la version full en vez de la -alpine
#FROM node:lts AS fe-build 
#WORKDIR /app/frontend
#COPY ./frontend/package.json ./
#RUN npm install
## en el dockerignore estan los directorios que no se copian
#COPY ./frontend/ .
#RUN npm run build

### STAGE 4: COPIA TODO ###
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#COPY --from=fe-build /app/frontend/dist/test-app ./frontend
ENTRYPOINT ["dotnet", "PQS.SeedApp.Api.dll"]
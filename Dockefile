FROM mcr.microsoft.com/dotnet/aspnet:7.0-bullseye-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye AS build
WORKDIR /src
COPY *.sln .
COPY src/SysmonDotNet.Core/*.csproj ./src/SysmonDotNet.Core/
COPY src/SysmonDotNet.Infrastructure/*.csproj ./src/SysmonDotNet.Infrastructure/
COPY src/SysmonDotNet.Api/*.csproj ./src/SysmonDotNet.Api/
RUN dotnet restore
COPY . .
WORKDIR /src/src/SysmonDotNet.Api
RUN dotnet publish -c Release -o /app/publish

FROM base
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet","SysmonDotNet.Api.dll"]

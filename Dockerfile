# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/RagMcpServer/*.csproj ./src/RagMcpServer/
RUN dotnet restore src/RagMcpServer/RagMcpServer.csproj

# Copy everything else and build
COPY src/RagMcpServer/. ./src/RagMcpServer/
WORKDIR /app/src/RagMcpServer
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/src/RagMcpServer/out .
ENTRYPOINT ["dotnet", "RagMcpServer.dll"]

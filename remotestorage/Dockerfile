# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore (cache friendly)
COPY src/remotestorage/remotestorage.csproj ./remotestorage/
RUN dotnet restore ./remotestorage/remotestorage.csproj

# Copy the rest of the source
COPY src/remotestorage ./remotestorage

# Go into the project folder (absolute path!)
WORKDIR /src/remotestorage

# Build and publish
RUN dotnet publish -c Release -o /app/publish

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY appsettings.json . 

EXPOSE 8080

ENTRYPOINT ["dotnet", "remotestorage.dll"]

# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution + restore
COPY remotestorage.sln ./
COPY src/remotestorage/remotestorage.csproj ./src/remotestorage/
RUN dotnet restore ./src/remotestorage/remotestorage.csproj

# Copy all source and build
COPY . .
WORKDIR /src/src/remotestorage
RUN dotnet publish -c Release -o /app/publish

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080

# Run the app
ENTRYPOINT ["dotnet", "remotestorage.dll"]

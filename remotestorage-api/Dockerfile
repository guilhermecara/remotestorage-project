# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore ./remotestorage-api.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o /app

# Use the official ASP.NET 9 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

# Expose port 5050
EXPOSE 5050

# Run the app
ENTRYPOINT ["dotnet", "remotestorage-api.dll"]

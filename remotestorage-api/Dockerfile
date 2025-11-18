# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY remotestorage-api.csproj ./
RUN dotnet restore ./remotestorage-api.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o /app

# Use the official ASP.NET 9 runtime DISTROLESS image for running
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine
WORKDIR /app
COPY --from=build /app ./

ENV POSTGRES_USER=guilhermeuser
ENV POSTGRES_PASSWORD=LIbolo0$
ENV POSTGRES_DB=database
ENV DB_HOST=database
ENV DB_PORT=5432 

# Expose port 5050
EXPOSE 5050

# Run the app
ENTRYPOINT ["dotnet", "remotestorage-api.dll"]

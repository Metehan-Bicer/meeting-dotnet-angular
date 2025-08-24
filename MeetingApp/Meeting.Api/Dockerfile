# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the solution and project files
COPY *.sln .
COPY Meeting.Api/*.csproj ./Meeting.Api/
COPY Meeting.Application/*.csproj ./Meeting.Application/
COPY Meeting.Domain/*.csproj ./Meeting.Domain/
COPY Meeting.Infrastructure/*.csproj ./Meeting.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the code
COPY Meeting.Api/. ./Meeting.Api/
COPY Meeting.Application/. ./Meeting.Application/
COPY Meeting.Domain/. ./Meeting.Domain/
COPY Meeting.Infrastructure/. ./Meeting.Infrastructure/

# Build the application
WORKDIR /app/Meeting.Api
RUN dotnet publish -c Release -o out

# Use the official .NET runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Create directory for SQLite database
RUN mkdir -p /app/database

# Copy published files
COPY --from=build /app/Meeting.Api/out .

# Expose the port
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "Meeting.Api.dll"]
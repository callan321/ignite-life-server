# Use ASP.NET runtime image as base for running app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
# Expose HTTP port only (no HTTPS in container)
EXPOSE 8080

# Use .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies
COPY IgniteLifeApi/IgniteLifeApi.csproj IgniteLifeApi/
RUN dotnet restore "IgniteLifeApi/IgniteLifeApi.csproj"

# Copy all source files
COPY IgniteLifeApi/ IgniteLifeApi/

WORKDIR "/src/IgniteLifeApi"

# Build the project
RUN dotnet build "IgniteLifeApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the app to a folder
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "IgniteLifeApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image to run the app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080

# Run the app
ENTRYPOINT ["dotnet", "IgniteLifeApi.dll"]

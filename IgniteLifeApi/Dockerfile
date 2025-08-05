# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Build and publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the source
COPY IgniteLifeApi/ IgniteLifeApi/

# Publish directly (no separate build step)
WORKDIR "/src/IgniteLifeApi"
RUN dotnet publish "IgniteLifeApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "IgniteLifeApi.dll"]

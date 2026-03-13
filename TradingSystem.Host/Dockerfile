# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["TradingSystem.Host/TradingSystem.Host.csproj", "TradingSystem.Host/"]
COPY ["TradingSystem.Core/TradingSystem.Core.csproj", "TradingSystem.Core/"]
COPY ["TradingSystem.Infrastructure/TradingSystem.Infrastructure.csproj", "TradingSystem.Infrastructure/"]

RUN dotnet restore "TradingSystem.Host/TradingSystem.Host.csproj"

# Copy the rest of the application source code
COPY . .

# Build the application
WORKDIR "/src"
RUN dotnet build "TradingSystem.Host/TradingSystem.Host.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "TradingSystem.Host/TradingSystem.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the ASP.NET runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy the published application from the publish stage
COPY --from=publish /app/publish .

# Set default environment variables (can be overridden in docker-compose.yml)
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose the HTTP port
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "TradingSystem.Host.dll"]

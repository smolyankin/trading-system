# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY TradingSystem.Emulator.csproj .
RUN dotnet restore "TradingSystem.Emulator.csproj"

# Copy the rest of the application
COPY . .

# Build the application
WORKDIR "/src"
RUN dotnet build "TradingSystem.Emulator.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "TradingSystem.Emulator.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the runtime image for the final stage
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

# Copy the published application from the publish stage
COPY --from=publish /app/publish .

# Set default environment variables (can be overridden)
ENV EMITTER_FORMAT=1
ENV PORT=8080
ENV TICKS_PER_SECOND=25
ENV ASPNETCORE_URLS=http://+:8080

# Expose the WebSocket port
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "TradingSystem.Emulator.dll"]

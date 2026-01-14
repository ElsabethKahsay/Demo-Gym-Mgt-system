# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore as distinct layers
COPY ["SoulFitness.Web/SoulFitness.Web.csproj", "SoulFitness.Web/"]
COPY ["SoulFitness.Abstractions/SoulFitness.Abstractions.csproj", "SoulFitness.Abstractions/"]
COPY ["SoulFitness.DataObjects/SoulFitness.DataObjects.csproj", "SoulFitness.DataObjects/"]
COPY ["SoulFitness.Utilities/SoulFitness.Utilities.csproj", "SoulFitness.Utilities/"]

RUN dotnet restore "SoulFitness.Web/SoulFitness.Web.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/SoulFitness.Web"
RUN dotnet build "SoulFitness.Web.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "SoulFitness.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create the FileUploads directory
RUN mkdir -p /app/FileUploads

ENTRYPOINT ["dotnet", "SoulFitness.Web.dll"]

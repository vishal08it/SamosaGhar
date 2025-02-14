# Use the official .NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Use the .NET SDK to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SamosaGhar.csproj", "./"]
RUN dotnet restore "./SamosaGhar.csproj"

# Copy and publish the project
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Use runtime image for execution
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SamosaGhar.dll"]

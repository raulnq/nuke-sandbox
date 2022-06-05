FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore "./nuke-sandbox-app/nuke-sandbox-app.csproj"
# Build and publish a release
RUN dotnet publish "./nuke-sandbox-app/nuke-sandbox-app.csproj" -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "nuke-sandbox-app.dll"]
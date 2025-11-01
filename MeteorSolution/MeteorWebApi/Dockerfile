# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY MeteorSolution/Shared/Shared.csproj MeteorSolution/Shared/
COPY MeteorSolution/MeteorWebApi/MeteorWebApi.csproj MeteorSolution/MeteorWebApi/

RUN dotnet restore MeteorSolution/MeteorWebApi/MeteorWebApi.csproj

COPY . .

WORKDIR /src/MeteorSolution/MeteorWebApi
RUN dotnet publish -c Release -o /app/out

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MeteorWebApi.dll"]
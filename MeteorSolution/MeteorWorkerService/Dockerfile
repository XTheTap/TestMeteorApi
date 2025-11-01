# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY MeteorSolution/Shared/Shared.csproj MeteorSolution/Shared/
COPY MeteorSolution/MeteorWorkerService/MeteorWorkerService.csproj MeteorSolution/MeteorWorkerService/

RUN dotnet restore MeteorSolution/MeteorWorkerService/MeteorWorkerService.csproj

COPY . .

WORKDIR /src/MeteorSolution/MeteorWorkerService
RUN dotnet publish -c Release -o /app/out

# Runtime
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "MeteorWorkerService.dll"]

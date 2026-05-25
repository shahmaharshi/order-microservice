FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/InventoryService.API/InventoryService.API.csproj", "src/InventoryService.API/"]
COPY ["src/InventoryService.Application/InventoryService.Application.csproj", "src/InventoryService.Application/"]
COPY ["src/InventoryService.Domain/InventoryService.Domain.csproj", "src/InventoryService.Domain/"]
COPY ["src/InventoryService.Infrastructure/InventoryService.Infrastructure.csproj", "src/InventoryService.Infrastructure/"]

RUN dotnet restore "src/InventoryService.API/InventoryService.API.csproj"

COPY . .
RUN dotnet publish "src/InventoryService.API/InventoryService.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InventoryService.API.dll"]

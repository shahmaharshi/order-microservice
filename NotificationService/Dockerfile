FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/NotificationService.API/NotificationService.API.csproj", "src/NotificationService.API/"]
COPY ["src/NotificationService.Application/NotificationService.Application.csproj", "src/NotificationService.Application/"]
COPY ["src/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj", "src/NotificationService.Infrastructure/"]

RUN dotnet restore "src/NotificationService.API/NotificationService.API.csproj"

COPY . .
RUN dotnet publish "src/NotificationService.API/NotificationService.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.API.dll"]

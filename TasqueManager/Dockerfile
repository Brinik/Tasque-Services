# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
EXPOSE 80

# Копируем файлы решения и проектов
COPY ["TasqueManager.sln", "."]
COPY ["TasqueManager.WebHost/TasqueManager.WebHost.csproj", "TasqueManager.WebHost/"]
COPY ["TasqueManager.Abstractions/TasqueManager.Abstractions.csproj", "TasqueManager.Abstractions/"]
COPY ["TasqueManager.Contracts/TasqueManager.Contracts.csproj", "TasqueManager.Contracts/"]
COPY ["TasqueManager.Domain/TasqueManager.Domain.csproj", "TasqueManager.Domain/"]
COPY ["TasqueManager.Infrastructure/TasqueManager.Infrastructure.csproj", "TasqueManager.Infrastructure/"]

# Восстанавливаем зависимости
RUN dotnet restore "TasqueManager.sln"

# Копируем весь исходный код
COPY . .

# Собираем и публикуем проект
WORKDIR "/src/TasqueManager.WebHost"
RUN dotnet publish "TasqueManager.WebHost.csproj" -c Release -o /app/publish

# Финальный этап (рантайм)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TasqueManager.WebHost.dll"]
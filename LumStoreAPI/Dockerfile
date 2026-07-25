FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LumStoreAPI/LumStoreAPI.csproj", "LumStoreAPI/"]
COPY ["LumStoreAPI.Application/LumStoreAPI.Application.csproj", "LumStoreAPI.Application/"]
COPY ["LumStoreAPI.Infrastructure/LumStoreAPI.Infrastructure.csproj", "LumStoreAPI.Infrastructure/"]
COPY ["LumStoreAPI.Libraries/LumStoreAPI.Libraries.csproj", "LumStoreAPI.Libraries/"]
COPY ["LumStoreAPI.Core/LumStoreAPI.Core.csproj", "LumStoreAPI.Core/"]
COPY ["LumStoreAPI.SDK/LumStoreAPI.SDK.csproj", "LumStoreAPI.SDK/"]
COPY ["LumStoreAPI.DataEngine/LumStoreAPI.DataEngine.csproj", "LumStoreAPI.DataEngine/"]
COPY ["LumStoreAPI.Tasks/LumStoreAPI.Tasks.csproj", "LumStoreAPI.Tasks/"]
RUN dotnet restore "LumStoreAPI/LumStoreAPI.csproj"
COPY . .
WORKDIR "/src/LumStoreAPI"
RUN dotnet build "./LumStoreAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LumStoreAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LumStoreAPI.dll"]

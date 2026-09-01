FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Api.Instartups.Auth/Api.Instartups.Auth.csproj", "Api.Instartups.Auth/"]
RUN dotnet restore "Api.Instartups.Auth/Api.Instartups.Auth.csproj"
COPY . .
WORKDIR "/src/Api.Instartups.Auth"
RUN dotnet build "./Api.Instartups.Auth.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Api.Instartups.Auth.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.Instartups.Auth.dll"]

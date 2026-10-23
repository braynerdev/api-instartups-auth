# syntax=docker/dockerfile:1

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
WORKDIR /src

# Restore em camada separada: só é refeito quando o .csproj muda
COPY global.json ./
COPY Api.Instartups.Auth/Api.Instartups.Auth.csproj Api.Instartups.Auth/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore Api.Instartups.Auth/Api.Instartups.Auth.csproj -a $TARGETARCH

COPY Api.Instartups.Auth/ Api.Instartups.Auth/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish Api.Instartups.Auth/Api.Instartups.Auth.csproj \
        -c Release -a $TARGETARCH --no-restore --self-contained false \
        -o /app \
        -p:UseAppHost=false \
        -p:DebugType=none \
        -p:DebugSymbols=false \
        -p:GenerateDocumentationFile=false \
        -p:SatelliteResourceLanguages=pt-BR


# Chiseled: sem shell/apt, roda como usuário não-root (app), imagem mínima.
# "-extra" inclui ICU e tzdata (culturas e fusos horários).
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS final
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_gcServer=0 \
    DOTNET_GCConserveMemory=5 \
    DOTNET_TieredPGO=1
WORKDIR /app
COPY --from=build --chown=app:app /app ./
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "Api.Instartups.Auth.dll"]

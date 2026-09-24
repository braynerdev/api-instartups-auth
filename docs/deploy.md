# Deploy

## Dockerfile

`Api.Instartups.Auth/Dockerfile` é um build multi-stage:

```dockerfile
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
```

- Imagem de runtime: `mcr.microsoft.com/dotnet/aspnet:10.0`; imagem de build: `mcr.microsoft.com/dotnet/sdk:10.0`.
- Roda como `$APP_UID` (usuário não-root, padrão das imagens oficiais do .NET).
- Expõe as portas **8080** e **8081**.
- `BUILD_CONFIGURATION` é configurável via build-arg, com padrão `Release`.

## `compose.yaml`

Na raiz do repositório (`api-instartups-auth/compose.yaml`):

```yaml
services:
  api.instartups.auth:
    image: api.instartups.auth
    build:
      context: .
      dockerfile: Api.Instartups.Auth/Dockerfile
```

Comandos:

```bash
docker compose build
docker compose up
```

**Importante:** este arquivo define **apenas o serviço da API** — não há um serviço de PostgreSQL declarado no compose. Isso significa que:
- O banco precisa estar disponível separadamente (outro container, serviço gerenciado, ou instância local).
- A string de conexão padrão em `appsettings.json` aponta para `Host=localhost`, o que **não funciona** de dentro de um container apontando para um Postgres rodando fora dele — é necessário sobrescrever `ConnectionStrings__PostgresConnection` (ou equivalente) com o host/porta corretos ao subir via Docker.

## CI/CD

O repositório possui a pasta `.github/workflows/`, mas ela está **vazia** — não há nenhum pipeline de CI/CD (build, teste, lint, publish de imagem) configurado atualmente. Ver [`limitacoes-conhecidas.md`](./limitacoes-conhecidas.md).

## Checklist mínimo para subir em um novo ambiente

1. Ter um PostgreSQL acessível e configurar `ConnectionStrings:PostgresConnection` apontando para ele.
2. Gerar (ou reutilizar) um par de chaves RSA para o JWT e configurar `Jwt:PrivateKeyPath`/`Jwt:PublicKeyPath` — nunca reaproveitar as chaves de exemplo versionadas no repositório em produção.
3. Definir uma chave AES própria em `Cursor:SecretKey` (base64) — a de `appsettings.Development.json` é só para desenvolvimento.
4. Decidir se um usuário admin deve ser semeado automaticamente, preenchendo `AdminUser:Email`/`UserName`/`Password` com credenciais próprias do ambiente (ou deixando em branco para pular o seed).
5. Rodar `dotnet ef database update` (ou aplicar a migration por outro meio) antes do primeiro start, para garantir que o schema `auth` e suas tabelas existam.
6. Construir e rodar a imagem Docker (`docker compose build && docker compose up`) ou publicar via `dotnet publish` diretamente.

Nenhum desses passos é automatizado hoje (não há script de setup, `Makefile` ou pipeline) — é um processo manual baseado nos arquivos de configuração descritos em [`configuracao-e-execucao.md`](./configuracao-e-execucao.md).

# Configuração e execução

## Pré-requisitos

- .NET SDK 10 (o `.csproj` alvo é `net10.0`).
- PostgreSQL acessível (localmente ou via container).
- Um par de chaves RSA para assinatura do JWT (localmente em `INSTARTUPS/keys/`, ver seção [Chaves JWT](#chaves-jwt-rsa)).

Localmente, toda a configuração é feita através de `appsettings.json` / `appsettings.Development.json` (providers padrão de configuração do ASP.NET Core), podendo ser sobrescrita por variáveis de ambiente ou `dotnet user-secrets` seguindo a convenção `Secao__Chave`. No Docker, só o que muda por ambiente ou é segredo vem de `infra/.env` (modelo em `infra/.env.example`, ver [`deploy.md`](./deploy.md)).

## Chaves de configuração

Todas as chaves lidas pela aplicação, com origem em `appsettings.json` (produção/base) e sobrescritas em `appsettings.Development.json`:

| Chave | `appsettings.json` (base) | `appsettings.Development.json` | Uso |
|---|---|---|---|
| `ConnectionStrings:PostgresConnection` | vazio (`""`) | string de conexão local | String de conexão do PostgreSQL |
| `AppDoc:NomeApi` | `API de Autenticação InStartups` | — | Título usado no Swagger |
| `AppDoc:DescricaoApi` | `API responsável pelo gerenciamento de autenticação e acesso aos serviços do sistema InStartups.` | — | Descrição usada no Swagger |
| `Jwt:Issuer` / `Jwt:Audience` | vazios (`""`) | `minha-api` / `meu-app` | Emissor (`iss`) e público (`aud`) do token; iguais na Command e na Query |
| `Jwt:PrivateKeyPath` | `/run/keys/jwt-private.pem` | caminho local da chave | Caminho da chave privada RSA usada para assinar o access token |
| `Jwt:PublicKeyPath` | `/run/keys/jwt-public.pem` | caminho local da chave | Caminho da chave pública RSA usada para validar o token recebido |
| `Jwt:ExpirationMinutes` | `60` | `10000` | Tempo de vida do access token, em minutos |
| `RefreshToken:ExpirationDays` | `30` | — | Tempo de vida da sessão/refresh token, em dias |
| `Cursor:SecretKey` | vazio (`""`) | chave base64 de exemplo preenchida | Chave AES-256 (base64) usada para criptografar/decriptar cursores de paginação |
| `AdminUser:Email` / `UserName` / `Password` | todos vazios | `admin@instartups.dev` / `admin` / `Admin@123` | Credenciais do usuário admin semeado na inicialização (ver [Seed de dados](#seed-de-dados-na-inicialização)) |
| `Serilog:MinimumLevel` | `Information`; `Microsoft` e `Wolverine` em `Warning`; `Microsoft.Hosting.Lifetime` em `Information`; `Api.Instartups.Auth.Middleware` em `Warning` | — | Níveis de log (iguais nas 3 APIs) |
| `Serilog:WriteTo` | `Console` sem tema | `Console` com tema ANSI + `Debug` | Sinks de log |
| `Serilog:Properties:Application` | `Instartups.Auth` | — | Propriedade adicionada a todo log |
| `AllowedHosts` | `*` | — | Padrão do ASP.NET Core |

"—" significa que o `appsettings.Development.json` não sobrescreve e vale o valor base.

> **Atenção:** o `appsettings.json` versionado não traz credenciais (connection string, issuer/audience, cursor e admin vazios). No container esses valores vêm do `infra/.env` (ver [`deploy.md`](./deploy.md)). Os valores locais ficam no `appsettings.Development.json`, que é ignorado pelo git.

O log usa o mesmo padrão da Command e da Query (`SerilogConfig.cs`): configuração lida da seção `Serilog`, enriquecida com `LogContext`, e um log por requisição (`UseSerilogRequestLogging`) com `HostRequisicao`, `CaminhoRequisicao` e `Metodo`. O nível é `Error` para exceção ou status ≥ 500, `Warning` acima de 1000 ms e `Information` nos demais casos.

## Chaves JWT (RSA)

O token de acesso é assinado com **RS256** (RSA + SHA-256), não com um segredo simétrico. Os arquivos apontados por `Jwt:PrivateKeyPath` e `Jwt:PublicKeyPath` (no container, `/run/keys/jwt-private.pem` e `/run/keys/jwt-public.pem`; localmente, a pasta `INSTARTUPS/keys/`) são lidos em tempo de execução:
- `GenerateJwtService` lê `Jwt:PrivateKeyPath` para assinar cada novo token (a chave é lida do disco a cada emissão, não é cacheada).
- `AuthenticationConf` lê `Jwt:PublicKeyPath` uma vez, na inicialização, para configurar a validação dos tokens recebidos.

Para gerar um novo par de chaves compatível (formato PEM, RSA), por exemplo:

```bash
openssl genrsa -out jwt-private.pem 2048
openssl rsa -in jwt-private.pem -pubout -out jwt-public.pem
```

e apontar `Jwt:PrivateKeyPath`/`Jwt:PublicKeyPath` para os novos arquivos.

## Seed de dados na inicialização

A cada start da aplicação (`Program.cs`, antes do pipeline HTTP ser montado), `IdentitySeeder.SeedIdentityDataAsync` roda duas etapas, de forma idempotente:

1. **Seed de roles/permissões:** garante que cada entrada de `PermissionConst.All` (`Users.View`, `Users.Create`, `Users.Update`, `Users.Delete`, `Sessions.Revoke`, `Admin`) exista como `IdentityRole`.
2. **Seed do usuário admin:** só executa se `AdminUser:Email`, `AdminUser:UserName` e `AdminUser:Password` estiverem **todos** preenchidos na configuração. Se algum estiver vazio, a etapa é pulada silenciosamente. Se as credenciais estiverem presentes, cria o usuário (caso não exista, por e-mail) e garante que ele tenha a role `Admin`.

Em `appsettings.Development.json` essas credenciais já vêm preenchidas (`admin@instartups.dev` / `admin` / `Admin@123`), então rodar em ambiente de desenvolvimento sempre resulta em um usuário admin pronto para uso. Em `appsettings.json` (base/produção) elas estão vazias, ou seja, por padrão nenhum admin é criado fora do ambiente de desenvolvimento — é necessário fornecer essas três chaves explicitamente.

## Rodando localmente com `dotnet`

```bash
cd Api.Instartups.Auth
dotnet restore
dotnet run
```

As migrations pendentes são aplicadas automaticamente na inicialização (`MigrationsConfig.ApplyMigrationsAsync`), então basta o PostgreSQL estar acessível.

O perfil de execução padrão (`Properties/launchSettings.json`) sobe a aplicação em `http://localhost:8080` com `ASPNETCORE_ENVIRONMENT=Development`. Não há redirecionamento HTTPS habilitado (`UseHttpsRedirection()` está comentado em `Program.cs`).

Com a aplicação em ambiente de desenvolvimento, a documentação interativa fica disponível em:
- Swagger UI: `http://localhost:8080/swagger-ui`
- Documento OpenAPI gerado pelo Swashbuckle: `http://localhost:8080/swagger/v1/swagger.json`
- Documento OpenAPI nativo do ASP.NET Core: `http://localhost:8080/openapi/v1.json`

## Rodando via Docker

Os arquivos ficam em `infra/` (`Dockerfile`, `Dockerfile.dockerignore`, `docker-compose.yml`, `.env.example`). O compose é uma stack do Docker Swarm que sobe só a API, em `Production`, puxando a imagem do GHCR. Ele não publica porta: a API é exposta pelo Traefik em `https://api.instartups.tech`. O banco é gerenciado/externo.

No container, o que muda por ambiente vem do `infra/.env` (mapeado no compose para `Secao__Chave`), o resto vem do `appsettings.json`, e as chaves RSA são montadas como volume somente leitura em `/run/keys/`. Variáveis, Traefik, formato Swarm, detalhes da imagem e banco externo estão em [`deploy.md`](./deploy.md).

## Migrations do EF Core

Existe uma única migration no projeto: `Migrations/20260907222626_FirstMigrate.cs`, que cria as tabelas padrão do ASP.NET Identity e a tabela `UserSessions`, todas no schema `auth` (ver [`modelo-de-dados.md`](./modelo-de-dados.md)). Para gerar uma nova migration ou aplicar as existentes:

```bash
dotnet ef migrations add NomeDaMigration --project Api.Instartups.Auth
dotnet ef database update --project Api.Instartups.Auth
```

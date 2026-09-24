# Configuração e execução

## Pré-requisitos

- .NET SDK 10 (o `.csproj` alvo é `net10.0`).
- PostgreSQL acessível (localmente ou via container).
- Um par de chaves RSA para assinatura do JWT (já existe um em `Api.Instartups.Auth/Keys/`, ver seção [Chaves JWT](#chaves-jwt-rsa)).

Não existe arquivo `.env`/`.env.example` no projeto. Toda a configuração é feita através de `appsettings.json` / `appsettings.Development.json` (providers padrão de configuração do ASP.NET Core), podendo ser sobrescrita por variáveis de ambiente ou `dotnet user-secrets` seguindo a convenção `Secao__Chave`.

## Chaves de configuração

Todas as chaves lidas pela aplicação, com origem em `appsettings.json` (produção/base) e sobrescritas em `appsettings.Development.json`:

| Chave | `appsettings.json` (base) | `appsettings.Development.json` | Uso |
|---|---|---|---|
| `ConnectionStrings:PostgresConnection` | `Host=localhost;Port=5432;Database=instartups_db;Username=postgres;Password=1208` | mesma, porém `Port=5433` | String de conexão do PostgreSQL |
| `AppDoc:NomeApi` | `API de Autenticação InStartups` | igual | Título usado no Swagger |
| `AppDoc:DescricaoApi` | `API responsável pelo gerenciamento de autenticação e acesso aos serviços do sistema InStartups.` | igual | Descrição usada no Swagger |
| `Jwt:Issuer` | `minha-api` | igual | Emissor (`iss`) validado no token |
| `Jwt:Audience` | `meu-app` | igual | Configurado mas **não validado nem incluído no token** (ver [`limitacoes-conhecidas.md`](./limitacoes-conhecidas.md)) |
| `Jwt:PrivateKeyPath` | `Keys/jwt-private.pem` | igual | Caminho da chave privada RSA usada para assinar o access token |
| `Jwt:PublicKeyPath` | `Keys/jwt-public.pem` | igual | Caminho da chave pública RSA usada para validar o token recebido |
| `Jwt:ExpirationMinutes` | `60` | igual | Tempo de vida do access token, em minutos |
| `RefreshToken:ExpirationDays` | `30` | igual | Tempo de vida da sessão/refresh token, em dias |
| `Cursor:SecretKey` | vazio (`""`) | chave base64 de exemplo preenchida | Chave AES-256 (base64) usada para criptografar/decriptar cursores de paginação |
| `AdminUser:Email` / `UserName` / `Password` | todos vazios | `admin@instartups.dev` / `admin` / `Admin@123` | Credenciais do usuário admin semeado na inicialização (ver [Seed de dados](#seed-de-dados-na-inicialização)) |
| `Serilog:MinimumLevel:Default` | `Fatal` | igual | Nível mínimo de log padrão |
| `Serilog:MinimumLevel:Override:Wolverine` | `Fatal` | igual | Nível de log específico para o namespace do Wolverine |
| `Serilog:MinimumLevel:Override:Api.Instartups.Auth.Middleware` | `Warning` | igual | Nível de log específico para o middleware de exceções |
| `AllowedHosts` | `*` | — | Padrão do ASP.NET Core |

> **Atenção:** os valores acima — incluindo a senha do banco (`1208`), a senha do admin de desenvolvimento (`Admin@123`) e a chave AES de cursor de desenvolvimento — estão versionados nos arquivos `appsettings*.json` deste repositório. Eles servem apenas para ambiente local/desenvolvimento; qualquer implantação real precisa sobrescrever essas chaves com segredos próprios (variáveis de ambiente, Key Vault, `dotnet user-secrets` etc.). Ver [`limitacoes-conhecidas.md`](./limitacoes-conhecidas.md).

## Chaves JWT (RSA)

O token de acesso é assinado com **RS256** (RSA + SHA-256), não com um segredo simétrico. Os arquivos `Keys/jwt-private.pem` e `Keys/jwt-public.pem` (relativos à raiz do projeto `Api.Instartups.Auth/`) são lidos em tempo de execução:
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
dotnet ef database update   # aplica a migration existente (requer PostgreSQL acessível)
dotnet run
```

O perfil de execução padrão (`Properties/launchSettings.json`) sobe a aplicação em `http://localhost:8080` com `ASPNETCORE_ENVIRONMENT=Development`. Não há redirecionamento HTTPS habilitado (`UseHttpsRedirection()` está comentado em `Program.cs`).

Com a aplicação em ambiente de desenvolvimento, a documentação interativa fica disponível em:
- Swagger UI: `http://localhost:8080/swagger-ui`
- Documento OpenAPI gerado pelo Swashbuckle: `http://localhost:8080/swagger/v1/swagger.json`
- Documento OpenAPI nativo do ASP.NET Core: `http://localhost:8080/openapi/v1.json`

## Rodando via Docker

O repositório traz um `Dockerfile` multi-stage (`Api.Instartups.Auth/Dockerfile`) e um `compose.yaml` na raiz:

```yaml
services:
  api.instartups.auth:
    image: api.instartups.auth
    build:
      context: .
      dockerfile: Api.Instartups.Auth/Dockerfile
```

```bash
docker compose build
docker compose up
```

Pontos de atenção:
- O `compose.yaml` **não** define um serviço de PostgreSQL — o banco precisa estar disponível separadamente, e a string de conexão (que por padrão aponta para `Host=localhost`) precisa ser sobrescrita para o host/porta corretos ao rodar em container (ex.: via variável de ambiente `ConnectionStrings__PostgresConnection`).
- O Dockerfile expõe as portas `8080` e `8081` (imagem base `mcr.microsoft.com/dotnet/aspnet:10.0`); build usa `mcr.microsoft.com/dotnet/sdk:10.0`.
- Mais detalhes em [`deploy.md`](./deploy.md).

## Migrations do EF Core

Existe uma única migration no projeto: `Migrations/20260907222626_FirstMigrate.cs`, que cria as tabelas padrão do ASP.NET Identity e a tabela `UserSessions`, todas no schema `auth` (ver [`modelo-de-dados.md`](./modelo-de-dados.md)). Para gerar uma nova migration ou aplicar as existentes:

```bash
dotnet ef migrations add NomeDaMigration --project Api.Instartups.Auth
dotnet ef database update --project Api.Instartups.Auth
```

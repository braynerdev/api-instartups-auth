# Deploy

Arquivos em `infra/`: `Dockerfile`, `Dockerfile.dockerignore`, `docker-compose.yml`, `.env.example`. O compose sobe **só a API**; o banco é externo e configurado pelo `infra/.env`. O contexto do build é a raiz do repositório (`context: ..`), e o BuildKit aplica o `Dockerfile.dockerignore` por estar ao lado do `Dockerfile`.

A imagem é publicada no GHCR pelo GitHub Actions — ver [`ci-cd.md`](./ci-cd.md).

## Subindo com Docker

```bash
cd infra
cp .env.example .env   # preencha banco, chaves JWT, cursor e admin
docker compose up -d --build
docker compose logs -f api-auth
```

A partir da raiz, use `docker compose -f infra/docker-compose.yml ...`.

A API fica em `http://localhost:${API_PORT}` (padrão `8080`, a mesma porta que o frontend espera em `AUTH_API_URL`) e roda em `Production`. Na inicialização ela aplica as migrations e roda o seed de roles/admin (ver [`configuracao-e-execucao.md`](./configuracao-e-execucao.md#seed-de-dados-na-inicialização)). Para conferir: `GET /api/healthcheck`.

| Variável | Uso |
| --- | --- |
| `IMAGE_TAG` | Tag da imagem do GHCR (`latest`, `develop`, `1.2.3`, `sha-xxxxxxx`...). |
| `ASPNETCORE_ENVIRONMENT` | Ambiente (padrão `Production`; o Swagger só existe em `Development`). |
| `API_PORT` | Porta publicada no host. |
| `TZ` | Fuso horário do container. |
| `DB_HOST`, `DB_PORT`, `DB_NAME`, `DB_USER`, `DB_PASSWORD` | Montam `ConnectionStrings:PostgresConnection`. |
| `DB_MIN_POOL_SIZE`, `DB_MAX_POOL_SIZE` | Tamanho do pool do Npgsql. |
| `JWT_PRIVATE_KEY_FILE` | Caminho no host da chave privada, montada como somente leitura em `/run/keys/jwt-private.pem` (`Jwt:PrivateKeyPath`). |
| `JWT_PUBLIC_KEY_FILE` | Caminho no host da chave pública, montada em `/run/keys/jwt-public.pem` (`Jwt:PublicKeyPath`). É a mesma usada pela Command e pela Query. |
| `JWT_ISSUER`, `JWT_AUDIENCE` | `Jwt:Issuer` / `Jwt:Audience`. O issuer precisa ser igual ao configurado na Command e na Query. |
| `JWT_EXPIRATION_MINUTES` | `Jwt:ExpirationMinutes` (padrão `60`). |
| `REFRESH_TOKEN_EXPIRATION_DAYS` | `RefreshToken:ExpirationDays` (padrão `30`). |
| `CURSOR_SECRET_KEY` | `Cursor:SecretKey`, chave AES-256 em base64. Gere com `openssl rand -base64 32`. |
| `ADMIN_EMAIL`, `ADMIN_USERNAME`, `ADMIN_PASSWORD` | `AdminUser:*`. Se algum ficar vazio, o admin não é criado. |
| `API_CPUS`, `API_MEMORY` | Limites de recursos do container. |

Se `JWT_PRIVATE_KEY_FILE`/`JWT_PUBLIC_KEY_FILE` ficarem vazios, o compose usa `../../keys/jwt-*.pem` (a pasta `backend/keys/` ao lado dos repositórios).

## Detalhes da imagem

- Build multi-stage com o restore em camada separada e cache do NuGet (BuildKit), então mudanças só no código não refazem o restore.
- Runtime `aspnet:10.0-noble-chiseled-extra`: sem shell nem gerenciador de pacotes, usuário não-root, com ICU e tzdata. Por não ter shell, não há `HEALTHCHECK`.
- O `Dockerfile.dockerignore` deixa de fora `appsettings.Development.json`, `launchSettings.json`, `.env` e `*.pem`: nenhum segredo entra na imagem; tudo vem por variável de ambiente e volume.
- O Wolverine roda em `TypeLoadMode.Static` fora de `Development` e exige o código pré-gerado em `Api.Instartups.Auth/Internal/Generated/`. Sempre que um handler mudar, regenere com `dotnet run --project Api.Instartups.Auth -- codegen write`; sem isso o container falha na inicialização.
- No compose, o container roda com filesystem somente leitura (`/tmp` em tmpfs), sem capabilities e com `no-new-privileges`. As chaves de Data Protection ficam em memória, o que não afeta a API (autenticação é por JWT e refresh token no banco).
- A connection string usa `Gss Encryption Mode=Disable`: a imagem chiseled não tem `libgssapi_krb5`, e sem isso o Npgsql loga erro ao tentar GSS.
- O projeto compose se chama `instartups`, o mesmo da Command e da Query.

## Banco na própria máquina

Use `DB_HOST=host.docker.internal` (mapeado para o gateway do Docker no compose). O Postgres precisa aceitar conexões vindas da rede do Docker:

- `postgresql.conf`: `listen_addresses` incluindo a interface do bridge, por exemplo `'localhost,172.17.0.1'` ou `'*'`.
- `pg_hba.conf`: liberar a sub-rede do Docker, por exemplo `host all all 172.16.0.0/12 scram-sha-256`.
- Reinicie o Postgres depois de alterar.

## Checklist para um novo ambiente

1. PostgreSQL acessível; preencher `DB_*` no `infra/.env`.
2. Par de chaves RSA próprio do ambiente (`openssl genrsa -out jwt-private.pem 2048` e `openssl rsa -in jwt-private.pem -pubout -out jwt-public.pem`), apontado em `JWT_PRIVATE_KEY_FILE`/`JWT_PUBLIC_KEY_FILE`. A pública vai também para a Command e a Query.
3. `CURSOR_SECRET_KEY` nova (`openssl rand -base64 32`).
4. Credenciais do admin em `ADMIN_*`, ou vazias para não semear.
5. `docker compose pull && docker compose up -d` (imagem do GHCR) ou `docker compose up -d --build` (build local). Migrations e seed rodam sozinhos no start.

# Deploy

Arquivos em `infra/`: `Dockerfile`, `Dockerfile.dockerignore`, `docker-compose.yml`, `.env.example`. O `docker-compose.yml` é uma **stack do Docker Swarm**: o Portainer aplica com `docker stack deploy`, puxando a imagem do GHCR (não há build). O banco é gerenciado/externo, e a API é exposta pelo **Traefik** em `https://api.instartups.tech`.

O passo a passo do servidor (DNS na Hostinger, rede `proxy`, Traefik, certificados, Portainer) está em **`INSTARTUPS/infra/README.md`**, fora deste repositório. A imagem é publicada no GHCR pelo GitHub Actions; ver [`ci-cd.md`](./ci-cd.md).

## Onde fica cada configuração

| Onde | O quê |
| --- | --- |
| `appsettings.json` (dentro da imagem) | Log (níveis, sink, `Application`), caminhos das chaves no container (`/run/keys/*.pem`), `Jwt:ExpirationMinutes` (60), `RefreshToken:ExpirationDays` (30), `AppDoc`, `AllowedHosts` |
| `docker-compose.yml` | Mapeia as variáveis do `.env` para as chaves do .NET (`Secao__Chave`) e fixa o que é de infraestrutura: TZ, pool do banco, limites, hardening, política de update e labels do Traefik |
| `.env` / variáveis da stack no Portainer | Só o que muda por ambiente ou é segredo |

O `.env` **não entra no container sozinho**: ele só preenche os `${VAR}` do compose. É o bloco `environment:` que leva os valores para a API. Os `${VAR:?}` fazem o deploy falhar se faltar algo obrigatório.

## Subindo no servidor (Portainer)

Pré-requisitos (ver o guia da infra): rede `proxy` criada, Traefik no ar, DNS de `api.instartups.tech` apontando para o servidor e registry `ghcr.io` cadastrado no Portainer.

1. **Chaves RSA** em `/opt/instartups/keys/`. O container roda como o usuário `app` (uid 1654): `chmod 644 jwt-public.pem` e, na privada, `chown 1654 jwt-private.pem && chmod 400 jwt-private.pem`.
2. **Stack** `instartups-auth` no Portainer, com o `infra/docker-compose.yml` (pelo repositório ou pelo web editor) e as variáveis do `.env.example` preenchidas.
3. **Atualizar** para uma imagem nova: *Update the stack* com **Re-pull image and redeploy**. O `update_config: start-first` sobe o container novo antes de derrubar o antigo; se o novo falhar, o Swarm volta para o anterior.

Na inicialização a API aplica as migrations e roda o seed de roles/admin (ver [`configuracao-e-execucao.md`](./configuracao-e-execucao.md#seed-de-dados-na-inicialização)). Para conferir: `GET https://api.instartups.tech/api/healthcheck`.

## Variáveis (`infra/.env`)

| Variável | Uso |
| --- | --- |
| `IMAGE_TAG` | Tag da imagem do GHCR (`latest`, `develop`, `1.2.3`, `sha-xxxxxxx`...). |
| `ASPNETCORE_ENVIRONMENT` | Ambiente (padrão `Production`; o Swagger só existe em `Development`). |
| `API_DOMAIN` | Subdomínio das APIs (`api.instartups.tech`), o mesmo nas 3. |
| `DB_HOST`, `DB_PORT`, `DB_NAME`, `DB_USER`, `DB_PASSWORD` | Montam `ConnectionStrings:PostgresConnection` (pool fixo em 0–50 no compose). |
| `DB_SSL_MODE` | `SSL Mode` do Npgsql (padrão `Prefer`; use `Require` se o provedor exigir). |
| `JWT_PRIVATE_KEY_FILE` | Caminho **absoluto** no host da chave privada, montada como somente leitura em `/run/keys/jwt-private.pem`. |
| `JWT_PUBLIC_KEY_FILE` | Caminho **absoluto** no host da chave pública, montada em `/run/keys/jwt-public.pem`. É a mesma usada pela Command e pela Query. |
| `JWT_ISSUER`, `JWT_AUDIENCE` | `Jwt:Issuer` / `Jwt:Audience`. Precisam ser iguais aos da Command e da Query. |
| `CURSOR_SECRET_KEY` | `Cursor:SecretKey`, chave AES-256 em base64. Gere com `openssl rand -base64 32`. |
| `ADMIN_EMAIL`, `ADMIN_USERNAME`, `ADMIN_PASSWORD` | `AdminUser:*`. Se algum ficar vazio, o admin não é criado. |

## Traefik e rotas

As 3 APIs ficam em `api.instartups.tech`, separadas pelo caminho, sem StripPrefix, porque as rotas já têm prefixo próprio:

| API | Caminhos |
| --- | --- |
| Auth | `/api/auth`, `/api/user`, `/api/permission`, `/api/healthcheck` |
| Command | `/api/instartups/command` |
| Query | `/api/instartups/query` |

Se a Auth ganhar um controller novo, acrescente o prefixo na label `traefik.http.routers.instartups-auth.rule` do compose.

O serviço não publica porta: só o Traefik o alcança, pela rede `proxy`. `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` faz a API respeitar o `X-Forwarded-Proto/For` do Traefik, o que dá esquema `https` e o IP real do cliente nos logs.

## Formato do compose (Swarm)

O `docker stack deploy` valida o arquivo contra o schema `version: "3.8"`:

- **Sem `name:` no topo.** O Swarm recusa (`Additional property name is not allowed`); era o bug do `name: instartups`. `pull_policy` também é recusado.
- `container_name`, `restart`, `build` e `security_opt` são ignorados no Swarm, por isso não estão no arquivo. O reinício é feito por `deploy.restart_policy`.
- As labels do Traefik ficam em **`deploy.labels`**. Labels fora de `deploy` vão para o container e o Traefik não enxerga.
- Para validar antes do deploy: `docker stack config -c infra/docker-compose.yml` (com as variáveis exportadas).

## Detalhes da imagem

- Build multi-stage com o restore em camada separada e cache do NuGet (BuildKit), então mudanças só no código não refazem o restore. Para buildar localmente: `docker build -f infra/Dockerfile -t ghcr.io/braynerdev/api-instartups-auth:local .` na raiz do repositório.
- Runtime `aspnet:10.0-noble-chiseled-extra`: sem shell nem gerenciador de pacotes, usuário não-root, com ICU e tzdata. Por não ter shell, não há `HEALTHCHECK`.
- O `Dockerfile.dockerignore` deixa de fora `appsettings.Development.json`, `launchSettings.json`, `.env` e `*.pem`: nenhum segredo entra na imagem; tudo vem por variável de ambiente e volume.
- O Wolverine roda em `TypeLoadMode.Static` fora de `Development` e exige o código pré-gerado em `Api.Instartups.Auth/Internal/Generated/`. Sempre que um handler mudar, regenere com `dotnet run --project Api.Instartups.Auth -- codegen write`; sem isso o container falha na inicialização.
- O container roda com filesystem somente leitura (`/tmp` em tmpfs) e sem capabilities. As chaves de Data Protection ficam em memória, o que não afeta a API (autenticação é por JWT e refresh token no banco).
- A connection string usa `Gss Encryption Mode=Disable`: a imagem chiseled não tem `libgssapi_krb5`, e sem isso o Npgsql loga erro ao tentar GSS.

## Banco externo

- `DB_HOST` é o endereço do provedor. Se ele tiver lista de IPs liberados, inclua o IP do servidor.
- SSL: `DB_SSL_MODE=Prefer` usa SSL quando o servidor oferece; `Require` obriga.
- Conexões: cada API abre até 50 (pool do compose), ou seja, até 150 com as 3. Confira o limite do plano do banco e, se preciso, reduza o `Maximum Pool Size` nos composes.

## Checklist para um novo ambiente

1. Infra pronta conforme `INSTARTUPS/infra/README.md` (DNS, rede `proxy`, Traefik, registry no Portainer).
2. Banco acessível a partir do servidor; preencher `DB_*`.
3. Par de chaves RSA próprio do ambiente (`openssl genrsa -out jwt-private.pem 2048` e `openssl rsa -in jwt-private.pem -pubout -out jwt-public.pem`) em `/opt/instartups/keys/`, legível pelo uid 1654. A pública vai também para a Command e a Query.
4. `CURSOR_SECRET_KEY` nova (`openssl rand -base64 32`).
5. Credenciais do admin em `ADMIN_*`, ou vazias para não semear.
6. Deploy da stack no Portainer. Migrations e seed rodam sozinhos no start.

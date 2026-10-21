# Autenticação e autorização

## Visão geral

A API usa **JWT Bearer** para autenticação, com tokens de acesso assinados via **RSA/RS256** e tokens de refresh **opacos** (strings aleatórias, não JWT) armazenados apenas como hash no banco. Autorização é feita por **policies** do ASP.NET Core, cada uma mapeada 1:1 para uma **role do Identity** — na prática, "permissões" são implementadas como roles.

## Fluxo de login (`POST /api/auth/login`)

Implementado em `LoginCommandHandler`:

1. Resolve o usuário por `UserNameOrEmail`: se contém `@`, busca por e-mail (`FindByEmailAsync`); caso contrário, por nome de usuário (`FindByNameAsync`). Se não encontrado, lança `InvalidCredentialsException` (400 — mesma mensagem genérica usada para senha incorreta, para não revelar se o usuário existe).
2. Se o usuário estiver bloqueado (`IsLockedOutAsync`), lança `ForbiddenException` (403) com a mensagem "Usuário bloqueado. Entre em contato com o administrador.".
3. Verifica a senha (`CheckPasswordAsync`). Se incorreta, chama `AccessFailedAsync` (incrementa o contador de tentativas falhas do Identity, que pode levar ao bloqueio) e lança `InvalidCredentialsException` (400).
4. Carrega as sessões ativas do usuário, gera um novo access token (JWT) e um novo refresh token, e persiste uma nova sessão (`ApplicationUser.AddSession`) — ver [Sessões e refresh token](#sessões-e-refresh-token) abaixo para o limite de 3 sessões simultâneas.
5. Retorna `{ AccessToken, RefreshToken }`.

## Formato do access token (JWT)

Gerado por `GenerateJwtService`, assinado com a chave privada RSA (`Jwt:PrivateKeyPath`) usando o algoritmo `RS256`.

Claims incluídas em todo token emitido:

| Claim | Valor |
|---|---|
| `sub` | Id do usuário (`ApplicationUser.Id`) |
| `iat` | Timestamp Unix de emissão |
| `unique_name` | Nome de usuário (`UserName`) |
| `email` | E-mail do usuário |
| `PhoneNumber` (claim customizada) | Telefone do usuário, ou string vazia se não houver |
| `role` (`ClaimTypes.Role`, um por role) | Uma claim de role para cada permissão/role atribuída ao usuário |

Parâmetros de emissão:
- `issuer`: `Jwt:Issuer` (`minha-api` no ambiente configurado)
- `notBefore`: agora (UTC)
- `expires`: agora + `Jwt:ExpirationMinutes` (60 minutos por padrão)

Validação do token recebido (`AuthenticationConf`): `ValidateIssuer = true` (contra `Jwt:Issuer`), **`ValidateAudience = false`**, `ValidateLifetime = true`, `ClockSkew = TimeSpan.Zero`, assinatura validada contra a chave pública RSA. O campo `Jwt:Audience` existe na configuração mas não é usado nem para emitir nem para validar o token — é uma configuração sem efeito hoje.

### Respostas de autenticação/autorização customizadas

`AuthenticationConf` registra `JwtBearerEvents` para que falhas de autenticação/autorização não retornem o corpo padrão vazio do ASP.NET Core, e sim o mesmo envelope `BaseResponseDTO<string>` usado no resto da API:

- **401 (`OnChallenge`)** — token ausente, inválido ou expirado. Mensagem `"O token de acesso expirou."` se a falha for especificamente de expiração (`SecurityTokenExpiredException`), ou `"É necessário estar autenticado para acessar este recurso."` nos demais casos.
- **403 (`OnForbidden`)** — usuário autenticado mas sem a role/policy exigida. Mensagem `"Você não possui permissão para acessar este recurso."`.

Esses dois casos são tratados na camada de autenticação, **antes** de chegar ao `ExceptionsMiddleware` (ver [`tratamento-de-erros.md`](./tratamento-de-erros.md)).

## Sessões e refresh token

Cada sessão é um registro `UserSessionsModel` (tabela `UserSessions`, schema `auth`) contendo:
- `TokenHash`: hash SHA-256 do refresh token — **o token em texto puro nunca é persistido**.
- `ExpiresAt`, `RevokedAt` (nulo enquanto ativa).
- `ReplacedByTokenId`: aponta para a sessão que a substituiu, formando uma cadeia de rotação.

O refresh token em si (`GenerateRefreshTokenService.GenerateRefreshToken()`) é gerado a partir de 32 bytes aleatórios criptograficamente seguros, codificados em hexadecimal (64 caracteres).

### Limite de sessões ativas

`ApplicationUser.MaxActiveSessions = 3`. Ao fazer login, `AddSession` conta as sessões ativas (`RevokedAt is null && ExpiresAt > now`) e lança `MaxActiveSessionsException` (400, "Limite de sessões ativas atingido.") se o limite já tiver sido atingido — ou seja, um quarto login simultâneo é rejeitado enquanto uma das três sessões anteriores não for revogada.

### Renovação (`POST /api/auth/refresh`, anônimo)

`RefreshTokenCommandHandler`:
1. Busca a sessão pelo hash do refresh token enviado. Se não existir, lança `InvalidRefreshTokenException` (401, "Refresh token inválido.").
2. Se o usuário dono da sessão estiver bloqueado, lança `ForbiddenException` (403).
3. Rotaciona a sessão (`ApplicationUser.RotateSession`): a sessão antiga é revogada e marcada com `ReplacedByTokenId` apontando para a nova; a nova sessão é criada com o **mesmo `ExpiresAt`** da sessão original (a renovação não estende a janela total da sessão, apenas troca o token).
4. Emite um novo access token e retorna `{ AccessToken, RefreshToken }` (novo par).

> Este endpoint não exige access token: a credencial é o próprio refresh token enviado no corpo. Assim o cliente consegue renovar a sessão mesmo depois que o access token expirou (dentro da janela de validade do refresh token).

### Logout (`POST /api/auth/logout`, requer `[Authorize]`)

Revoga apenas a sessão associada ao refresh token informado no corpo (`RevokeTokenCommand`). Se a sessão já estiver inativa/inexistente, lança `InvalidRefreshTokenException` (401).

### Logout de todas as sessões (`POST /api/auth/logout/all`, requer `[Authorize]`)

Não recebe corpo — usa o id do usuário extraído do JWT (`User.GetUserId()`) para revogar todas as sessões ativas dele (`ApplicationUser.RevokeAllSessions`).

### Revogação automática de sessões

Além do logout explícito, as sessões de um usuário são revogadas automaticamente (forçando novo login em todos os dispositivos) quando:
- O próprio usuário troca a senha (`PUT /api/user/me/password`).
- Um admin altera a senha de um usuário (`PUT /api/user/{id}` com `NewPassword` preenchido).
- Um admin bloqueia um usuário que antes não estava bloqueado (transição de desbloqueado → bloqueado via `PUT /api/user/{id}`).

## Política de senha

Há **duas camadas** de regras de senha no projeto, com comportamentos diferentes:

1. **ASP.NET Core Identity nativo** (`IdentityConf.cs`) — está deliberadamente **permissivo**: `RequireDigit`, `RequireLowercase`, `RequireUppercase`, `RequireNonAlphanumeric` = `false`, `RequiredLength = 1`, `RequiredUniqueChars = 0`. Ou seja, o próprio Identity aceitaria praticamente qualquer senha.
2. **FluentValidation** (`PasswordExtensions.ValidationPassword()`), aplicada a todo comando que recebe senha (registro, troca de senha, atualização por admin) — é a regra que **de fato** é aplicada antes do handler rodar:
   - Não vazia.
   - Entre 8 e 20 caracteres.
   - Deve conter ao menos: uma letra maiúscula, uma letra minúscula, um dígito e um caractere não alfanumérico (símbolo).

Outras configurações do Identity (`IdentityConf.cs`):
- Lockout: até 5 tentativas de login falhas (`MaxFailedAccessAttempts = 5`), bloqueio temporário de 5 minutos (`DefaultLockoutTimeSpan`), habilitado para novos usuários.
- E-mail único obrigatório (`RequireUniqueEmail = true`).
- `PasswordHasherOptions.IterationCount = 1000` — abaixo dos valores tipicamente recomendados para PBKDF2/Identity (ver [`limitacoes-conhecidas.md`](./limitacoes-conhecidas.md)).

## Permissões e autorização

O catálogo de permissões é uma lista fixa em código, `Constants/PermissionConst.cs`:

```csharp
Users.View, Users.Create, Users.Update, Users.Delete, Sessions.Revoke, Admin
```

`AuthorizationConfig` registra automaticamente **uma policy do ASP.NET Core Authorization para cada entrada dessa lista**, exigindo usuário autenticado e a role correspondente (`RequireAuthenticatedUser()` + `RequireRole(nomeDaPermissao)`). Ou seja, uma "permissão" é, na prática, uma role do Identity com o mesmo nome.

**Somente a policy `Admin` é de fato usada** em algum endpoint hoje (`[Authorize(Policy = PermissionConst.Admin)]` em `UserController` e `PermissionController`). As policies `Users.View`, `Users.Create`, `Users.Update`, `Users.Delete` e `Sessions.Revoke` existem e são registradas, mas nenhum endpoint as exige atualmente — são permissões definidas para uso futuro.

### Concedendo/revogando permissões a um usuário

Um administrador pode conceder ou remover qualquer uma das permissões do catálogo para um usuário via:
- `POST /api/user/{id}/permissions/{permissionName}` — adiciona a role. Retorna 409 (`ConflictException`, "Usuário já possui essa permissão.") se já atribuída.
- `DELETE /api/user/{id}/permissions/{permissionName}` — remove a role. Retorna 404 se o usuário não tiver a permissão.

Em ambos os casos, `permissionName` é validado contra a lista `PermissionConst.All` — um nome fora da lista é rejeitado na validação (código de erro `Format`, mensagem "Permissão inválida.") antes mesmo de chegar ao handler.

Novos usuários registrados via `POST /api/user` **não recebem nenhuma role/permissão automaticamente** — um admin precisa concedê-las manualmente após o cadastro.

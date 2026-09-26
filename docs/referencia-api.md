# Referência da API

Convenções gerais válidas para todos os endpoints abaixo:

- Todas as rotas são **minúsculas** (`RouteOptions.LowercaseUrls = true`).
- Content-Type de requisição e resposta é `application/json` (configurado globalmente em `ControllersConfig`).
- A resposta é sempre encapsulada em `BaseResponseDTO<T>`, sem exceções. Formato completo do envelope e dos erros em [`tratamento-de-erros.md`](./tratamento-de-erros.md).
- Endpoints marcados `[Authorize]` exigem um header `Authorization: Bearer <accessToken>` com um JWT válido e não expirado.
- Endpoints marcados `Policy = Admin` exigem, além de autenticação, que o usuário possua a role `Admin`.

## Health

### `GET /api/healthcheck`
- **Autenticação:** nenhuma.
- **Controller:** `HealthCheckController.Health`.
- **Resposta 200 (`BaseResponseDTO<string>`):**

```json
{
  "isSuccess": true,
  "responseAt": "2026-01-01T00:00:00Z",
  "message": "Ação executada com sucesso.",
  "data": "ok"
}
```

## Auth (`/api/auth`)

### `POST /api/auth/login`
- **Autenticação:** nenhuma.
- **Corpo (`LoginCommand`):**

```json
{
  "userNameOrEmail": "string",
  "password": "string"
}
```
- **Validação:** `userNameOrEmail` e `password` obrigatórios (não vazios).
- **Resposta 200 (`BaseResponseDTO<LoginCommandResponse>`):**

```json
{
  "isSuccess": true,
  "responseAt": "2026-01-01T00:00:00Z",
  "message": "Ação executada com sucesso.",
  "data": { "accessToken": "string", "refreshToken": "string" }
}
```
- **Erros possíveis:** `400` credenciais inválidas ou limite de sessões ativas atingido; `403` usuário bloqueado.

### `POST /api/auth/refresh`
- **Autenticação:** nenhuma — o refresh token no corpo é a credencial; funciona mesmo com o access token expirado.
- **Corpo (`RefreshTokenCommand`):** `{ "refreshToken": "string" }` (obrigatório).
- **Resposta 200:** `BaseResponseDTO<RefreshTokenCommandResponse>` com `{ accessToken, refreshToken }` — **novo** par, a sessão anterior é revogada e substituída.
- **Erros possíveis:** `401` refresh token inválido/inexistente; `403` usuário bloqueado.

### `POST /api/auth/logout`
- **Autenticação:** `[Authorize]`.
- **Corpo (`RevokeTokenCommand`):** `{ "refreshToken": "string" }` (obrigatório).
- **Resposta 200:** `BaseResponseDTO<string>`, `data: null`, mensagem "Logout realizado com sucesso.".
- **Erros possíveis:** `401` refresh token inválido/já revogado.

### `POST /api/auth/logout/all`
- **Autenticação:** `[Authorize]`.
- **Corpo:** nenhum — o id do usuário é extraído do JWT (`User.GetUserId()`).
- **Resposta 200:** `BaseResponseDTO<string>`, `data: null`, mensagem "Sessões encerradas com sucesso.".
- **Efeito:** revoga todas as sessões ativas do usuário autenticado.

## User (`/api/user`)

### `POST /api/user` — cadastro público
- **Autenticação:** nenhuma (endpoint de cadastro).
- **Corpo (`RegisterUserCommand`):**

```json
{
  "userName": "string",
  "email": "string",
  "password": "string",
  "phoneNumber": "string | null"
}
```
- **Validação:** `userName` obrigatório (≤ 20 caracteres); `email` obrigatório, formato válido, ≤ 100 caracteres; `phoneNumber` opcional, ≤ 20 caracteres quando informado; `password` obrigatória, 8–20 caracteres, deve conter maiúscula + minúscula + dígito + símbolo.
- **Resposta 200 (`BaseResponseDTO<RegisterUserCommandResponse>`):**

```json
{
  "isSuccess": true,
  "responseAt": "2026-01-01T00:00:00Z",
  "message": "Ação executada com sucesso.",
  "data": { "userName": "string", "email": "string", "phoneNumber": "string | null" }
}
```
- **Efeito:** cria o usuário via `UserManager.CreateAsync`; **não atribui nenhuma role/permissão** — um admin precisa concedê-las depois via os endpoints de permissão.
- **Erros possíveis:** `409` (`IdentityValidationException`) em caso de nome de usuário/e-mail duplicado ou violação das regras do Identity.

### `GET /api/user` — listar usuários
- **Autenticação:** `Policy = Admin`.
- **Query params:** `cursor` (string, opcional — cursor opaco retornado por uma chamada anterior), `pageSize` (int; se `<= 0`, assume `20`).
- **Validação:** `pageSize` deve estar entre 1 e 100 (a normalização de `<=0 → 20` acontece no controller antes da validação, então só valores explicitamente fora do intervalo 1–100 e positivos são rejeitados).
- **Resposta 200 (`BaseResponseDTO<ListUsersQueryResponse>`):**

```json
{
  "items": [
    { "id": "string", "userName": "string", "email": "string", "phoneNumber": "string | null" }
  ],
  "nextCursor": "string | null",
  "hasMore": true
}
```
- **Paginação:** por cursor opaco criptografado (AES-GCM), ordenado por `Id`. Ver detalhes em [`arquitetura.md`](./arquitetura.md) e no serviço `CursorEncoder`.
- **Erros possíveis:** `400` cursor inválido/corrompido.

### `GET /api/user/{id}` — obter usuário por id
- **Autenticação:** `Policy = Admin`.
- **Resposta 200 (`BaseResponseDTO<GetUserByIdQueryResponse>`):**

```json
{
  "id": "string", "userName": "string", "email": "string", "phoneNumber": "string | null",
  "roles": ["string"], "isLocked": true
}
```
- **Erros possíveis:** `404` usuário não encontrado.

### `GET /api/user/me` — perfil do usuário autenticado
- **Autenticação:** `[Authorize]`.
- **Resposta 200 (`BaseResponseDTO<GetMeQueryResponse>`):**

```json
{ "id": "string", "userName": "string", "email": "string", "phoneNumber": "string | null" }
```
- **Observação de robustez:** o handler (`GetMeQueryHandler`) não verifica se o usuário retornado por `FindByIdAsync` é nulo antes de acessar seus campos. Se o token ainda for válido mas o usuário associado tiver sido excluído do banco, a requisição resulta em erro `500` em vez de `404`/`401`. Ver [`limitacoes-conhecidas.md`](./limitacoes-conhecidas.md).

### `PUT /api/user/{id}` — atualização administrativa
- **Autenticação:** `Policy = Admin`.
- **Corpo (`AdminUpdateUserRequest`):**

```json
{
  "userName": "string",
  "email": "string",
  "phoneNumber": "string | null",
  "newPassword": "string | null",
  "isLocked": true
}
```
- **Validação:** `userName`/`email`/`phoneNumber` seguem as mesmas regras do cadastro; `newPassword`, quando informado, segue a regra completa de senha (8–20 caracteres, maiúscula+minúscula+dígito+símbolo) — é opcional (só validado se não vazio).
- **Regras de negócio:**
  - Um admin **não pode bloquear a própria conta** (`command.IsLocked && AdminUserId == UserId` → `403`, "Não é possível bloquear seu próprio usuário.").
  - Se `newPassword` for informado, a senha é redefinida via token de reset interno gerado e consumido na hora — **sem exigir a senha atual**.
  - Bloqueio/desbloqueio: `isLocked = true` define `LockoutEnd = DateTimeOffset.MaxValue` (bloqueio permanente); `false` remove o bloqueio (`LockoutEnd = null`).
  - Se a senha foi alterada, ou se o usuário passou de desbloqueado para bloqueado, **todas as sessões ativas do usuário são revogadas**.
- **Resposta 200 (`BaseResponseDTO<AdminUpdateUserCommandResponse>`):**

```json
{ "id": "string", "userName": "string", "email": "string", "phoneNumber": "string | null", "isLocked": true }
```
- **Erros possíveis:** `403` autobloqueio; `404` usuário não encontrado; `409` violação de regras do Identity (ex.: e-mail duplicado).

### `PUT /api/user/me` — atualização do próprio perfil
- **Autenticação:** `[Authorize]`.
- **Corpo (`UpdateMeRequest`):** `{ "userName": "string", "email": "string", "phoneNumber": "string | null" }`, mesmas regras de validação do cadastro (sem campo de senha).
- **Resposta 200 (`BaseResponseDTO<UpdateMeCommandResponse>`):** `{ id, userName, email, phoneNumber }`.
- **Erros possíveis:** `401` se o usuário do token não existir mais; `409` violação de regras do Identity.

### `PUT /api/user/me/password` — trocar a própria senha
- **Autenticação:** `[Authorize]`.
- **Corpo (`ChangePasswordRequest`):** `{ "currentPassword": "string", "newPassword": "string" }`. `currentPassword` obrigatória; `newPassword` segue a regra completa de senha.
- **Efeito:** troca a senha via `UserManager.ChangePasswordAsync` e **revoga todas as sessões ativas** do usuário (força novo login em todos os dispositivos).
- **Resposta 200:** `BaseResponseDTO<string>`, `data: null`, mensagem "Senha alterada com sucesso.".
- **Erros possíveis:** `400` senha atual incorreta (`InvalidCredentialsException`); `401` se o usuário do token não existir mais.

### `POST /api/user/{id}/permissions/{permissionName}` — conceder permissão
- **Autenticação:** `Policy = Admin`.
- **Parâmetros de rota:** `id` (id do usuário-alvo), `permissionName` (deve corresponder exatamente a um nome do catálogo em `PermissionConst`, ver [`autenticacao-e-autorizacao.md`](./autenticacao-e-autorizacao.md)).
- **Resposta 200:** `BaseResponseDTO<string>`, mensagem "Permissão adicionada com sucesso.".
- **Erros possíveis:** `400` nome de permissão fora do catálogo; `404` usuário não encontrado; `409` usuário já possui a permissão.

### `DELETE /api/user/{id}/permissions/{permissionName}` — revogar permissão
- **Autenticação:** `Policy = Admin`.
- **Resposta 200:** `BaseResponseDTO<string>`, mensagem "Permissão removida com sucesso.".
- **Erros possíveis:** `400` nome de permissão fora do catálogo; `404` usuário não encontrado, ou usuário não possui a permissão informada.

## Permission (`/api/permission`)

### `GET /api/permission`
- **Autenticação:** `Policy = Admin`.
- **Resposta 200 (`BaseResponseDTO<ListPermissionsQueryResponse>`):**

```json
{ "permissions": ["Users.View", "Users.Create", "Users.Update", "Users.Delete", "Sessions.Revoke", "Admin"] }
```
- **Observação:** a lista é **estática**, retornada diretamente de `PermissionConst.All` — não reflete nenhuma consulta ao banco, apenas o catálogo fixo definido em código.

## Exemplos de uso (arquivo `.http`)

O repositório inclui `Api.Instartups.Auth/Api.Instartups.Auth.http` com exemplos manuais de requisição para um cliente REST (VS Code/Rider/Visual Studio). Ele encadeia cadastro → login → refresh → logout → logout/all reaproveitando o token da resposta do login.

> ⚠️ Esse arquivo `.http` contém um exemplo rotulado "Dados do usuário autenticado" apontando para `GET /api/auth/me` — **essa rota não existe**; o endpoint real e correto para o perfil do usuário autenticado é `GET /api/user/me` (documentado acima, em `UserController`). Use `/api/user/me`.

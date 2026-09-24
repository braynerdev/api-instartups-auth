# Tratamento de erros

## Envelope de resposta padrão

Toda resposta da API (sucesso ou erro), sem exceções, segue o formato definido em `src/DTOs/BaseResponseDTO.cs` (ver [`referencia-api.md`](./referencia-api.md) para o corpo de cada endpoint):

```json
{
  "isSuccess": true,
  "responseAt": "2026-01-01T12:00:00Z",
  "message": "Ação executada com sucesso.",
  "data": { }
}
```

| Campo | Tipo | Descrição |
|---|---|---|
| `isSuccess` | `bool` | `true` em respostas de sucesso, `false` em erros |
| `responseAt` | `DateTimeOffset` (UTC) | Momento em que a resposta foi construída |
| `message` | `string` | Mensagem legível — "Ação executada com sucesso." por padrão em sucesso, "Erro ao executar a ação." por padrão em erro (mensagens específicas sobrescrevem esses padrões) |
| `data` | `T?` | Corpo específico do endpoint, ou `null` em vários casos de erro |

## Middleware global (`ExceptionsMiddleware`)

Registrado como o primeiro middleware do pipeline (`app.UseExceptionsMiddleware()`), envolve toda a requisição em um `try/catch` e mapeia o tipo da exceção para um status HTTP:

| Tipo de exceção | Status HTTP |
|---|---|
| `FluentValidation.ValidationException` | 400 |
| `BadRequestException` e subclasses (`InvalidCredentialsException`, `InvalidCursorException`, `MaxActiveSessionsException`) | 400 |
| `UnauthorizedException` e subclasse `InvalidRefreshTokenException` | 401 |
| `ForbiddenException` | 403 |
| `NotFoundException` (`Base/NotFoundException`) | 404 |
| `ConflictException` e subclasse `IdentityValidationException` | 409 |
| `OperationCanceledException` | 499 (Client Closed Request) |
| Qualquer outra exceção | 500 |

### Corpo de erro por tipo

- **`FluentValidation.ValidationException`** — erros de validação agrupados por campo:

```json
{
  "isSuccess": false,
  "message": "Erro de validação.",
  "data": [
    { "field": "email", "code": "Format", "message": ["O e-mail informado não é válido."] }
  ]
}
```

- **`IdentityValidationException`** — mesmo formato de item (`ValidateErrorDTO`), gerado a partir de um `IdentityResult` malsucedido (ex.: `UserManager.CreateAsync`, `UpdateAsync`, `ResetPasswordAsync`). O campo (`field`) é inferido a partir do código de erro do Identity, em `IdentityResultExtensions.GetField`:
  - `DuplicateUserName`, `InvalidUserName` → `"username"`
  - `DuplicateEmail`, `InvalidEmail` → `"email"`
  - `PasswordTooShort`, `PasswordRequiresDigit`, `PasswordRequiresUpper`, `PasswordRequiresLower`, `PasswordRequiresNonAlphanumeric`, `PasswordRequiresUniqueChars` → `"password"`
  - Qualquer outro código → `""` (campo vazio)

- **`UnauthorizedException` / `BadRequestException` / `NotFoundException`** — `data: null`, `message` é a mensagem própria da exceção (em português, específica do cenário — ver tabela abaixo).

- **`OperationCanceledException`** — `data: null`, mensagem fixa "Requisição cancelada pelo cliente" (cliente fechou a conexão antes da resposta).

- **Qualquer outra exceção (500)** — `data: null`, mensagem fixa "Erro inesperado". O detalhe real da exceção **nunca** é exposto ao cliente; é logado internamente via `ILogger.LogError`.

### Mensagens das exceções de negócio

| Exceção | Status | Mensagem padrão |
|---|---|---|
| `InvalidCredentialsException` | 400 | "Credenciais inválidas." |
| `MaxActiveSessionsException` | 400 | "Limite de sessões ativas atingido." |
| `InvalidCursorException` | 400 | "Cursor de paginação inválido." |
| `UnauthorizedException` (sem mensagem customizada) | 401 | "Token inválido." |
| `InvalidRefreshTokenException` | 401 | "Refresh token inválido." |
| `ForbiddenException` (sem mensagem customizada) | 403 | "Acesso negado." |
| `ForbiddenException` (login com usuário bloqueado) | 403 | "Usuário bloqueado. Entre em contato com o administrador." |
| `ForbiddenException` (admin tentando se autobloquear) | 403 | "Não é possível bloquear seu próprio usuário." |
| `NotFoundException` (usuário) | 404 | "Usuário não encontrado." |
| `NotFoundException` (permissão) | 404 | "Usuário não possui essa permissão." |
| `ConflictException` (permissão duplicada) | 409 | "Usuário já possui essa permissão." |

### Logging

- Exceções que resultam em `500` são logadas com `LogError` (stack trace completo incluído).
- Todas as demais exceções tratadas (400/401/403/404/409/499) são logadas com `LogWarning("Erro de negócio: {mensagem}")` — nível de log configurado explicitamente para `Warning` no namespace `Api.Instartups.Auth.Middleware` via `Serilog:MinimumLevel:Override`, já que o nível padrão da aplicação é `Fatal`.

## Erros 401/403 na camada de autenticação (fora do middleware)

Falhas de autenticação/autorização do JWT Bearer (token ausente, expirado, inválido, ou usuário sem a role exigida) são tratadas **diretamente pelos eventos do `JwtBearerHandler`** (`AuthenticationConf.cs`, `OnChallenge`/`OnForbidden`), **antes** de a requisição chegar ao MVC/`ExceptionsMiddleware`. O formato de resposta é o mesmo (`BaseResponseDTO<string>`), mas o caminho de código é diferente — ver [`autenticacao-e-autorizacao.md`](./autenticacao-e-autorizacao.md) para detalhes e mensagens.

## Hierarquia de exceções (`src/Exceptions/`)

```
Exception
├── Base/BadRequestException
│   ├── InvalidCredentialsException
│   ├── InvalidCursorException
│   └── MaxActiveSessionsException
├── UnauthorizedException
│   └── InvalidRefreshTokenException
├── ForbiddenException
├── Base/NotFoundException
└── Base/ConflictException
    └── IdentityValidationException
```

O choque comum para transformar um `IdentityResult` malsucedido em `IdentityValidationException` é a extensão `IdentityResultExtensions.EnsureSucceeded()` (`src/Common/Identity/IdentityResultExtensions.cs`), chamada após toda operação de `UserManager` que pode falhar (`CreateAsync`, `UpdateAsync`, `ResetPasswordAsync`, `AddToRoleAsync`, `RemoveFromRoleAsync`, `SetLockoutEndDateAsync`).

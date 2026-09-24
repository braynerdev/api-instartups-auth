# Modelo de dados

## Banco de dados

- **Engine:** PostgreSQL, via provider `Npgsql.EntityFrameworkCore.PostgreSQL`.
- **Schema:** todas as tabelas da aplicação (incluindo as do ASP.NET Identity) vivem no schema dedicado **`auth`** — configurado em `AppDbContext.OnModelCreating` (`modelBuilder.HasDefaultSchema("auth")`).
- **Tabela de histórico de migrations:** `__EFMigrationsHistory`, também no schema `auth` (configurado em `DatabaseConf.AddPostgresConf` via `bd.MigrationsHistoryTable("__EFMigrationsHistory", "auth")`).
- **DbContext:** `AppDbContext : IdentityDbContext<ApplicationUser>` (`src/Configurations/Database/AppDbContext.cs`), com um único `DbSet` próprio: `UserSessions`.
- **Migrations:** existe uma única migration no projeto, `Migrations/20260907222626_FirstMigrate.cs`, responsável por criar todas as tabelas descritas abaixo.
- **Convenção de mapeamento:** qualquer `IEntityTypeConfiguration<T>` que também implemente a interface marcadora `IAppConfiguration` é aplicada automaticamente (`modelBuilder.ApplyConfigurationsFromAssembly(...)`). Hoje só `UserSessionsModelMap` segue essa convenção.

## Tabelas do ASP.NET Core Identity

Criadas automaticamente por herdar de `IdentityDbContext<ApplicationUser>` (todas no schema `auth`):

| Tabela | Papel |
|---|---|
| `AspNetUsers` | Usuários (`ApplicationUser`) |
| `AspNetRoles` | Roles — usadas também para representar "permissões" (ver [`autenticacao-e-autorizacao.md`](./autenticacao-e-autorizacao.md)) |
| `AspNetUserRoles` | Tabela de junção usuário ↔ role |
| `AspNetUserClaims` | Claims associadas diretamente a um usuário |
| `AspNetRoleClaims` | Claims associadas a uma role |
| `AspNetUserLogins` | Logins externos (infraestrutura padrão do Identity; não há provedor externo configurado no projeto) |
| `AspNetUserTokens` | Tokens internos do Identity (ex.: tokens de reset de senha gerados via `GeneratePasswordResetTokenAsync`) |

## `ApplicationUser` (`src/Models/ApplicationUser.cs`)

Estende `Microsoft.AspNetCore.Identity.IdentityUser`, herdando campos padrão como `Id` (string/GUID), `UserName`, `NormalizedUserName`, `Email`, `NormalizedEmail`, `PasswordHash`, `PhoneNumber`, `LockoutEnd`, `LockoutEnabled`, `AccessFailedCount`, `SecurityStamp`, `ConcurrencyStamp`.

Além disso, adiciona:
- `public const int MaxActiveSessions = 3` — limite de sessões (refresh tokens) ativas simultâneas por usuário.
- `UserSessionsModel UserSessionsModel { get; }` — coleção somente leitura das sessões do usuário (backing field privado `_userSessionsModel`).
- Métodos de domínio que encapsulam as regras de sessão: `AddSession(tokenHash, expiresAt)`, `RotateSession(sessãoAtual, novoTokenHash, expiresAt)`, `RevokeSession(sessão)`, `RevokeAllSessions()` — detalhados em [`autenticacao-e-autorizacao.md`](./autenticacao-e-autorizacao.md).

## `UserSessionsModel` (`src/Models/UserSessionsModel.cs`)

Representa uma sessão de refresh token. Todas as propriedades são somente leitura fora da própria classe; instâncias só são criadas via o factory estático `Create(tokenHash, expiresAt, userId)`.

| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | `string` | GUID v7 (`Guid.CreateVersion7()`), gerado na criação |
| `TokenHash` | `string` | Hash SHA-256 (hex) do refresh token — o token em texto puro nunca é persistido |
| `ExpiresAt` | `DateTimeOffset` | Data/hora de expiração da sessão |
| `RevokedAt` | `DateTimeOffset?` | Nulo enquanto ativa; preenchido ao revogar |
| `ReplacedByTokenId` | `string?` | Id da sessão que substituiu esta (rotação) |
| `UserId` | `string` | FK para `AspNetUsers` |
| `User` | `ApplicationUser` | Propriedade de navegação |

Comportamento: `IsActive(now) => RevokedAt is null && ExpiresAt > now`; `Revoke(revokedAt, replacedByTokenId?)`.

### Mapeamento EF Core (`UserSessionsModelMap`)

Tabela `UserSessions` (schema `auth`), com as seguintes colunas e restrições:

| Propriedade | Coluna (snake_case) | Tipo de coluna |
|---|---|---|
| `Id` | `Id` (chave primária) | padrão (string) |
| `ExpiresAt` | `expires_at` | `timestamptz` |
| `TokenHash` | `token_hash` | `varchar(255)` |
| `RevokedAt` | `revoked_at` | `timestamptz` |
| `ReplacedByTokenId` | `replaced_by_token_id` | `varchar(450)` |
| `UserId` | `user_id` | `varchar(450)` |

Relacionamentos e índices:
- `ReplacedByTokenId` é uma FK auto-referenciada 1:1 para outra linha de `UserSessions`, com `DeleteBehavior.Restrict` e índice único.
- `UserId` é FK para `ApplicationUser` (`AspNetUsers`), com `DeleteBehavior.Cascade` — ao excluir um usuário, todas as suas sessões são excluídas em cascata.
- Índice único composto em `(TokenHash, UserId)`.

## Diagrama simplificado

```
AspNetUsers (ApplicationUser)  1 ──── * UserSessions (UserSessionsModel)
        │                                    │
        │ *                                  │ 0..1 (auto-referência)
        ▼                                    ▼
AspNetUserRoles ──── * AspNetRoles     ReplacedByTokenId → UserSessions.Id
```

## Escopo do módulo de domínio `Instartups` (fora deste serviço)

Vale registrar, para não gerar confusão: existe um projeto irmão (`Instartups/`, repositório git separado) com entidades de domínio para "perfis" de startups/investidores (`PerfilEntity`, `ReferenciaUsuarioEntity`, enum `TiposPerfisEnum`). Esse projeto **não possui banco de dados, `DbContext`, migrations ou API exposta** — é modelagem de domínio isolada, sem integração com este serviço de autenticação. Esta documentação cobre exclusivamente `api-instartups-auth`.

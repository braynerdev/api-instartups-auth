# Arquitetura

## Stack tecnológica

O projeto é um único projeto ASP.NET Core Web API (`Api.Instartups.Auth.csproj`), alvo `net10.0`, com `Nullable` e `ImplicitUsings` habilitados.

Dependências declaradas em `Api.Instartups.Auth.csproj`:

| Pacote | Versão | Papel |
|---|---|---|
| `Microsoft.AspNetCore.OpenApi` | 10.0.11 | Geração nativa de documento OpenAPI (`/openapi/v1.json` em desenvolvimento) |
| `Scalar.AspNetCore` | 2.18.3 | Documentação interativa em `/docs` (em desenvolvimento), igual à Command e à Query |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.11 | Autenticação via JWT Bearer |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 10.0.11 | ASP.NET Core Identity (usuários, roles) persistido via EF Core |
| `Microsoft.EntityFrameworkCore` / `.Design` | 10.0.11 | ORM e ferramentas de migration |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.0.3 | Provider do EF Core para PostgreSQL |
| `System.IdentityModel.Tokens.Jwt` | 8.22.0 | Criação/validação de tokens JWT (usado junto com o pacote JwtBearer) |
| `WolverineFx` | 6.33.0 | Mediator/message bus interno para o padrão CQRS (`IMessageBus`) |
| `WolverineFx.FluentValidation` | 6.33.0 | Executa validadores FluentValidation automaticamente antes de cada handler |
| `WolverineFx.RuntimeCompilation` | 6.33.0 | Compilação em tempo de execução usada pelo Wolverine |
| `Mapster.DependencyInjection` | 10.0.12 | Mapeamento de objetos (entidade → DTO de resposta) |
| `Serilog.AspNetCore` | 10.0.0 | Logging estruturado |
| `Microsoft.VisualStudio.Web.CodeGeneration.Design` | 10.0.2 | Ferramentas de scaffolding (dependência de desenvolvimento) |
| `NuGet.Protocol` | 7.9.0 | Protocolo NuGet (dependência transitiva de ferramentas) |

Não há framework de testes (`xunit`/`nunit`/`MSTest`) referenciado em nenhum lugar do `.csproj` — ver [`limitacoes-conhecidas.md`](./limitacoes-conhecidas.md).

## Padrão arquitetural: CQRS orientado a casos de uso

O projeto não segue uma divisão clássica em camadas por múltiplos `.csproj` (Controller/Service/Repository espalhados). Em vez disso, é um **monólito modular organizado por caso de uso**, com um mediator (Wolverine) desacoplando o controller da lógica de negócio:

1. O **Controller** recebe a requisição HTTP, monta um `Command` ou `Query` (records imutáveis) e o envia via `IMessageBus.InvokeAsync<TResponse>(mensagem, ct)`.
2. O **Wolverine** localiza o `Handler` correspondente à mensagem por convenção.
3. Antes do handler rodar, o Wolverine executa automaticamente o **validador FluentValidation** associado à mensagem (via `WolverineFx.FluentValidation`, configurado com `opt.UseFluentValidation()`). Se a validação falhar, uma `FluentValidation.ValidationException` é lançada e tratada pelo middleware global de exceções.
4. O **Handler** (`ICommandHandler<TCommand, TResponse>`, `IVoidCommandHandler<TCommand>` ou `IQueryHandler<TQuery, TResponse>`) executa a regra de negócio, normalmente usando `UserManager<ApplicationUser>` (ASP.NET Identity), repositórios de sessão, ou serviços de geração de token.
5. O Controller devolve o resultado do handler encapsulado em `BaseResponseDTO<T>` (ver [`tratamento-de-erros.md`](./tratamento-de-erros.md)).

Cada caso de uso vive em sua própria pasta sob `src/UseCases/<Módulo>/<NomeDoCaso>/`, por exemplo:

```
src/UseCases/User/RegisterUserCommand/
├── RegisterUserCommand.cs           # record de entrada (implementa ICommand)
├── RegisterUserCommandResponse.cs   # record de saída
├── RegisterUserCommandValidation.cs # regras FluentValidation
└── RegisterUserCommandHandler.cs    # lógica de negócio
```

Essa convenção se repete para todos os casos de uso em `src/UseCases/Auth/*`, `src/UseCases/User/*` e `src/UseCases/Permission/*`.

## Organização de pastas (`src/`)

| Pasta | Conteúdo |
|---|---|
| `Controller/` | `AuthController`, `UserController`, `PermissionController`, `HealthCheckController` — todos finos, delegando para o `IMessageBus` |
| `UseCases/` | Commands/queries + handlers + validações, organizados por módulo (`Auth`, `User`, `Permission`) |
| `Models/` | `ApplicationUser` (extensão de `IdentityUser`) e `UserSessionsModel` (sessão de refresh token) |
| `ModelsMaps/` | Configuração Fluent do EF Core para entidades que não são do Identity (`UserSessionsModelMap`) |
| `Configurations/` | Classes estáticas de bootstrap/DI: autenticação, autorização, CORS, rate limit, banco de dados, Identity, OpenAPI/Scalar, Serilog, mapeamento, Wolverine |
| `Services/` | `GenerateJwtService`, `GenerateRefreshTokenService` — geração de tokens |
| `Repositories/` | `UserSessionRepository` — acesso a dados de sessões |
| `Interfaces/` | Contratos (`ICommand`, `IQuery`, `ICommandHandler`, `IQueryHandler`, `IGenerateJwtService`, `IUserSessionRepository`, `ICursorEncoder`, etc.) |
| `Middleware/` | `ExceptionsMiddleware` — tratamento global de exceções |
| `Exceptions/` | Hierarquia de exceções de negócio (`Base/BadRequestException`, `Base/ConflictException`, `Base/NotFoundException` e subclasses específicas) |
| `DTOs/` | `BaseResponseDTO<T>` (envelope padrão de resposta) e `ValidateErrorDTO` (erro de validação por campo) |
| `Constants/` | `PermissionConst` (catálogo fixo de permissões/roles), `CorsConst`, `RateLimitConst`, `CodeError`, `MessageError` |
| `Common/Validation/` | Extensões reutilizáveis de regras FluentValidation (`ValidationEmail`, `ValidationPassword`, `ValidationPhoneNumber`, `ValidationUserName`) |
| `Common/Pagination/` | `CursorEncoder`, `CursorPage`, `CursorPayload` — paginação por cursor opaco criptografado |
| `Common/Identity/` | `IdentityResultExtensions.EnsureSucceeded()` — traduz falhas do `IdentityResult` em `IdentityValidationException` |
| `Extension/` | `ClaimsPrincipalExtensions` (extrai o id do usuário do JWT), `ExceptionsExtension`, `AuthenticationMiddlewareExtensions` |
| `Options/` | Classes de configuração fortemente tipadas (`JwtOptions`, `RefreshTokenOptions`, `CursorOptions`) |

## Bootstrap da aplicação (`Program.cs`)

Ordem de configuração e do pipeline HTTP, conforme `Program.cs`:

```csharp
builder.Services.AddDependencyInjection(builder.Configuration);
builder.AddWolverineConf();

builder.Services
    .AddAuthenticationConf(builder.Configuration)
    .AddIdentityConf()
    .AddAuthorizationConfig()
    .AddControllersConfig()
    .AddLowerCaseConfig()
    .AddMappingConfig()
    .AddCorsConfigurations(builder.Configuration)
    .AddRateLimiterConfig()
    .AddOpenApiConfig();

builder.AddSerilogConfig();

var app = builder.Build();

var executandoComando = args.Length > 0 && !args[0].StartsWith('-');
if (!executandoComando)          // comandos CLI (ex.: codegen write) não tocam no banco
{
    await app.ApplyMigrationsAsync();
    await app.Services.SeedIdentityDataAsync(builder.Configuration); // seed de roles + admin
}

app.UseCors(CorsConst.Frontend); // 1. CORS antes de tudo (responde preflight)
app.UseSerilogConfig();          // 2. log por requisição
app.UseExceptionsMiddleware();   // 3. captura exceções do resto da pipeline

if (app.Environment.IsDevelopment())
    app.UseOpenApiConfig();      // OpenAPI + Scalar em /docs

else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAuthenticationConf();     // 4. autenticação JWT
app.UseRateLimiter();            // 5. rate limit (depois da autenticação, para particionar pelo sub)
app.UseAuthorization();          // 6. autorização (policies)

app.MapControllers();
return await app.RunJasperFxCommands(args);
```

Pontos relevantes:
- `LowerCasesConfig.AddLowerCaseConfig()` força todas as rotas geradas para minúsculas (`RouteOptions.LowercaseUrls = true`), por isso todos os endpoints documentados aqui usam caminhos em minúsculo (ex.: `api/user/me`).
- `ControllersConfig.AddControllersConfig()` define globalmente `Produces`/`Consumes` como `application/json`, `ReturnHttpNotAcceptable = true` e `AllowEmptyInputInBodyModelBinding = false` (corpo vazio é rejeitado quando o endpoint espera um body). Também documenta 401 e 429 em todos os endpoints, desliga o `[Required]` implícito (`SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true`, a obrigatoriedade fica com o FluentValidation) e troca a resposta de JSON malformado pelo envelope `BaseResponseDTO` (ver [`tratamento-de-erros.md`](./tratamento-de-erros.md)).
- A pipeline segue a mesma ordem da Command e da Query.
- O Scalar (`/docs`) e o `/openapi/v1.json` só são montados em ambiente de desenvolvimento (`app.Environment.IsDevelopment()`). Fora dele entram HSTS e redirecionamento HTTPS; atrás do Traefik o `X-Forwarded-Proto` já chega como `https`, então não há redirecionamento de fato.
- `SeedIdentityDataAsync` roda uma única vez na inicialização, antes do pipeline HTTP ser configurado, e só quando a aplicação sobe como servidor (ver [`configuracao-e-execucao.md`](./configuracao-e-execucao.md)).

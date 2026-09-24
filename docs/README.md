# API de Autenticação InStartups

> Descrição oficial da API (definida em `appsettings.json`, chave `AppDoc:DescricaoApi`):
> "API responsável pelo gerenciamento de autenticação e acesso aos serviços do sistema InStartups."

Esta é a documentação técnica do serviço **`Api.Instartups.Auth`**, uma API ASP.NET Core responsável por cadastro de usuários, login, emissão e renovação de tokens JWT, controle de sessões (refresh tokens) e um sistema simples de permissões baseado em roles do ASP.NET Core Identity.

Toda a documentação aqui presente foi escrita a partir da leitura direta do código-fonte em `Api.Instartups.Auth/`. Onde o comportamento observado tem alguma limitação, inconsistência ou pendência, isso é declarado explicitamente em [`limitacoes-conhecidas.md`](./limitacoes-conhecidas.md) em vez de omitido.

## Sumário

| Documento | Conteúdo |
|---|---|
| [`arquitetura.md`](./arquitetura.md) | Stack tecnológica, organização de pastas, padrões arquiteturais (CQRS com Wolverine, camadas, etc.) |
| [`configuracao-e-execucao.md`](./configuracao-e-execucao.md) | Como configurar e rodar o projeto localmente e via Docker, variáveis de configuração |
| [`autenticacao-e-autorizacao.md`](./autenticacao-e-autorizacao.md) | Fluxo de login/refresh/logout, formato do JWT, sessões, política de senha, permissões |
| [`modelo-de-dados.md`](./modelo-de-dados.md) | Schema do banco PostgreSQL, entidades e mapeamentos EF Core |
| [`referencia-api.md`](./referencia-api.md) | Referência completa de endpoints (rota, autenticação, request/response) |
| [`tratamento-de-erros.md`](./tratamento-de-erros.md) | Formato padrão de resposta e de erro, mapeamento de exceções para status HTTP |
| [`deploy.md`](./deploy.md) | Dockerfile, `compose.yaml`, portas e estado da esteira de CI/CD |
| [`limitacoes-conhecidas.md`](./limitacoes-conhecidas.md) | Pontos de atenção e débitos técnicos observados no código atual |

## Visão geral rápida

- **Stack:** ASP.NET Core Web API (.NET 10), Entity Framework Core 10 + Npgsql (PostgreSQL), ASP.NET Core Identity, JWT Bearer assinado com RSA (RS256), [WolverineFx](https://wolverinefx.net/) como mediator CQRS, FluentValidation, Mapster, Serilog e Swashbuckle (Swagger).
- **Porta padrão (dev):** `http://localhost:8080` (ver [`configuracao-e-execucao.md`](./configuracao-e-execucao.md)).
- **Banco de dados:** PostgreSQL, schema dedicado `auth`.
- **Recursos expostos:** cadastro/login/logout de usuários, refresh de sessão, administração de usuários (listagem, edição, bloqueio) e um catálogo fixo de permissões/roles.
- **Estado do projeto:** não há testes automatizados nem pipeline de CI configurado neste snapshot do código — ver [`limitacoes-conhecidas.md`](./limitacoes-conhecidas.md).

## Onde ficam as coisas

Todo o código do serviço vive em `Api.Instartups.Auth/`, dentro deste mesmo repositório (`api-instartups-auth`):

```
Api.Instartups.Auth/
├── Program.cs                 # bootstrap da aplicação
├── appsettings*.json          # configuração
├── Keys/                      # par de chaves RSA usado para assinar/validar o JWT
├── Migrations/                # migrations do EF Core
└── src/
    ├── Controller/             # controllers HTTP (finos, delegam para UseCases via Wolverine)
    ├── UseCases/                # commands/queries + handlers + validações (regra de negócio)
    ├── Models/                  # entidades (ApplicationUser, UserSessionsModel)
    ├── Configurations/          # wiring de DI, autenticação, banco, swagger etc.
    ├── Services/, Repositories/ # serviços de infraestrutura (JWT, refresh token, sessões)
    ├── Exceptions/, Middleware/ # exceções de domínio e tratamento global de erros
    └── DTOs/, Constants/        # contratos de resposta e constantes (permissões, mensagens)
```

Mais detalhes de cada pasta em [`arquitetura.md`](./arquitetura.md).

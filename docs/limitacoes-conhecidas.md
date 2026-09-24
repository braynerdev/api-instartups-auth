# Limitações conhecidas e pontos de atenção

Esta lista documenta objetivamente débitos técnicos e comportamentos observados diretamente no código atual (não são suposições). O objetivo é dar visibilidade, não criticar — vários desses pontos são naturais para o estágio atual do projeto.

## Ausência de automação

- **Sem testes automatizados:** não há projeto de testes nem dependência de framework de testes (`xunit`/`nunit`/`MSTest`) em `Api.Instartups.Auth.csproj`.
- **Sem CI/CD:** a pasta `.github/workflows/` existe, mas está vazia — nenhum pipeline de build/teste/publish está configurado.
- **Sem lint automatizado além do `.editorconfig`:** as convenções de estilo existem, mas não há um passo de CI que as valide.

## Configuração e segredos

- **Segredos de exemplo versionados:** `appsettings.json` traz uma senha de banco em texto puro (`Password=1208`); `appsettings.Development.json` traz uma senha de admin (`Admin@123`) e uma chave AES (`Cursor:SecretKey`) reais, prontas para uso — adequadas apenas para desenvolvimento local, nunca para produção.
- **Chaves RSA versionadas:** o par `Keys/jwt-private.pem` / `Keys/jwt-public.pem` usado para assinar os tokens JWT está presente na árvore de trabalho do projeto. Um commit (`d7347d1`, "adicionando pasta Keys ao .gitignore") sugere que houve uma tentativa de excluir essa pasta do controle de versão — vale conferir no histórico do repositório se a chave privada chegou a ser commitada antes disso, e trocá-la caso tenha sido exposta.
- **`Jwt:Audience` configurado mas sem efeito:** a chave existe em `appsettings.json` e é lida para `JwtOptions.Audience`, mas nunca é usada ao gerar o token (`GenerateJwtService` não passa `audience` para `JwtSecurityToken`) nem validada no consumo (`AuthenticationConf` define `ValidateAudience = false`). É uma configuração morta hoje.

## Segurança de senha

- **Política nativa do Identity desativada:** `IdentityConf.cs` desliga todas as exigências nativas de complexidade de senha do ASP.NET Identity (`RequireDigit/Lowercase/Uppercase/NonAlphanumeric = false`, `RequiredLength = 1`). Toda a força da política real vem das regras do FluentValidation (`PasswordExtensions.ValidationPassword()`), que é aplicada a nível de comando/caso de uso — se algum novo fluxo de criação/alteração de senha for adicionado sem usar essa extensão, ele herdará a política permissiva do Identity.
- **Iteração de hash de senha reduzida:** `PasswordHasherOptions.IterationCount = 1000`, abaixo dos valores tipicamente recomendados para hashing de senha baseado em PBKDF2 (o padrão do ASP.NET Identity é uma ordem de grandeza maior). Isso reduz o custo computacional de um ataque de força bruta offline caso o hash vaze.

## Rede e transporte

- **HTTPS redirection desabilitado:** `app.UseHttpsRedirection()` está presente em `Program.cs`, porém comentado — a aplicação aceita tráfego HTTP sem redirecionar para HTTPS.
- **Sem CORS configurado:** não existe nenhuma política de CORS registrada; por padrão, requisições cross-origin de navegadores serão bloqueadas pelo navegador (nenhum header `Access-Control-*` é emitido), o que pode ser um bloqueio a resolver quando um frontend em outro domínio precisar consumir a API.
- **Sem rate limiting:** não há middleware de limitação de taxa de requisições (ex.: contra tentativas de força bruta em `/api/auth/login`, além do lockout nativo do Identity após 5 tentativas falhas).

## Comportamento de código

- **`GetMeQueryHandler` sem checagem de nulo:** em `GET /api/user/me`, o handler busca o usuário pelo id do token (`FindByIdAsync`) e acessa seus campos sem verificar se o resultado é nulo. Se o usuário associado ao token tiver sido excluído do banco enquanto o access token ainda é válido, a requisição resulta em erro interno (`500`) em vez de uma resposta de erro mais apropriada (ex.: `401`/`404`).
- **Inconsistência no arquivo de exemplos `.http`:** `Api.Instartups.Auth.http` rotula um exemplo como "Dados do usuário autenticado" apontando para `GET /api/auth/me`, rota que não existe no `AuthController`. O endpoint correto é `GET /api/user/me`.
- **Permissões definidas mas não aplicadas:** `Users.View`, `Users.Create`, `Users.Update`, `Users.Delete` e `Sessions.Revoke` existem como policies registradas (`AuthorizationConfig`) e podem ser concedidas/revogadas via os endpoints de permissão, mas nenhum endpoint atual exige essas policies especificamente — só `Admin` é de fato usado para proteger rotas hoje.

## Escopo do serviço

- **Paginação por cursor limitada a uma consulta:** o mecanismo de cursor criptografado (`CursorEncoder`/`CursorPage`) hoje só é usado por `GET /api/user` (listagem de usuários), embora tenha sido escrito de forma reutilizável.
- **Projeto irmão `Instartups` fora de escopo:** o repositório vizinho `Instartups/` (modelagem de domínio de perfis de startups/investidores) não possui API, banco ou integração com este serviço de autenticação — não documentado aqui por não ter comportamento observável em tempo de execução.

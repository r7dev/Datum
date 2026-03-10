# Datum Blog API

API REST de blog construída com **.NET 8**, seguindo arquitetura em camadas, princípios **SOLID**, autenticação **JWT**, banco de dados **SQL Server** com abordagem **Database First** via Entity Framework Core, e notificações em tempo real com **WebSockets**.

---

## Índice

- [Visão Geral](#visão-geral)
- [Tecnologias](#tecnologias)
- [Arquitetura](#arquitetura)
- [Estrutura da Solution](#estrutura-da-solution)
- [Princípios SOLID](#princípios-solid)
- [Banco de Dados](#banco-de-dados)
- [Autenticação](#autenticação)
- [Endpoints da API](#endpoints-da-api)
- [WebSockets](#websockets)
- [Testes](#testes)
- [Como Executar](#como-executar)
- [Configuração](#configuração)
- [Pacotes NuGet](#pacotes-nuget)

---

## Visão Geral

O **Datum Blog API** é um sistema de blog que permite o gerenciamento completo de postagens com controle de autoria. Qualquer visitante pode visualizar os posts publicados, enquanto usuários autenticados podem criar, editar e excluir suas próprias postagens. Ao criar um novo post, todos os clientes conectados via WebSocket recebem uma notificação em tempo real.

**Funcionalidades principais:**

- Registro e login de usuários com senha armazenada em hash BCrypt
- Emissão e validação de tokens JWT com expiração de 24 horas
- CRUD completo de postagens com controle de propriedade por autor
- Notificações em tempo real via WebSocket ao publicar novos posts
- Documentação interativa via Swagger UI com suporte a autenticação Bearer

---

## Tecnologias

| Tecnologia | Versão | Uso |
|---|---|---|
| .NET | 8.0 | Plataforma base |
| ASP.NET Core | 8.0 | Web API e WebSockets |
| Entity Framework Core | 8.0 | ORM — Database First |
| SQL Server | 2019+ | Banco de dados relacional |
| JWT Bearer | 8.0 | Autenticação stateless |
| BCrypt.Net | 4.0.3 | Hash seguro de senhas |
| Swagger / Swashbuckle | 6.5.0 | Documentação da API |
| xUnit | 2.6.6 | Framework de testes |
| Moq | 4.20.70 | Mock de dependências |
| FluentAssertions | 6.12.0 | Asserções expressivas |
| EF Core InMemory | 8.0 | Banco em memória para testes |

---

## Arquitetura

O projeto segue uma **arquitetura monolítica em camadas**, com separação clara de responsabilidades entre os projetos. A regra de dependência flui em apenas uma direção: camadas externas dependem das internas, nunca o contrário.

```
┌─────────────────────────────────────┐
│           Datum.BlogAPI             │  ← Apresentação (HTTP, WebSocket)
│     Controllers  │  Middleware      │
└──────────────┬──────────────────────┘
               │ depende de
┌──────────────▼──────────────────────┐
│         Datum.Infrastructure        │  ← Infraestrutura (EF Core, serviços)
│  Repositories  │  Services  │  Data │
└──────────────┬──────────────────────┘
               │ depende de
┌──────────────▼──────────────────────┐
│           Datum.Domain              │  ← Domínio (entidades, interfaces, DTOs)
│  Entities  │  Interfaces  │  DTOs   │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│           Datum.Tests               │  ← Testes (xUnit + Moq + InMemory)
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│          Datum.Database             │  ← Scripts SQL para SQL Server
└─────────────────────────────────────┘
```

**Por que essa separação?**

- `Datum.Domain` não tem dependências externas — pode ser testado e evoluído isoladamente
- `Datum.Infrastructure` conhece apenas o domínio, nunca a API
- `Datum.BlogAPI` é a única camada que lida com HTTP — troca de protocolo não afeta o restante
- `Datum.Tests` referencia Domain e Infrastructure diretamente, sem depender da API

---

## Estrutura da Solution

```
Datum.sln
│
├── Datum.Domain/
│   ├── Entities/
│   │   ├── User.cs                         # Entidade de usuário
│   │   └── Post.cs                         # Entidade de postagem
│   ├── DTOs/
│   │   └── BlogDTOs.cs                     # Requests, Responses e Notifications
│   └── Interfaces/
│       ├── Services/
│       │   ├── IAuthService.cs             # Contrato de autenticação
│       │   ├── IPostService.cs             # Contrato de posts
│       │   ├── IJwtService.cs              # Contrato de geração de token
│       │   └── INotificationService.cs     # Contrato de WebSocket
│       └── Repositories/
│           ├── IUserRepository.cs          # Contrato de persistência de usuários
│           └── IPostRepository.cs          # Contrato de persistência de posts
│
├── Datum.Infrastructure/
│   ├── Data/
│   │   └── ApplicationDbContext.cs         # DbContext com mapeamento Database First
│   ├── Repositories/
│   │   ├── UserRepository.cs               # Implementação IUserRepository
│   │   └── PostRepository.cs               # Implementação IPostRepository
│   └── Services/
│       ├── AuthService.cs                  # Registro e login
│       ├── PostService.cs                  # CRUD de postagens + notificação
│       ├── JwtService.cs                   # Geração e leitura de tokens JWT
│       └── NotificationService.cs          # Gerenciamento de conexões WebSocket
│
├── Datum.BlogAPI/
│   ├── Controllers/
│   │   ├── AuthController.cs               # POST /api/auth/register|login
│   │   └── PostsController.cs              # GET|POST|PUT|DELETE /api/posts
│   ├── Middleware/
│   │   └── WebSocketMiddleware.cs          # Intercepta conexões em /ws/notifications
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── Program.cs                          # Configuração de DI, JWT, Swagger, pipeline
│   ├── appsettings.json
│   └── appsettings.Development.json
│
├── Datum.Database/
│   └── Scripts/
│       ├── 01_CreateDatabase.sql           # Cria o banco [datum]
│       ├── 02_CreateTables.sql             # Cria dbo.Users e dbo.Posts
│       ├── 03_CreateIndexes.sql            # Índices de performance
│       └── 04_SeedData.sql                 # Dados iniciais (opcional)
│
└── Datum.Tests/
    ├── Helpers/
    │   ├── DbContextFactory.cs             # Cria DbContext InMemory isolado por teste
    │   └── EntityBuilder.cs               # Construtores de entidades para testes
    ├── Services/
    │   ├── AuthServiceTests.cs             # 8 testes de AuthService
    │   └── PostServiceTests.cs             # 13 testes de PostService
    └── Repositories/
        ├── UserRepositoryTests.cs          # 9 testes de UserRepository
        └── PostRepositoryTests.cs          # 11 testes de PostRepository
```

---

## Princípios SOLID

### S — Single Responsibility Principle
Cada classe tem exatamente uma razão para mudar:

- `AuthService` — somente autenticação (registro e login)
- `PostService` — somente regras de negócio de postagens
- `JwtService` — somente geração e leitura de tokens JWT
- `NotificationService` — somente gerenciamento de conexões WebSocket e envio de mensagens
- `UserRepository` / `PostRepository` — somente persistência de suas respectivas entidades

### O — Open/Closed Principle
Toda lógica está atrás de interfaces. Para adicionar, por exemplo, autenticação via OAuth, basta criar uma nova implementação de `IAuthService` e registrá-la no container — sem tocar no código existente.

### L — Liskov Substitution Principle
As implementações concretas (`AuthService`, `PostService`, etc.) são completamente substituíveis por seus contratos. Os controllers e serviços que dependem de interfaces funcionam corretamente com qualquer implementação que respeite o contrato.

### I — Interface Segregation Principle
As interfaces são específicas e coesas. `IUserRepository` e `IPostRepository` estão separados — nenhum consumidor é forçado a depender de métodos que não usa. O mesmo vale para os serviços: `IJwtService` é separado de `IAuthService`.

### D — Dependency Inversion Principle
Controllers dependem de `IAuthService` e `IPostService`, nunca de `AuthService` ou `PostService` diretamente. Serviços dependem de `IUserRepository` e `IPostRepository`. Todas as dependências são injetadas via construtor e registradas no container em `Program.cs`.

```csharp
// Controllers dependem de interfaces, nunca de implementações
public PostsController(IPostService postService) { ... }

// Serviços dependem de repositórios via interface
public PostService(IPostRepository postRepository, INotificationService notificationService) { ... }

// Program.cs — registros de DI
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
```

---

## Banco de Dados

O projeto utiliza a abordagem **Database First**: o schema é definido e gerenciado pelos scripts SQL em `Datum.Database`. O Entity Framework Core apenas mapeia as tabelas já existentes — **não há uso de migrations**.

### Diagrama de Entidades

```
┌─────────────────────────┐        ┌──────────────────────────────┐
│       dbo.Users         │        │         dbo.Posts            │
├─────────────────────────┤        ├──────────────────────────────┤
│ Id           INT (PK)   │◄───┐   │ Id           INT (PK)        │
│ Username     NVARCHAR   │    └───│ UserId       INT (FK)        │
│ Email        NVARCHAR   │        │ Title        NVARCHAR(200)   │
│ PasswordHash NVARCHAR   │        │ Content      NVARCHAR(MAX)   │
│ CreatedAt    DATETIME2  │        │ CreatedAt    DATETIME2       │
└─────────────────────────┘        │ UpdatedAt    DATETIME2 NULL  │
                                   └──────────────────────────────┘
```

### Scripts de implantação

Execute os scripts na ordem abaixo, conectado ao seu SQL Server:

```bash
# Via sqlcmd
sqlcmd -S localhost -E -i "Datum.Database/Scripts/01_CreateDatabase.sql"
sqlcmd -S localhost -d datum -E -i "Datum.Database/Scripts/02_CreateTables.sql"
sqlcmd -S localhost -d datum -E -i "Datum.Database/Scripts/03_CreateIndexes.sql"
sqlcmd -S localhost -d datum -E -i "Datum.Database/Scripts/04_SeedData.sql"  # opcional
```

Ou execute diretamente pelo **SSMS** ou **Azure Data Studio**.

| Script | O que faz |
|---|---|
| `01_CreateDatabase.sql` | Cria o banco `datum` com filegroups e recovery model definidos |
| `02_CreateTables.sql` | Cria `dbo.Users` e `dbo.Posts` com constraints, defaults e FK |
| `03_CreateIndexes.sql` | Cria índices não-clusterizados para performance em listagens e buscas |
| `04_SeedData.sql` | Insere usuários e posts de exemplo para desenvolvimento e testes manuais |

> Todos os scripts são **idempotentes**: podem ser executados múltiplas vezes sem causar erros ou duplicações.

---

## Autenticação

O sistema utiliza **JWT Bearer Token** com as seguintes características:

- Senhas armazenadas com **BCrypt** (fator de custo 11)
- Tokens com expiração de **24 horas**
- Claims incluídas no token: `NameIdentifier` (Id), `Email`, `Name` (Username)
- Validação de Issuer, Audience, Lifetime e assinatura em todas as requisições autenticadas

**Fluxo de autenticação:**

```
1. POST /api/auth/register  →  cria usuário, retorna token JWT
2. POST /api/auth/login     →  valida credenciais, retorna token JWT
3. GET/POST/PUT/DELETE ...  →  Authorization: Bearer {token}
```

---

## Endpoints da API

A documentação interativa completa está disponível via Swagger em `http://localhost:5000` ao executar o projeto em modo Development.

### Autenticação

| Método | Rota | Autenticação | Descrição |
|---|---|---|---|
| `POST` | `/api/auth/register` | Não | Registra novo usuário e retorna token JWT |
| `POST` | `/api/auth/login` | Não | Autentica usuário e retorna token JWT |

**Exemplo — Register:**
```json
// POST /api/auth/register
// Body:
{
  "username": "joao",
  "email": "joao@email.com",
  "password": "Senha@123"
}

// Response 201:
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "username": "joao",
  "email": "joao@email.com"
}
```

### Postagens

| Método | Rota | Autenticação | Descrição |
|---|---|---|---|
| `GET` | `/api/posts` | Não | Lista todas as postagens (ordem: mais recente primeiro) |
| `GET` | `/api/posts/{id}` | Não | Retorna uma postagem pelo ID |
| `POST` | `/api/posts` | JWT | Cria nova postagem e notifica clientes WebSocket |
| `PUT` | `/api/posts/{id}` | JWT | Edita postagem (somente o autor) |
| `DELETE` | `/api/posts/{id}` | JWT | Exclui postagem (somente o autor) |

**Exemplo — Criar post:**
```json
// POST /api/posts
// Header: Authorization: Bearer {token}
// Body:
{
  "title": "Minha primeira postagem",
  "content": "Conteúdo da postagem..."
}

// Response 201:
{
  "id": 1,
  "title": "Minha primeira postagem",
  "content": "Conteúdo da postagem...",
  "authorUsername": "joao",
  "authorId": 1,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null
}
```

**Códigos de resposta:**

| Código | Situação |
|---|---|
| `200 OK` | Operação bem-sucedida (GET, PUT) |
| `201 Created` | Recurso criado com sucesso (POST) |
| `204 No Content` | Recurso excluído com sucesso (DELETE) |
| `400 Bad Request` | Dados inválidos ou regra de negócio violada |
| `401 Unauthorized` | Token ausente, inválido ou expirado |
| `403 Forbidden` | Usuário autenticado mas sem permissão (ex: editar post de outro usuário) |
| `404 Not Found` | Recurso não encontrado |

---

## WebSockets

O sistema implementa notificações em tempo real usando WebSockets nativos do ASP.NET Core. Ao criar uma nova postagem, todos os clientes conectados recebem uma mensagem automaticamente.

**Endpoint:** `ws://localhost:5000/ws/notifications`

**Mensagem recebida ao criar um post:**
```json
{
  "type": "NEW_POST",
  "data": {
    "postId": 1,
    "title": "Minha primeira postagem",
    "authorUsername": "joao",
    "createdAt": "2024-01-15T10:30:00Z"
  }
}
```

**Como testar com wscat:**
```bash
npm install -g wscat
wscat -c ws://localhost:5000/ws/notifications
# Mantenha a conexão aberta e crie um post pela API — a notificação chegará aqui
```

**Como testar com JavaScript no browser:**
```javascript
const ws = new WebSocket('ws://localhost:5000/ws/notifications');
ws.onopen    = () => console.log('Conectado ao Datum WebSocket');
ws.onmessage = (e) => console.log('Notificação recebida:', JSON.parse(e.data));
ws.onclose   = () => console.log('Conexão encerrada');
```

**Detalhes de implementação:**

- `NotificationService` é registrado como **Singleton** — mantém a lista de conexões ativas durante toda a vida da aplicação
- Conexões inativas são ignoradas automaticamente ao notificar
- O keep-alive interval é de 2 minutos, configurável em `Program.cs`
- O `WebSocketMiddleware` intercepta requisições antes dos controllers, sem interferir no pipeline HTTP normal

---

## Testes

O projeto `Datum.Tests` cobre as camadas de **Services** e **Repositories** da Infrastructure, com **30 testes** no total.

### Estratégia

| Camada | Abordagem | Motivo |
|---|---|---|
| Services | **Moq** para mockar repositórios | Testa lógica de negócio isolada, sem banco |
| Repositories | **EF Core InMemory** | Testa queries e comportamento do EF com banco isolado por teste |

### Helpers

**`DbContextFactory`** — cria um `ApplicationDbContext` com banco InMemory de nome único (GUID) por teste, garantindo isolamento total entre execuções paralelas.

**`EntityBuilder`** — construtores com valores padrão para `User` e `Post`, eliminando boilerplate nos arranjos dos testes.

### Cobertura

| Arquivo | Testes | Cenários cobertos |
|---|---|---|
| `AuthServiceTests` | 8 | Registro válido, e-mail duplicado, username duplicado, verificação de hash BCrypt, login correto, e-mail inexistente, senha errada, geração de token |
| `PostServiceTests` | 13 | GetAll vazio e com dados, GetById existente e inexistente, Create com notificação WS, Create com falha no reload, Update correto, Update post inexistente, Update por não-autor, UpdatedAt preenchido, Delete correto, Delete inexistente, Delete por não-autor, Delete não invoca repositório em erro |
| `UserRepositoryTests` | 9 | GetById existente e inexistente, GetByEmail existente e inexistente, ExistsByEmail true e false, ExistsByUsername true e false, Add persiste no banco, Add atribui Id, Add retorna mesma instância |
| `PostRepositoryTests` | 11 | GetAll com múltiplos posts, GetAll inclui Author, GetAll ordenação decrescente, GetAll lista vazia, GetById existente e inexistente, GetById inclui Author, Add atribui Id, Update persiste alterações, Delete remove post, Delete não afeta outros posts |

### Executar os testes

```bash
# Todos os testes
dotnet test

# Somente testes de services
dotnet test --filter "FullyQualifiedName~Services"

# Somente testes de repositories
dotnet test --filter "FullyQualifiedName~Repositories"

# Com cobertura de código (requer coverlet)
dotnet test --collect:"XPlat Code Coverage"
```

No Visual Studio, acesse **Test Explorer** (`Ctrl+E, T`) e clique em **Run All Tests**.

---

## Como Executar

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [SQL Server](https://www.microsoft.com/sql-server) 2019 ou superior (ou SQL Server Express)
- [SSMS](https://aka.ms/ssms) ou [Azure Data Studio](https://azure.microsoft.com/products/data-studio) (opcional)

### Passo a passo

**1. Clone o repositório**
```bash
git clone https://github.com/seu-usuario/datum.git
cd datum
```

**2. Crie o banco de dados**

Execute os scripts em ordem no seu SQL Server:
```bash
sqlcmd -S localhost -E -i "Datum.Database/Scripts/01_CreateDatabase.sql"
sqlcmd -S localhost -d datum -E -i "Datum.Database/Scripts/02_CreateTables.sql"
sqlcmd -S localhost -d datum -E -i "Datum.Database/Scripts/03_CreateIndexes.sql"
```

**3. Configure a connection string**

Edite `Datum.BlogAPI/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=datum;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**4. Execute a API**
```bash
cd Datum.BlogAPI
dotnet restore
dotnet run
```

A API estará disponível em `http://localhost:5000`. O Swagger abrirá automaticamente no browser.

**5. Execute os testes**
```bash
dotnet test
```

---

## Configuração

### appsettings.json — opções disponíveis

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=datum;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "SuaChaveSecretaComMinimo32Caracteres!",
    "Issuer": "Datum",
    "Audience": "DatumUsers"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

> **Segurança:** Nunca versione a `SecretKey` em repositórios públicos. Em produção, utilize variáveis de ambiente ou Azure Key Vault.

**Via variáveis de ambiente:**
```bash
export JwtSettings__SecretKey="SuaChaveSecretaSegura"
export ConnectionStrings__DefaultConnection="Server=...;Database=datum;..."
```

### Exemplos de connection strings

```
# Windows — Autenticação integrada
Server=localhost;Database=datum;Trusted_Connection=True;TrustServerCertificate=True;

# SQL Server com usuário e senha
Server=localhost;Database=datum;User Id=sa;Password=SuaSenha;TrustServerCertificate=True;

# SQL Server Express
Server=localhost\SQLEXPRESS;Database=datum;Trusted_Connection=True;TrustServerCertificate=True;

# Azure SQL
Server=seu-servidor.database.windows.net;Database=datum;User Id=seu_usuario;Password=SuaSenha;Encrypt=True;
```

---

## Pacotes NuGet

### Datum.Infrastructure

| Pacote | Versão | Finalidade |
|---|---|---|
| `Microsoft.EntityFrameworkCore` | 8.0.0 | ORM base |
| `Microsoft.EntityFrameworkCore.SqlServer` | 8.0.0 | Provider SQL Server |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.0 | Validação de tokens JWT |
| `BCrypt.Net-Next` | 4.0.3 | Hash seguro de senhas |

### Datum.BlogAPI

| Pacote | Versão | Finalidade |
|---|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | 8.0.0 | Configuração do DbContext |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.0 | Middleware de autenticação |
| `Swashbuckle.AspNetCore` | 6.5.0 | Documentação Swagger/OpenAPI |

### Datum.Tests

| Pacote | Versão | Finalidade |
|---|---|---|
| `xunit` | 2.6.6 | Framework de testes |
| `xunit.runner.visualstudio` | 2.5.6 | Integração com Test Explorer |
| `Microsoft.NET.Test.Sdk` | 17.8.0 | Runner de testes .NET |
| `Moq` | 4.20.70 | Mock de interfaces |
| `FluentAssertions` | 6.12.0 | Asserções legíveis |
| `Microsoft.EntityFrameworkCore.InMemory` | 8.0.0 | Banco em memória para testes |
| `BCrypt.Net-Next` | 4.0.3 | Verificação de hash nos testes |

---

## Licença

Este projeto foi desenvolvido para fins educacionais e de demonstração técnica.

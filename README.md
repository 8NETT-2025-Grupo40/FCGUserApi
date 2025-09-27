# FCG – User API

API de **Usuários** da FIAP Cloud Games (FCG). Responsável por cadastro, autenticação (JWT) e consulta de usuários.
Tecnologias: **.NET 8 (Minimal APIs)**, **EF Core (SQL Server)**, **Swagger**, **Serilog**, **OpenTelemetry**.

## Arquitetura hexagonal
* src/FCGUser.Api -> Adapter HTTP
* src/FCGUser.Application -> Use cases
* src/FCGUser.Domain -> Core
* src/FCGUser.Infra -> Adapters (EF, Auth)

* Observabilidade: **OpenTelemetry** (ASP.NET/HTTP/EF) + exportação OTLP (compatível com ADOT Collector/AWS X-Ray).
* Logs estruturados: **Serilog**.
  
## Como rodar (Local)

```bash
# na raiz do repo
dotnet restore FCGUserApi.sln
dotnet build   FCGUserApi.sln -c Release

# subir API
dotnet run --project src/Adapters/Drivers/FCGUser.Api/FCGUser.Api.csproj
# Swagger: http://localhost:5067/docs
```

## Como rodar (Docker)

```bash
docker build -t fcg-user-api:dev .
docker run --rm -p 5067:5067 --env-file .env fcg-user-api:dev
```

## Banco de dados (EF Core)

Migrations no projeto

```bash
# aplicar no banco
dotnet ef database update \
  -p src/Adapters/Driven/FCGUser.Infra/FCGUser.Infra.csproj \
  -s src/Adapters/Drivers/FCGUser.Api/FCGUser.Api.csproj
```

## Testes
Testes unitários feitos em XUnit e NSubstitute

## Padrões e decisões

* **DDD**: entidade `User`, invariantes básicas, portas/adapters.
* **Autenticação**: JWT Token;
* **Observabilidade**: tracing ASP.NET/HTTP/EF + OTLP exporter.

## CI/CD

* **CI** (PR/commit): restore, build, testes, análise estática.
* **Docker**: imagem **ASP.NET 8**
* **CD** (merge → main): build/push para **ECR**, atualização do **ECS Fargate Service** 
* **Infra AWS**:
  * **ECS Fargate** + **ALB** (Path `/users/*`)
  * **RDS** (SQL Server)
  * **CloudWatch Logs** / **X-Ray** / **OTel Collector**

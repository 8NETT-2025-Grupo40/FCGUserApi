# FCGUserApi

Microserviço de usuário — arquitetura hexagonal — .NET 8

## Como rodar

- Ajuste `docker-compose.yml` com senha
- `docker-compose up --build`

## Estrutura
- src/FCGUser.Api -> Adapter HTTP
- src/FCGUser.Application -> Use cases
- src/FCGUser.Domain -> Core
- src/FCGUser.Infra -> Adapters (EF, Auth)
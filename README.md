# FCGUserApi

Microserviço de usuário — arquitetura hexagonal — .NET 8

## Como rodar

pelo Docker
docker build -t fcguserapi:dev .
docker run -p 5000:80 fcguserapi:dev

## Estrutura
- src/FCGUser.Api -> Adapter HTTP
- src/FCGUser.Application -> Use cases
- src/FCGUser.Domain -> Core
- src/FCGUser.Infra -> Adapters (EF, Auth)


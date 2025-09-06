# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia primeiro a solution
COPY FCGUserApi.sln ./

# Copia os arquivos .csproj para os caminhos esperados pelo .sln
COPY src/Adapters/Drivers/FCGUser.Api/FCGUser.Api.csproj ./src/FCGUser.Api/
COPY src/Core/FCGUser.Application/FCGUser.Application.csproj ./src/FCGUser.Application/
COPY src/Core/FCGUser.Domain/FCGUser.Domain.csproj ./src/FCGUser.Domain/
COPY src/Adapters/Driven/FCGUser.Infra/FCGUser.Infra.csproj ./src/FCGUser.Infra/

# Restaura as dependências
RUN dotnet restore FCGUserApi.sln

# Agora copia todo o código fonte
COPY src/ ./src/

# Publica a API no modo Release
RUN dotnet publish src/Adapters/Drivers/FCGUser.Api/FCGUser.Api.csproj -c Release -o /app/publish

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish ./

# Expõe a porta 80
EXPOSE 80

# Faz o Kestrel escutar em todas as interfaces (necessário no Docker)
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "FCGUser.Api.dll"]

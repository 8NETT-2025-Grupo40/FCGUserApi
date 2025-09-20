# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia solution
COPY FCGUserApi.sln ./

# Copia toda a pasta src de uma vez, mantendo a estrutura
COPY src/ ./src/

# Restaura dependências (agora os caminhos batem)
RUN dotnet restore FCGUserApi.sln

# Publica a API
RUN dotnet publish src/Adapters/Drivers/FCGUser.Api/FCGUser.Api.csproj -c Release -o /app/publish

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:5067
EXPOSE 5067

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "FCGUser.Api.dll"]

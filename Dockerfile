# ==================================
# Stage 1: Runtime base (Alpine)
# ==================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base

# Suporte a globalização (formatação de datas, JWT claims, etc.)
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Segurança: executa como usuário não-root
USER app
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:5067
EXPOSE 5067

# ==================================
# Stage 2: Build (Alpine)
# ==================================
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia solution e projetos para cache de restore
COPY FCGUserApi.sln ./
COPY src/Adapters/Drivers/FCGUser.Api/FCGUser.Api.csproj src/Adapters/Drivers/FCGUser.Api/
COPY src/Core/FCGUser.Application/FCGUser.Application.csproj src/Core/FCGUser.Application/
COPY src/Core/FCGUser.Domain/FCGUser.Domain.csproj src/Core/FCGUser.Domain/
COPY src/Adapters/Driven/FCGUser.Infra/FCGUser.Infra.csproj src/Adapters/Driven/FCGUser.Infra/
COPY src/UnitTests/UnitTests.csproj src/UnitTests/

# Restore separado (melhor cache)
RUN dotnet restore FCGUserApi.sln

# Copia código fonte
COPY src/ ./src/

# Build e publish
WORKDIR /src/src/Adapters/Drivers/FCGUser.Api
RUN dotnet publish FCGUser.Api.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# ==================================
# Stage 3: Final
# ==================================
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FCGUser.Api.dll"]

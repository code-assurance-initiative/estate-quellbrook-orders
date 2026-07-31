# syntax=docker/dockerfile:1
# Build from the repository root:
#   docker build -f src/Quellbrook.Orders.Api/Dockerfile -t ghcr.io/code-assurance-initiative/quellbrook-orders .
#
# Base images are pinned by digest (the 10.0 tag at the last bump); Dependabot proposes the next digest.
# No HEALTHCHECK: the image runs only on Kubernetes, which ignores it; probes live in deploy/k8s/deployment.yaml.
FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:e70cdb7f80b0348f5cb85f19a8f670fca061f033d57eed12fa003d58b0e06317 AS build
WORKDIR /src
COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/.editorconfig src/
COPY src/Quellbrook.Orders.Domain/Quellbrook.Orders.Domain.csproj src/Quellbrook.Orders.Domain/packages.lock.json src/Quellbrook.Orders.Domain/
COPY src/Quellbrook.Orders.Contracts/Quellbrook.Orders.Contracts.csproj src/Quellbrook.Orders.Contracts/packages.lock.json src/Quellbrook.Orders.Contracts/
COPY src/Quellbrook.Orders.Application/Quellbrook.Orders.Application.csproj src/Quellbrook.Orders.Application/packages.lock.json src/Quellbrook.Orders.Application/
COPY src/Quellbrook.Orders.Infrastructure/Quellbrook.Orders.Infrastructure.csproj src/Quellbrook.Orders.Infrastructure/packages.lock.json src/Quellbrook.Orders.Infrastructure/
COPY src/Quellbrook.Orders.Api/Quellbrook.Orders.Api.csproj src/Quellbrook.Orders.Api/packages.lock.json src/Quellbrook.Orders.Api/
RUN dotnet restore src/Quellbrook.Orders.Api/Quellbrook.Orders.Api.csproj --locked-mode
COPY src/ src/
RUN dotnet publish src/Quellbrook.Orders.Api/Quellbrook.Orders.Api.csproj -c Release --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:222759b391a1aaf241166672c8f99b2d4ada452e7b5319f3c6e8f265a37b5ad4 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
# An unprivileged UID above any host account range, by number so the kubelet can verify runAsNonRoot.
USER 10001:10001
ENTRYPOINT ["dotnet", "Quellbrook.Orders.Api.dll"]

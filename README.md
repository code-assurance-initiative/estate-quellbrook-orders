# Quellbrook Orders

The order service of Quellbrook Freight's operations platform. Customer-service operators place orders for shippers
— one consignee, one to twenty parcels, standard or same-day express service — through the operator console; the
console reaches this service through the API gateway, over the cluster's service mesh (mutual TLS). The service validates the order against the carrier's rules,
stores it in PostgreSQL and announces it to the rest of the platform as an `orders.order-placed.v1` event on the
RabbitMQ topic exchange `quellbrook.events`, where the dispatch service plans its delivery and the notifier tells the
consignee.

**Owner:** the Orders team (`#team-orders`). **System:** Quellbrook operations platform (see `catalog-info.yaml`).

## API

| Method and path | Scope | What it does |
|---|---|---|
| `POST /orders` | `orders:write` | Place an order (`X-Quellbrook-Operator` names the operator) |
| `GET /orders/{id}` | `orders:read` | Read one order |
| `GET /orders?page=&pageSize=` | `orders:read` | List orders, newest first |
| `GET /health/live`, `GET /health/ready` | none | Liveness and readiness probes |

The contract is `contracts/openapi.yaml`. Only the gateway holds a token with these scopes; it authenticates the
operator and forwards the operator's id.

## Events

| Routing key | When |
|---|---|
| `orders.order-placed.v1` | an order was placed |

## Build and run

Requires the .NET 10 SDK (`global.json`), a PostgreSQL 16 database and a RabbitMQ broker.

```sh
dotnet tool restore
dotnet build Quellbrook.Orders.slnx
export ConnectionStrings__Orders="Host=localhost;Database=orders;Username=orders"
dotnet ef database update --project src/Quellbrook.Orders.Infrastructure
dotnet run --project src/Quellbrook.Orders.Api
```

## Configuration

| Setting | Meaning |
|---|---|
| `ConnectionStrings__Orders` | PostgreSQL connection string (a Kubernetes Secret in production) |
| `Authentication__Authority`, `Authentication__Audience` | the OpenID Connect issuer and this API's audience |
| `RabbitMq__Uri`, `RabbitMq__UserName`, `RabbitMq__Password`, `RabbitMq__Exchange` | the broker; credentials from a Secret |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | where traces, metrics and logs are exported, when set |

Secrets are never written to `appsettings*.json`; supply them through environment variables or user secrets.

## Testing

```sh
dotnet test Quellbrook.Orders.slnx
```

`tests/Quellbrook.Orders.UnitTests` covers the Order aggregate and its value objects, the command handlers, the
repository and queries (over in-memory SQLite) and the broker publisher (against a substituted channel).
`tests/Quellbrook.Orders.IntegrationTests` hosts the real API with `WebApplicationFactory` over in-memory SQLite, an
in-process token issuer and a recording publisher. No test touches the network.

## Architecture

Domain, application, infrastructure and API projects, with dependencies pointing inward (ADR 0002). See
[docs/architecture.md](docs/architecture.md) and the decision records in [docs/adr](docs/adr).

## Deployment

A published GitHub release builds the image and the migrations bundle (`.github/workflows/release.yml`); the manual
`deploy.yml` workflow migrates the database and rolls the image out to the `quellbrook-orders` namespace
(`deploy/k8s/`), rolling back when the new pods do not become ready.

## Contributing

Open a pull request against `main`. CI must be green and CodeQL must report no new alerts. Schema changes come with
an EF Core migration (`dotnet ef migrations add`); never edit an applied migration. Record user-visible changes in
`CHANGELOG.md`.

## Licence

MIT; see [LICENSE](LICENSE).

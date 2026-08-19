# Architecture — order service

## Context

The order service is one of five parts of the Quellbrook operations platform. Operators work in the web console;
the console calls the API gateway; the gateway calls this service with its own client credentials and forwards the
operator's id. Traffic between services inside the cluster goes through the Linkerd service mesh, which encrypts and
authenticates it with mutual TLS; the services themselves listen on plain HTTP inside their pods and are reachable
only from the namespaces their NetworkPolicies allow. Placed orders are announced on the platform's event broker.

```mermaid
C4Context
  title System context — Quellbrook operations platform
  Person(operator, "Operator", "Quellbrook customer service and dispatch staff")
  Person_Ext(consignee, "Consignee", "Receives the parcels and the notifications")
  System_Boundary(platform, "Quellbrook operations platform") {
    System(web, "Operator console", "estate-quellbrook-web")
    System(gateway, "API gateway / BFF", "estate-quellbrook-gateway")
    System(orders, "Order service", "this repository")
    System(dispatch, "Dispatch service", "estate-quellbrook-dispatch")
    System(notifier, "Notifier", "estate-quellbrook-notifier")
  }
  System_Ext(idp, "Identity provider", "OpenID Connect")
  Rel(operator, web, "Uses", "HTTPS")
  Rel(web, gateway, "Calls /api", "HTTPS/JSON")
  Rel(gateway, orders, "Places and reads orders", "HTTP/JSON, mesh mTLS")
  Rel(gateway, dispatch, "Plans routes", "HTTP/JSON, mesh mTLS")
  Rel(orders, dispatch, "orders.* events", "RabbitMQ")
  Rel(orders, notifier, "orders.* events", "RabbitMQ")
  Rel(notifier, consignee, "E-mail and SMS")
  Rel(gateway, idp, "Verifies operators, obtains service tokens", "HTTPS")
```

## Containers

```mermaid
C4Container
  title Containers — order service
  System_Ext(gateway, "API gateway / BFF", "estate-quellbrook-gateway")
  Container_Boundary(service, "Order service") {
    Container(api, "Orders API", "ASP.NET Core, .NET 10", "Places, reads, lists and cancels orders; outbox relay")
    ContainerDb(db, "Orders database", "PostgreSQL 16", "orders, outbox_messages")
  }
  ContainerQueue(broker, "Event broker", "RabbitMQ", "topic exchange quellbrook.events")
  System_Ext(idp, "Identity provider", "OpenID Connect")
  Rel(gateway, api, "Calls", "HTTP/JSON over mesh mTLS, bearer token with orders:read / orders:write")
  Rel(api, db, "Reads and writes", "EF Core / Npgsql")
  Rel(api, broker, "Publishes orders.*.v1 from the outbox", "AMQPS")
  Rel(api, idp, "Fetches token signing keys", "HTTPS")
```

## Inside the service

| Project | Responsibility | Depends on |
|---|---|---|
| `Quellbrook.Orders.Domain` | the Order aggregate, its value objects and domain events | nothing |
| `Quellbrook.Orders.Contracts` | the published integration-event contracts (`OrderPlacedV1`) | nothing |
| `Quellbrook.Orders.Application` | command handlers, queries, the contract mapping | Domain, Contracts |
| `Quellbrook.Orders.Infrastructure` | EF Core persistence and migrations, the outbox and its relay, the RabbitMQ publisher | Application |
| `Quellbrook.Orders.Api` | minimal-API endpoints, authentication and authorization, hosting | Infrastructure |

The aggregate knows nothing about storage: `EfOrderRepository` maps it to an `orders` row (consignee and parcels as
JSON documents, the list's filter and sort fields as columns). When it saves, it also writes one `outbox_messages`
row per domain event the aggregate raised, in the same transaction; `OutboxRelay`, a background service in the same
process, publishes pending rows in order and marks them dispatched (ADR 0003).

```mermaid
sequenceDiagram
  participant G as Gateway
  participant A as Orders API
  participant D as PostgreSQL
  participant R as Outbox relay
  participant B as RabbitMQ
  G->>A: POST /orders
  A->>D: INSERT orders + outbox_messages (one transaction)
  A-->>G: 201 Created
  loop every 2 s
    R->>D: SELECT undispatched ORDER BY occurred_at
    R->>B: publish (message-id = outbox id), wait for confirm
    R->>D: UPDATE dispatched_at
  end
```

## Message contracts

| Routing key | Payload | Consumers |
|---|---|---|
| `orders.order-placed.v1` | `OrderPlacedV1`: order id, customer account, service level, consignee (name, address, optional e-mail and phone), parcels | dispatch, notifier |
| `orders.order-cancelled.v1` | `OrderCancelledV1`: order id, reason, time | dispatch |

Messages are JSON, persistent, with the AMQP `message-id` set; consumers de-duplicate on it. A v1 contract only gains
optional fields; a breaking change is a new routing key (`.v2`) published alongside v1 until every consumer moved.

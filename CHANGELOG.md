# Changelog

All notable changes to this project are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses [Semantic Versioning](https://semver.org/).

## [0.3.0] - 2026-09-04

### Added
- Filtering the order list by status.
- `Idempotency-Key` on `POST /orders`: a retried submission returns the order the first one placed.
- JSON Schemas and an AsyncAPI document for the published events; ADR 0004 on how they evolve.
- `docs/privacy.md`: the personal data the service holds and for how long.

### Changed
- E-mail addresses and phone numbers are validated; phone numbers are stored in international form.
- Dispatched outbox messages are deleted after seven days.

## [0.2.0] - 2026-08-21

### Added
- Cancelling an order (`POST /orders/{id}/cancellation`) and `orders.order-cancelled.v1`.

### Changed
- Events are published through a transactional outbox (ADR 0003): an order and its event are committed together, and
  a relay publishes them. Previously an event could be lost when the broker was unavailable after the order was saved.

## [0.1.0] - 2026-08-07

### Added
- Placing, reading and listing orders over the HTTP API, with scoped bearer tokens.
- `orders.order-placed.v1` published to the `quellbrook.events` exchange.
- PostgreSQL storage through EF Core with migrations; health checks; OpenTelemetry.
- Container image, Kubernetes manifests, CI, CodeQL, release and deploy workflows.

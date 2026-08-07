# Changelog

All notable changes to this project are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses [Semantic Versioning](https://semver.org/).

## [0.1.0] - 2026-08-07

### Added
- Placing, reading and listing orders over the HTTP API, with scoped bearer tokens.
- `orders.order-placed.v1` published to the `quellbrook.events` exchange.
- PostgreSQL storage through EF Core with migrations; health checks; OpenTelemetry.
- Container image, Kubernetes manifests, CI, CodeQL, release and deploy workflows.

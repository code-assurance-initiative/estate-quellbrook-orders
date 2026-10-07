# Scripted history

26 commits by fictional authors, rebuilt and verified by `build-history.sh` (see its header). Commits after them are real benchmark maintenance.

## Sprint 1 (2026-07-27 – 2026-08-07, release `v0.1.0`)

| Commit | Date | Author | Subject | Note |
|---|---|---|---|---|
| `b449bc9` | 2026-07-27 | Ruth Calloway | chore: solution skeleton with central package management and lock files |  |
| `0b29443` | 2026-07-27 | Ruth Calloway | docs: ADR 0001 (record decisions) and ADR 0002 (layered domain model) |  |
| `2921ae6` | 2026-07-28 | Pavel Strand | feat(domain): Order aggregate with consignee, parcels and service levels |  |
| `0b26b69` | 2026-07-29 | Ruth Calloway | feat(persistence): store orders in PostgreSQL through EF Core |  |
| `f8a2a0f` | 2026-07-30 | Pavel Strand | feat: place orders and publish orders.order-placed.v1 | sprint 1 publishes the event straight after the database commit (a dual write) |
| `bdc66ea` | 2026-07-31 | Pavel Strand | feat(api): order endpoints with scoped bearer authentication |  |
| `ca6c83b` | 2026-08-04 | Odile Marchetti | build: container image and Kubernetes manifests |  |
| `9bf1217` | 2026-08-05 | Ruth Calloway | ci: build, test, CodeQL, release and deploy workflows |  |
| `df72416` | 2026-08-06 | Odile Marchetti | docs: README, architecture with C4 diagrams, API contract and catalog entry |  |
| `9f4cb57` | 2026-08-07 | Ruth Calloway | release 0.1.0 | tag `v0.1.0` |

## Sprint 2 (2026-08-10 – 2026-08-21, release `v0.2.0`)

| Commit | Date | Author | Subject | Note |
|---|---|---|---|---|
| `9a85f35` | 2026-08-10 | Ruth Calloway | docs: ADR 0003 transactional outbox | why the outbox: orders saved but never announced |
| `00d8d41` | 2026-08-12 | Pavel Strand | feat(domain): cancel an order with a reason |  |
| `8eb2083` | 2026-08-13 | Ruth Calloway | refactor: publish order events through a transactional outbox (ADR 0003) | the improvement: transactional outbox |
| `9c642c3` | 2026-08-17 | Odile Marchetti | feat: cancel orders through the API and announce orders.order-cancelled.v1 |  |
| `c98d5fb` | 2026-08-19 | Odile Marchetti | docs: outbox and cancellation in the README, architecture and API contract |  |
| `e91f36a` | 2026-08-21 | Ruth Calloway | release 0.2.0 | tag `v0.2.0` |

## Sprint 3 (2026-08-24 – 2026-09-04, release `v0.3.0`)

| Commit | Date | Author | Subject | Note |
|---|---|---|---|---|
| `07ba7f4` | 2026-08-24 | Ruth Calloway | feat(outbox): delete dispatched messages after seven days |  |
| `5a7c6d3` | 2026-08-25 | Odile Marchetti | feat(domain): validate e-mail addresses and store phone numbers in international form |  |
| `67a28aa` | 2026-08-25 | Ruth Calloway | docs: ADR 0004 versioned, consumer-tolerant event contracts |  |
| `207ff0d` | 2026-08-26 | Pavel Strand | feat: filter the order list by status |  |
| `4041518` | 2026-08-28 | Odile Marchetti | docs: JSON Schemas and AsyncAPI document for the order events |  |
| `b6d9575` | 2026-09-01 | Ruth Calloway | docs: personal data held by the service and its retention |  |
| `6a10da2` | 2026-09-01 | Pavel Strand | refactor(domain): DomainException with the one constructor it needs |  |
| `54343fd` | 2026-09-02 | Pavel Strand | feat(api): Idempotency-Key for placing orders; one path for operator-authored commands | idempotent order placement |
| `e85ab7c` | 2026-09-03 | Ruth Calloway | test(messaging): connection reuse and reconnection |  |
| `fedfd1a` | 2026-09-04 | Ruth Calloway | release 0.3.0 | tag `v0.3.0` |

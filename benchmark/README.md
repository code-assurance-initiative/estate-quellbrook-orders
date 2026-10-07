# Benchmark: estate-quellbrook-orders

**Theme.** Quellbrook Freight reference estate, order service: a C# DDD service (Order aggregate, domain events, transactional outbox to a message broker) with three sprints of scripted history in which the team replaces a dual write by an outbox and keeps improving.

This repository is part of the scanner benchmark of the Code Assurance Initiative (`code-assurance-initiative/scanner-benchmark`). Its answer key, `benchmark/answer-key.json`, labels the defects that are really there (`must-fire`), sites that look like defects and are not (`must-not-fire`), code certified clean, concepts that do not apply, and expected score ranges. The key is at integration level: it labels the repository as a whole service, at the final commit, plus the one finding that lives only in history. `benchmark/journal.md` records how the repository was made and every scan judged on the way.

## The estate

Quellbrook Freight is a fictional parcel and freight carrier (the name was checked against companies, products and
trademarks; see `scanner-benchmark/docs/ESTATE-NAME.md`). Its operations platform is five repositories, owned by four
teams, that together form one system:

| Repository | What it is | Team | Trajectory over the three sprints |
|---|---|---|---|
| `estate-quellbrook-gateway` | TypeScript API gateway / BFF (Node 22, Fastify): operator authentication, routing, shipment view | Edge | steady |
| `estate-quellbrook-web` | TypeScript React + Vite operator console served by nginx | Edge | steady |
| `estate-quellbrook-orders` | C# order service: Order aggregate, domain events, transactional outbox | Orders | improving |
| `estate-quellbrook-dispatch` | C# dispatch service: consignments, routes, drivers, vehicles | Fleet | falling in sprint 2, partly repaired in sprint 3 |
| `estate-quellbrook-notifier` | C# worker sending e-mail and SMS through abstracted providers | Customer Comms | security incident introduced (sprint 2) and fixed (sprint 3) |

Operators use the web console, which calls the gateway; the gateway calls the order and dispatch services over HTTP
with its own service identity. The order service publishes `orders.*` events to a RabbitMQ topic exchange; dispatch
consumes them and publishes `dispatch.*` events; the notifier consumes both and notifies consignees. Every repository
owns its contracts: producers publish JSON Schemas and an AsyncAPI document under `contracts/`, consumers keep a
pinned copy of the versions they consume under `contracts/consumed/`. Each repository has its own Dockerfile,
Kubernetes manifests (`deploy/k8s/`), GitHub Actions workflows pinned by commit SHA, `docs/architecture.md` with C4
diagrams, ADRs and a `catalog-info.yaml`.

The sprints are two weeks long and identical across the estate: **sprint 1** 2026-07-27 – 2026-08-07 (release
`v0.1.0`), **sprint 2** 2026-08-10 – 2026-08-21 (`v0.2.0`), **sprint 3** 2026-08-24 – 2026-09-04 (`v0.3.0`). The
history is scripted and reproducible (`benchmark/history/`); its authors are fictional (`@example.invalid`).

## The sprint story (for a film director)

The Orders team (Ruth Calloway, Pavel Strand, Odile Marchetti) is the estate's **improving** team.

- **Sprint 1 (v0.1.0).** The service is created: placing, reading and listing orders over an HTTP API secured with
  scoped bearer tokens, PostgreSQL through EF Core, health checks, container image, Kubernetes manifests and CI. The
  `OrderPlaced` event is published to the broker straight after the database commit — a dual write: an order can be
  saved and its event lost.
- **Sprint 2 (v0.2.0).** The team writes ADR 0003 and replaces the dual write by a **transactional outbox**: the event
  is stored in the same transaction as the order and a relay publishes it. Cancelling an order (`OrderCancelled`)
  ships on top of the outbox.
- **Sprint 3 (v0.3.0).** Outbox retention, contact-detail validation, filtering the order list by status, the
  published JSON Schemas and AsyncAPI document for the events, and a privacy note. Tests grow every sprint.

What a scanner should see at the end: a healthy service with two small leftovers (ORD-001, ORD-002), and a history in
which quality rises. The commits that tell the story are listed in `benchmark/history/README.md`.

## Planted defects (`must-fire`)

| Id | Concept | Site | Why |
|---|---|---|---|
| ORD-001 | `publicly-mutable-entity-state` | `src/Quellbrook.Orders.Domain/Orders/Parcel.cs:15-17` | Parcel is an entity inside the Order aggregate, yet its weight and dimensions have public setters: any code holding an order can change a parcel after the order was placed, bypassing the aggregate's weight and size limits (Order.Place validates them once) and raising no domain event, so the published OrderPlaced contract and the stored order can disagree. |
| ORD-002 | `missing-cancellation-propagation` | `src/Quellbrook.Orders.Infrastructure/Persistence/OrderQueries.cs:9-35` | The order-list query performs two database round-trips (the count and the page) and takes no CancellationToken, so the endpoint cannot hand it the request's: a client that disconnects, or a gateway timeout, leaves the query running against PostgreSQL to completion. Every other async path in the service accepts and forwards a token. |

## Traps (`must-not-fire`)

| Id | Concept | Site | Why |
|---|---|---|---|
| TRP-001 | `dual-write-without-outbox` | `src/Quellbrook.Orders.Infrastructure/Outbox/OutboxRelay.cs:23-56` | The outbox relay publishes a stored message to the broker and then marks the row dispatched in a second write. That is the transactional-outbox pattern's relay, not a dual write: the business change and the message were committed together earlier; a crash between publish and mark re-publishes the same message id, which consumers de-duplicate. |
| TRP-002 | `event-not-named-in-past-tense` | `src/Quellbrook.Orders.Api/Contracts/PlaceOrderRequest.cs:5` | PlaceOrderRequest is the HTTP request body of POST /orders, not an event; an imperative name is correct for a request. |
| TRP-003 | `event-not-named-in-past-tense` | `src/Quellbrook.Orders.Application/PlaceOrder/PlaceOrderCommand.cs:3` | PlaceOrderCommand is a command handled by exactly one handler; commands are named in the imperative. |
| TRP-004 | `personal-data-in-event-store` | `src/Quellbrook.Orders.Contracts/IntegrationEvents/OrderPlacedV1.cs:7-24` | OrderPlacedV1 carries the consignee's name, address and optional contact details because dispatch and the notifier need them to deliver and to notify. It is an integration message, not a persisted event of an event store: the service stores state, not events, and the outbox row that carries it is deleted seven days after it was dispatched (OutboxRetention), so the personal data in it can be erased. |
| TRP-005 | `primitive-entity-identifier` | `src/Quellbrook.Orders.Contracts/IntegrationEvents/OrderPlacedV1.cs:8` | The published contract uses a plain Guid for the order id on purpose: a wire contract shared with other teams carries primitive types, and the domain's strongly typed OrderId is mapped at the boundary. |
| TRP-006 | `hardcoded-credential` | `deploy/k8s/deployment.yaml:55-70` | The database connection string and the broker credentials are read from Kubernetes Secrets (secretKeyRef, materialised by an ExternalSecret); nothing secret is written in the manifest. |
| TRP-007 | `suppressed-diagnostic` | `.editorconfig:36-37` | CA2007 (ConfigureAwait) is switched off at the repository root with its reason on the line above: tests must not call ConfigureAwait(false) (xUnit1030), and src/.editorconfig switches it back on as a warning for all production code. A scoped, documented suppression is not hidden debt. |
| TRP-009 | `solution-structure` | `(repository)` | Quellbrook.Orders.Contracts is a small project on purpose: it holds only the published event contracts, so they can be versioned on their own and can never reference the domain (ADR 0002, ADR 0004). A thin contract assembly is the intended structure, not a project to consolidate. Repository-level: a scanner reports the solution's shape without a site. |
| TRP-010 | `missing-image-healthcheck` | `src/Quellbrook.Orders.Api/Dockerfile` | The image runs only on Kubernetes, which ignores a Dockerfile HEALTHCHECK; liveness and readiness probes are declared in deploy/k8s/deployment.yaml (the Dockerfile says so in its header). |
| TRP-011 | `compiled-code-size` | `src/Quellbrook.Orders.Infrastructure/Persistence/OrderRecordConfiguration.cs:8-10` | OrderRecordConfiguration.Configure is EF Core's fluent mapping of one table: a flat sequence of declarative calls with no branches. Its IL size grows with the number of columns, not with any logic a reader has to follow. |
| TRP-012 | `primitive-entity-identifier` | `src/Quellbrook.Orders.Domain/Orders/Order.cs:57` | Order.RequestKey is the caller's opaque idempotency key for the request that placed the order, not an identifier of the order or of any domain object; nothing references an order by it except the duplicate check. Wrapping a pass-through string in a strongly typed id would add nothing. |
| TRP-008 | `cleartext-transmission` | `src/Quellbrook.Orders.Api/appsettings.Development.json:9` | appsettings.Development.json points the broker at amqp://localhost for a developer's local RabbitMQ container; production configuration (appsettings.json) uses amqps. Plain AMQP to the loopback interface crosses no network. |

## Certified clean

173 `clean` entries, one per tracked file: files without a label are certified clean for every concept (`"*"`); a file that carries a plant or a trap is certified clean for every finding concept except the labelled ones and the concepts a result of those labels would restate.

## Not applicable

- `missing-text-alternative` — The orders service is an HTTP API with a message relay; it has no user interface: there are no images.
- `form-control-without-label` — The orders service is an HTTP API with a message relay; it has no user interface: there are no form controls.
- `page-structure-violation` — The orders service is an HTTP API with a message relay; it has no user interface: there are no pages.
- `non-keyboard-accessible-interaction` — The orders service is an HTTP API with a message relay; it has no user interface: there are no interactive markup.
- `invalid-aria-usage` — The orders service is an HTTP API with a message relay; it has no user interface: there are no ARIA attributes.
- `visual-and-motion-safety` — The orders service is an HTTP API with a message relay; it has no user interface: there are no styles or animation.
- `accessibility-checks-in-ci` — The orders service is an HTTP API with a message relay; it has no user interface: there are no user interface to check.
- `alt-text-quality` — The orders service is an HTTP API with a message relay; it has no user interface: there are no images.
- `link-and-button-text-quality` — The orders service is an HTTP API with a message relay; it has no user interface: there are no links or buttons.
- `heading-and-label-text-quality` — The orders service is an HTTP API with a message relay; it has no user interface: there are no headings or labels.
- `cross-site-scripting` — The API returns JSON only (problem+json for errors); it renders no HTML.
- `sensitive-data-in-browser-storage` — No browser code.
- `nondeterministic-event-fold` — The service stores state, not events: no event-sourced aggregate, no fold.
- `mutable-persisted-event` — The service stores state, not events: there is no event store.
- `https-enforcement` — An internal API reachable only from the gateway's namespace, behind the cluster's service mesh, which encrypts and authenticates every connection with mutual TLS; the pod listens on plain HTTP to its sidecar by design. HTTPS redirection would break the gateway's calls and HSTS is a browser mechanism. Transport security is enforced, by the platform, not the process.

## Score bands

Bands were set from the intent of the code, before any scan, and are wide where a model judges.

| Id | Concept | Band | Why |
|---|---|---|---|
| BND-001 | `high-cyclomatic-complexity` | 80–100 | Small handlers and aggregates; no method above the usual threshold. |
| BND-002 | `high-cognitive-complexity` | 80–100 | Small handlers and aggregates; flat control flow. |
| BND-003 | `duplicated-code` | 80–100 | No copied blocks of note. |
| BND-004 | `test-coverage` | 70–100 | Unit tests for the domain and application layers, integration tests over the HTTP API and persistence. |
| BND-005 | `test-pyramid-distribution` | 50–100 | More unit than integration tests. |
| BND-006 | `dependencies-not-locked` | 80–100 | Central package management with committed lock files, restored in locked mode in CI. |
| BND-007 | `security-tooling-in-ci` | 60–100 | CodeQL on every pull request plus a vulnerable-package gate in CI. |
| BND-008 | `vulnerability-disclosure-policy` | 80–100 | SECURITY.md with a private reporting channel and response times. |
| BND-009 | `readme-quality` | 70–100 | README with purpose, build/run, configuration, testing, architecture and ownership sections. |
| BND-010 | `architecture-documentation` | 70–100 | docs/architecture.md with C4 diagrams in Mermaid, plus ADRs. |
| BND-011 | `folder-structure` | 80–100 | src/ and tests/ separation, one root namespace prefix. |
| BND-012 | `ci-build-and-test-pipeline` | 80–100 | CI builds and tests every push and pull request. |
| BND-013 | `observability` | 60–100 | Structured logging through ILogger, OpenTelemetry traces and metrics, health checks. |
| BND-014 | `deployment-rollback-safety` | 50–100 | Rolling updates by image digest with a rollback step in the deploy workflow. |
| BND-015 | `release-hygiene` | 40–100 | CHANGELOG with every release, <Version> 0.3.0 and tags v0.1.0..v0.3.0. |
| BND-016 | `build-provenance-and-signing` | 40–100 | Release images built by CI with provenance and SBOM attestations. |
| BND-017 | `network-egress-policy` | 60–100 | Default-deny NetworkPolicies with one allowance per real flow. |
| BND-018 | `workload-syscall-confinement` | 10–80 | seccomp RuntimeDefault, all capabilities dropped; no AppArmor or custom profiles. |
| BND-019 | `runtime-threat-detection-and-admission` | 0–40 | Pod Security Admission 'restricted' on the namespace; no runtime threat detection or signed-image admission. |
| BND-020 | `churn-complexity-hotspot` | 70–100 | Six weeks of history; churn lands on small files. |
| BND-021 | `knowledge-concentration` | 30–100 | A team of two or three authors over six weeks; some files are naturally single-author. |
| BND-022 | `knowledge-freshness` | 70–100 | The whole history is recent and the authors are active. |
| BND-023 | `change-coupling` | 50–100 | Co-changes are explicit dependencies or tests changing with their subject. |
| BND-024 | `documentation-quality` | 60–100 | Model-judged. README, architecture overview and ADRs are clear and current. Wide band. |
| BND-025 | `adr-quality` | 60–100 | Model-judged. ADRs carry status, context, decision and two-sided consequences. Wide band. |
| BND-026 | `adr-conformance` | 60–100 | Model-judged. The code follows its ADRs. Wide band. |
| BND-027 | `documentation-accuracy` | 50–100 | Model-judged. README and architecture describe the code as it is. Wide band. |
| BND-028 | `inconsistent-naming` | 60–100 | Model-judged. Consistent domain vocabulary. Wide band. |
| BND-029 | `low-value-comments` | 60–100 | Model-judged. Comments explain why, not what. Wide band. |
| BND-030 | `internal-api-inconsistency` | 60–100 | Model-judged. Endpoints and handlers follow one shape. Wide band. |
| BND-031 | `security-response-headers` | 70–100 | CSP, X-Content-Type-Options, frame, referrer and resource policy on every response. |
| BND-032 | `authorization-enforcement` | 70–100 | Every endpoint but health requires a named scope policy. |
| BND-033 | `inbound-input-validation` | 60–100 | Request bodies validated before they reach the domain; the domain re-checks its invariants. |
| BND-034 | `versioned-schema-migrations` | 80–100 | EF Core migrations, one per schema change, applied by a migrations bundle. |
| BND-035 | `data-retention-policy` | 0–60 | Outbox rows are purged seven days after dispatch, but orders (with consignee data) have no enforced retention yet; docs/privacy.md says so. |
| BND-036 | `audit-trail` | 10–80 | Orders record the operator who placed or cancelled them; no general audit log. |
| BND-037 | `data-subject-rights` | 0–50 | No erasure or export operation for consignee data. |
| BND-038 | `data-encryption-controls` | 0–70 | TLS to the database and broker and encryption at rest by the platform; no field-level encryption. |

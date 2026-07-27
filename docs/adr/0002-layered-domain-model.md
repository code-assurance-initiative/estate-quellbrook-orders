# 2. Layered projects around a persistence-ignorant domain model

- Status: Accepted
- Date: 2026-07-27
- Deciders: Ruth Calloway, Pavel Strand

## Context

Orders carry real business rules (parcel limits per service level, weights, delivery countries, who may cancel and
when) and will grow. Other teams depend on what an order means, so the rules must live in one place and be testable
without a database or HTTP.

## Decision

Four projects with dependencies pointing inward: Domain (the Order aggregate, value objects, domain events; no
framework references), Application (command handlers and queries), Infrastructure (EF Core persistence, the broker
client) and Api (endpoints and hosting). The aggregate enforces its invariants and is stored through a repository
that maps it to rows; it is not an EF Core entity. Integration contracts live in their own project and are mapped
from the aggregate in the application layer.

## Consequences

- Positive: domain rules are unit-tested in isolation; persistence can change without touching them; the published
  contract cannot leak domain types.
- Negative: an explicit mapping layer to maintain (aggregate to row, aggregate to contract); more projects than a
  single-project service would need.

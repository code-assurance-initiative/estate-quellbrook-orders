# 4. Versioned, consumer-tolerant event contracts

- Status: Accepted
- Date: 2026-08-25
- Deciders: Ruth Calloway, Odile Marchetti, Kasper Nyholt (Fleet), Lucia Brennan (Customer Comms)

## Context

Dispatch and the notifier consume our events and keep their own copies of the payload types. There is no shared
code library between the teams, and we do not want one: it would couple every release to every consumer. We still
need a written contract that consumers can pin and that tells us what we may change.

## Decision

Each event type has a routing key with a major version (`orders.order-placed.v1`) and a JSON Schema in
`contracts/events/`, listed in `contracts/asyncapi.yaml`. Within a major version we only add optional fields;
consumers ignore fields they do not know. A breaking change gets a new routing key (`.v2`), published alongside the
old one until every consumer has moved; we announce it to the consuming teams through an ADR. Consumers keep a
pinned copy of the schema they implement under their own `contracts/consumed/`.

## Consequences

- Positive: teams release independently; a consumer can check its copy against ours in review; breaking changes are
  explicit and rare.
- Negative: optional-only evolution makes some changes awkward (a field that should be required stays optional);
  running two versions side by side costs broker traffic and code for a while.

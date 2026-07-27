# 1. Record architecture decisions

- Status: Accepted
- Date: 2026-07-27
- Deciders: Ruth Calloway, Pavel Strand, Odile Marchetti

## Context

The order service is new and owned by one team, but its decisions affect the gateway, dispatch and notifier teams.
Decisions made in conversation are lost when people move on.

## Decision

We record every decision that shapes the service's structure, contracts or operation as an architecture decision
record in `docs/adr`, numbered, with status, context, decision and consequences. A decision that replaces an earlier
one supersedes it explicitly.

## Consequences

- Positive: newcomers and other teams can read why the service looks the way it does; reviews can point at a record.
- Negative: writing a record takes time, and records go stale if superseded decisions are not marked.

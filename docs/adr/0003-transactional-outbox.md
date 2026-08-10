# 3. Publish events through a transactional outbox

- Status: Accepted
- Date: 2026-08-10
- Deciders: Ruth Calloway, Pavel Strand, Odile Marchetti

## Context

Since 0.1.0 the place-order handler saves the order and then publishes `orders.order-placed.v1` to the broker: two
writes to two systems with nothing tying them together. On 2026-08-06 a broker restart during a load test left
eleven orders stored and never announced; dispatch never planned them. The reverse (an event for an order that was
rolled back) is possible too. More events are coming (cancellation in this sprint), so the gap would widen.

## Decision

Events are written to an `outbox_messages` table in the same database transaction as the aggregate change that
raised them; the repository does this when it saves. A background relay in the same process reads undispatched
messages in order, publishes each with its stored message id and marks it dispatched. Delivery is at least once:
a crash between publishing and marking re-publishes the message under the same id, and consumers de-duplicate on
the AMQP `message-id` (the dispatch and notifier teams already keep inboxes).

## Consequences

- Positive: an order and its event are committed together or not at all; publishing survives broker outages (the
  relay retries); handlers no longer know about the broker.
- Negative: events reach the broker with a delay (the relay polls every two seconds); with two replicas both relays
  may publish the same message, which is safe but doubles broker traffic for those messages; the table grows and
  needs a retention job.

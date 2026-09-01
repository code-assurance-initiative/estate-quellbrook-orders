# Personal data in the order service

| Data | Why it is held | Where | Retention |
|---|---|---|---|
| Consignee name and delivery address | needed to deliver the parcels | `orders` row (JSON column), `orders.order-placed.v1` | with the order (see below) |
| Consignee e-mail and phone (optional) | needed only to notify the consignee about this delivery | as above | as above |
| Operator id of who placed or cancelled the order | accountability for changes made on a shipper's behalf | `orders` row | with the order |

Outbox rows carry the same data as the events they hold. They are deleted seven days after they were published
(`Outbox:Retention`).

Orders are kept for 13 months after they were placed, for invoicing and claims. **The job that anonymises the
consignee of older orders is not built yet** (backlog item ORD-112); until it is, orders are kept indefinitely.
There is no self-service export or erasure: requests from consignees go to customer service, who handle them by hand
with the Orders team.

Data is encrypted in transit (mesh mutual TLS, TLS to PostgreSQL and RabbitMQ) and at rest by the managed database
and broker; the service adds no field-level encryption.

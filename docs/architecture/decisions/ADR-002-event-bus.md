# ADR-002: Event bus technology

## Status

**Proposed.** No decision has been made. Nothing in this document is an accepted requirement.

## Context

The project documentation has described asynchronous communication between services as a goal, but today there is none:

- `RedisEventBus` (Redis Pub/Sub) exists in Medical Appointments but is not registered in dependency injection, and no active code path calls it.
- Domain events are raised by the aggregates but are never dispatched or published.
- `docker-compose.yml` provisions RabbitMQ, which no code uses. Earlier documentation listed RabbitMQ as the broker.
- There are no event consumers. The first realistic consumer is Notifications reacting to appointment events (created, confirmed, cancelled).
- Duplicate abstractions exist in `src/BuildingBlocks/EventBus` and `src/Shared/Events`.

## Decision

Pending. Until a decision is accepted, do not wire up an event bus and do not document one as in use.

## Alternatives Considered

- **Redis Pub/Sub** (what the code uses). Already a dependency (cache). Fire-and-forget: messages are lost if a subscriber is down, and there are no acknowledgements or retries. Acceptable for best-effort signals, not for notifications that must be delivered.
- **RabbitMQ.** Durable queues, acknowledgements, retries and dead-lettering. Already provisioned in the compose file. Adds an infrastructure component and a client library, and the existing `RedisEventBus` would be replaced.
- **Redis Streams.** Persistent, supports consumer groups and acknowledgements, and reuses the Redis dependency. Less operational tooling than RabbitMQ.
- **Do not introduce a bus yet.** Services call each other over HTTP or the Notifications service is driven by a direct call until a second consumer exists (YAGNI).

## Consequences

Not yet applicable. For whichever option is accepted, the following must also be decided and documented: an outbox (so that saving the aggregate and publishing the event cannot diverge), delivery guarantees and idempotent consumers, event schema versioning, and removal of the duplicate event abstractions and of whichever infrastructure goes unused.

A suggestion for the owner to confirm: if Notifications must reliably deliver messages triggered by appointment events, prefer a durable option (RabbitMQ or Redis Streams) over Pub/Sub, and keep the choice behind the existing `IEventBus` abstraction.

## References

- `src/Services/MedicalAppointments/MedicalAppointments.Infrastructure/EventBus/RedisEventBus.cs`
- `src/BuildingBlocks/EventBus/`, `src/Shared/Events/`
- `docker-compose.yml` (RabbitMQ service)

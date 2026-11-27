# Spectrum PEAK Integration — Architecture

## 1. Purpose

PEAK is a game-specific adapter inside the existing Spectrum game integration ecosystem. It is not a standalone Twitch integration.

The implementation must preserve existing system boundaries and reuse established session, pairing, delivery, and acknowledgement patterns from the Stardew Valley and R.E.P.O. projects.

## 2. System boundaries

### Spectrum Control Center — management plane

Owns:
- Canonical Twitch identity and authorisation.
- Spectrum member activation and access permissions.
- Pairing approval and revocation.
- PEAK-specific configuration and reward mappings.
- Mod version, connection, lobby, and health status.
- Administrative enable/disable controls.

Does not:
- Execute PEAK gameplay.
- Create or synchronise PEAK items.
- Process charity donation logic.

### Spectrum Event Hub — Twitch event plane

Owns:
- Existing central EventSub ingress and signature validation.
- Twitch channel point redemption ingestion.
- Event identity and duplicate suppression at the event boundary.
- Routing approved redemptions to the correct broadcaster-scoped PEAK session.

Does not:
- Know PEAK item classes or prefabs.
- Choose game objects.
- Execute gameplay.
- Become a donation event processor.

### Charity worker/app — donation plane

Owns:
- Donation provider integration and validation.
- Donation metadata and event creation.
- Donation-specific rules and configuration.
- Publishing an authenticated donation event to the PEAK integration path.

Do not route donation events through Twitch EventSub or duplicate charity provider credentials in the mod.

### Shared Spectrum game integration contract

The existing Control Center / gateway pattern should provide:
- Pairing and revocable game credentials.
- Authenticated session registration.
- Broadcaster-scoped event routing.
- Typed event/action envelopes.
- Request expiry and deduplication.
- Delivery state and acknowledgements.
- Reconnect handling and bounded queues.
- Version/capability reporting.

Prefer extending the existing common service rather than creating a PEAK-only backend. Any new route or schema must be reviewed against Stardew and R.E.P.O. compatibility before implementation.

### Spectrum PEAK plugin — game adapter

Owns:
- BepInEx lifecycle and configuration.
- PEAK lobby/session observation.
- PEAK host and player identity resolution.
- Mapping authenticated Spectrum identities to live PEAK players.
- Safe, explicit item catalogue.
- Validation of target player and item ID.
- Main-thread scheduling of game operations.
- Host-authoritative execution using PEAK's native network path.
- Result reporting and diagnostic logging.

Does not:
- Perform Twitch OAuth.
- Store Twitch access/refresh tokens.
- Connect directly to Twitch EventSub.
- Process donation provider webhooks.
- Trust client-supplied arbitrary game type names or prefab paths.
- Replace PEAK's multiplayer synchronisation.

## 3. Intended event flows

These flows are design targets. The exact PEAK peer-forwarding mechanism remains unverified until game assemblies and a multiplayer test are available.

### Channel point redemption — target one player

1. A viewer redeems a configured reward on a Spectrum broadcaster's Twitch channel.
2. Event Hub validates the EventSub event and routes it to that broadcaster's authenticated game session.
3. The paired PEAK client receives a typed redemption command.
4. The client resolves its current PEAK lobby and local player identity.
5. If it is not the authoritative host, it forwards a typed request to the host over PEAK's supported multiplayer messaging path.
6. The host validates the sender against the current lobby roster and resolves the target player.
7. The host validates the configured stable item ID against its local allowlist.
8. The host executes the approved item creation through the game's supported network-aware API.
9. The host confirms the result to the originating client.
10. The originating client acknowledges delivery to the Spectrum gateway.

If the broadcaster's player is not connected, the request must expire or be rejected according to the configured policy. It must not silently target a different player.

### Charity donation — one item per connected player

1. The charity system validates a donation and creates a stable donation event ID.
2. The charity system publishes the event through its dedicated integration path.
3. The integration resolves the active PEAK lobby/lobbies eligible for that event.
4. Each eligible connected player is represented by one unique delivery target.
5. The host coordinates one approved spawn per eligible player, or an equivalent native-network-safe execution path.
6. The host tracks completion independently for each target.
7. Disconnected or unresolved players are excluded; they are not substituted.
8. The donation event is complete only when each eligible target has a terminal result or the event expires.

The implementation must not accidentally produce one item per client copy of the same event. A single authoritative coordinator and stable per-target delivery IDs are required.

## 4. Identity and player mapping

Do not match players by display name alone.

The design needs a binding between:
- The authenticated Spectrum broadcaster identity.
- The paired mod installation/session credential.
- The PEAK network player identity in the current lobby.

The exact PEAK identifier must be selected after inspecting the current game assemblies. Prefer a stable network/account identifier that the host can independently verify against the live lobby roster.

The host must validate that:
- The forwarding peer is currently connected.
- The claimed player belongs to that peer.
- The player is still in the current lobby/session.
- The event has not already been processed.
- The target is eligible for the requested action.

Never accept a client-supplied arbitrary player object reference, network view ID, prefab path, or item class without server-side resolution and validation.

## 5. Host authority and native multiplayer

Preferred model:
- Only the current PEAK authority/host executes shared-world item creation.
- Clients send small typed requests to the host.
- The host resolves targets from the authoritative lobby roster.
- The host uses PEAK's own network-aware object creation path.
- All clients observe the same networked object through PEAK's normal replication.

Do not instantiate a local-only copy on every client. Do not build a parallel multiplayer transport if PEAK already provides a suitable messaging mechanism.

Host migration must be treated as a session boundary or explicit authority transfer. Pending events must not be replayed by both old and new hosts.

## 6. Typed event contract

Use stable IDs and a versioned envelope. Final property names must match the existing Spectrum gateway contract; this is a conceptual contract, not a new schema to deploy unilaterally.

Common envelope fields:
- `schema_version`
- `event_id` or `action_id`
- `game` = `peak`
- `event_type`
- `created_at`
- `expires_at`
- `broadcaster_user_id` where applicable
- `session_id`
- `payload`

Gameplay payload should contain only:
- A stable allowlisted item ID.
- A resolved target identity or target reference that the host validates.
- A delivery ID.
- A bounded count fixed by the approved configuration.
- Optional safe display metadata.

It must not contain arbitrary class names, assembly-qualified names, constructor arguments, asset paths, or executable instructions.

## 7. Delivery state machine

Recommended states:
- `received`
- `validated`
- `queued_for_host`
- `executing`
- `succeeded`
- `rejected`
- `expired`
- `failed_retryable`

Each event and each charity target needs a stable idempotency key. The host keeps a bounded in-memory processed-ID cache for the active lobby/session. The gateway owns durable event-level deduplication where it already does so.

Acknowledgement rules:
- A network send is not a successful gameplay result.
- A client acknowledgement is not a host execution result.
- The origin client acknowledges the gateway only after the host returns a terminal result.
- Retries reuse the same delivery ID.
- A duplicate delivery ID returns the prior result and never executes again.

Do not claim exactly-once execution across crashes unless a durable transaction or reconciliation mechanism actually guarantees it. The practical goal is idempotent at-most-once execution within a live session, with explicit expiry and recovery semantics.

## 8. Item catalogue and safety

Use an explicit, local allowlist with stable IDs and validation. The first release catalogue should be limited to benign, non-combat utility/cosmetic/gameplay items approved for this integration.

The gateway sends a stable item ID, never a PEAK class name or prefab path. The plugin maps that ID to a locally verified definition and validates it again on the host before execution.

Do not automatically approve every loaded asset. Modded content is out of scope for the first milestone unless its network registration, ownership, and compatibility are explicitly verified.

PEAKLib's Items and Core/network-prefab APIs are candidates to evaluate. Do not add PEAKLib merely because it exists; first determine whether PEAK's native APIs already provide the required functionality.

## 9. Threading and lifecycle

- Network callbacks must parse, validate, and enqueue only.
- All Unity/PEAK game-state reads and mutations must occur on the appropriate Unity/game thread.
- Bound message sizes, queue lengths, retry counts, and event lifetime.
- Cancel background work on plugin shutdown.
- Clear lobby-scoped state on disconnect, lobby change, scene transition, and host migration.
- Do not persist transient lobby queues across process restarts unless a deliberate recovery protocol is designed.

## 10. Security and privacy

- No Twitch secrets or provider credentials in the plugin.
- Use short-lived pairing codes and revocable, scoped game credentials.
- Store only the minimum local credential material needed to reconnect.
- Redact credentials and private payloads from logs.
- Validate message size, schema version, event expiry, session binding, and sender identity.
- Do not trust local client claims about host status or target ownership.
- Keep access restricted to explicitly enabled Spectrum members.
- Treat charity donation metadata as untrusted input; bound all display strings and avoid logging unnecessary donor details.

## 11. Cost and operational constraints

- Reuse the existing gateway and Event Hub infrastructure.
- Prefer one persistent session connection per paired broadcaster.
- Avoid frequent polling when lobby events or heartbeat signals are available.
- Do not add D1/KV writes for routine per-frame or per-player state.
- Keep transient per-lobby state in memory where possible.
- Persist only state needed for access control, durable deduplication, and operational recovery.
- Do not add analytics or metrics instrumentation without a concrete operational need.

## 12. Unknowns requiring verification

No PEAK-specific API names or method signatures are approved yet. The lobby authority, player identity, item creation, network prefab, and host migration implementations must be grounded in the uploaded PEAK assemblies and verified in a private multiplayer test before code is considered complete.

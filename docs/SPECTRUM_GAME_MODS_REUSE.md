# Reusing Spectrum Game Mods — PEAK Integration Notes

Reviewed: 2026-11-27

## Decision

PEAK must reuse the existing `spectrum-game-mods` Worker in the Spectrum Cloudflare repository. Do not create a separate PEAK gateway Worker and do not add a second Twitch authentication flow.

The current service already has game-specific session adapters for Stardew Valley and R.E.P.O. The PEAK work should extend this shared service and the existing Control Center management plane.

## Existing service layout

Source repository: https://github.com/rydrakeandlonk-rgb/spectrum-cloudflare

Relevant paths:
- `workers/spectrum-game-mods/src/index.js` — shared HTTP routes and Durable Object dispatch.
- `workers/spectrum-game-mods/src/game-host-session.js` — Stardew host-session implementation.
- `workers/spectrum-game-mods/src/repo-host-session.js` — R.E.P.O. host-session implementation.
- `workers/spectrum-game-mods/src/repo-lobby-coordinator.js` — R.E.P.O. lobby coordination.
- `workers/spectrum-game-mods/README.md` — current endpoint and protocol documentation.
- `docs/SPECTRUM-GAME-MODS-ARCHITECTURE.md` — canonical shared service architecture.

## Existing pairing patterns

### R.E.P.O.

The current R.E.P.O. device pairing flow uses:
- `POST /connect/repo/pairing/start`
- `POST /connect/repo/pairing/poll`
- Control Center approval through the internal R.E.P.O. pairing-authorisation route.
- A broadcaster-scoped host credential returned only to the approved requesting device.
- WebSocket connection at `/connect/repo/{broadcaster_id}`.
- `Authorization: Bearer {host_token}` authentication.
- A client-generated session ID.
- A `session.ready` server message with protocol version 2.

The R.E.P.O. client sends lobby/session state and bounded diagnostics. The worker validates the current connection attachment and broadcaster session before accepting messages.

### Stardew Valley

The Stardew client uses its own device pairing namespace and a broadcaster-scoped host session. Its WebSocket protocol has typed command/action envelopes, bounded queues, idempotency IDs and explicit acknowledgements. Network callbacks enqueue work; the game adapter performs game-thread operations.

## PEAK extension strategy

The PEAK adapter should follow the established device-pairing and broadcaster-session pattern, but must not blindly clone either game implementation.

Before adding routes, inspect the complete current `spectrum-game-mods/src/index.js`, Durable Object classes, Wrangler bindings, and Control Center's existing Stardew/R.E.P.O. management routes.

The likely extension points are:
1. A PEAK-specific device-pairing namespace under the existing Game Mods Worker.
2. A broadcaster-scoped PEAK host session using the same credential validation and reconnect conventions.
3. A PEAK-specific session-state message containing only verified PEAK lobby/host/player observations.
4. Control Center pairing approval and revocation endpoints derived from the signed-in Twitch identity.
5. A PEAK management page showing pairing, connection, lobby, host/client and mod-version status.

Whether PEAK needs a separate Durable Object class or can share a more generic host-session implementation is an implementation decision to make after inspecting the complete current code. Do not introduce a new Durable Object binding or migration without proving it is needed.

## Event routing boundaries

- Twitch Channel Point redemptions continue to enter through Event Hub and are routed through the existing shared Game Mods service only after the PEAK session is authenticated and explicitly enabled.
- Charity donations remain in the charity worker/app. The charity system should call a dedicated authenticated Game Mods ingress or an approved shared internal event path; do not route donation events through Twitch EventSub.
- The PEAK plugin must never receive Twitch OAuth access/refresh tokens, Event Hub service credentials, Control Center service credentials, or charity-provider secrets.

## Operational constraints

- Use Durable Object transient session state for connection and pending delivery state where appropriate.
- Do not add D1 writes for WebSocket pings, heartbeats or unchanged routine state.
- Do not add KV polling or high-frequency Worker timers for lobby state.
- Use bounded messages, queues, payload sizes and reconnect delays.
- Keep host credentials revocable and scoped to one game/broadcaster session.
- Keep the integration fail-closed until the Control Center member, pairing, and active-session checks pass.

## Current verified boundary

The shared Game Mods Worker documentation describes Stardew and R.E.P.O. routes. No PEAK pairing route or PEAK-specific Control Center management route has been implemented yet. The plugin scaffold therefore does not attempt to connect to a nonexistent endpoint.

## Next backend tasks

- [ ] Inspect all Game Mods route dispatch and Durable Object bindings.
- [ ] Inspect Control Center R.E.P.O. pairing/revocation handlers and dashboard UI.
- [ ] Decide whether to generalise existing session/pairing code or add a small PEAK adapter within the same Worker.
- [ ] Add PEAK pairing start/poll/approval/revocation with Control Center-derived identity.
- [ ] Add authenticated PEAK WebSocket session and session-state diagnostics.
- [ ] Add Control Center PEAK status page.
- [ ] Add Event Hub PEAK redemption routing only after session tests pass.
- [ ] Add charity-worker PEAK delivery path separately from Event Hub.

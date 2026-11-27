# Spectrum PEAK Integration — Action Plan

Last reviewed: 2026-11-27

This is the implementation plan and acceptance checklist. Work proceeds in gated stages: do not build higher layers on top of unverified game APIs.

## Guiding principles

- Reuse the existing Spectrum Control Center, Event Hub, and charity architecture.
- Reuse the strongest delivery/session patterns from Stardew Valley and host-authority lessons from R.E.P.O.
- Keep PEAK-specific logic in the PEAK plugin.
- Use PEAK's native multiplayer and network-aware object creation.
- Do not add a second Twitch auth system or donation backend.
- Keep Cloudflare operations and persistent writes minimal.
- Use stable IDs, typed messages, deduplication, expiry, and acknowledgements.
- Start with a small explicit catalogue of approved non-combat items.
- Every stage must have a reproducible test and documented result.

## Stage 0 — Gather the correct source material

- [x] Inspect current PEAK repository tree.
- [x] Audit uploaded BepInEx archive.
- [x] Review Stardew architecture documentation and network client patterns.
- [x] Review R.E.P.O. integration design and host-authority patterns.
- [x] Review public PEAK modding documentation and PEAKLib capabilities.
- [ ] Upload PEAK managed game assemblies.
- [ ] Upload the active Vortex profile's `BepInEx/plugins/` directory and dependency manifests.
- [ ] Confirm PEAK game build/version.
- [ ] Confirm exact installed BepInEx package and version.

**Exit criteria:** We can inspect the actual game code and identify the correct build/runtime references without guessing.

## Stage 1 — PEAK API reconnaissance

- [ ] Inventory assemblies and versions.
- [ ] Confirm Unity version, scripting backend, and compatible .NET target.
- [ ] Identify lobby/session manager and session lifecycle events.
- [ ] Identify local player and remote player representations.
- [ ] Identify authoritative host/owner and host migration behaviour.
- [ ] Identify stable network player identity and peer-to-player mapping.
- [ ] Identify native network messaging/RPC mechanism.
- [ ] Identify a network-aware, benign item creation path.
- [ ] Identify item IDs/registries and validation requirements.
- [ ] Evaluate PEAKLib Core/Items against native APIs.
- [ ] Record verified types/methods in `docs/PEAK_API_NOTES.md`.

**Exit criteria:** Each critical API has assembly/type/member evidence and a clear test strategy. No guessed reflection strings remain.

## Stage 2 — Build and load a minimal plugin

- [ ] Start from the maintained PEAK BepInEx template if its current target is compatible.
- [ ] Pin the .NET SDK/target framework and game assembly references.
- [ ] Create a minimal plugin entry point.
- [ ] Add structured, credential-safe logging.
- [ ] Add config defaults and plugin version reporting.
- [ ] Verify plugin load and clean shutdown.
- [ ] Verify no dependency conflicts with the user's active Vortex profile.
- [ ] Add a repeatable local build/deploy workflow.

**Exit criteria:** The plugin loads in PEAK, reports its version, and exits cleanly without changing gameplay.

## Stage 3 — Observe lobby and player state

- [ ] Detect when the player enters/leaves a lobby.
- [ ] Report lobby/session ID without exposing private credentials.
- [ ] Detect current host/authority.
- [ ] Enumerate connected players.
- [ ] Resolve each player to a stable network identity.
- [ ] Detect disconnect/reconnect and scene transitions.
- [ ] Detect host migration.
- [ ] Clear stale lobby-scoped state.

**Exit criteria:** A local diagnostic panel/log can accurately report lobby, host, and connected player state during a private multiplayer test.

## Stage 4 — Prove native item creation and synchronisation

- [ ] Select one benign, approved item from the verified catalogue.
- [ ] Identify the game's supported creation method.
- [ ] Execute creation from the host only.
- [ ] Confirm item appears at the intended player's location.
- [ ] Confirm every client sees the same networked item.
- [ ] Confirm no client creates a duplicate local copy.
- [ ] Confirm invalid targets and invalid item IDs are rejected.
- [ ] Confirm disconnect during a pending request is handled safely.

**Exit criteria:** Host and clients observe one shared item, created once, for the intended target. No Twitch or Cloudflare connection is required for this test.

## Stage 5 — Implement Spectrum pairing and session connection

- [ ] Define PEAK-specific Control Center activation/configuration.
- [ ] Reuse existing pairing and revocable credential model.
- [ ] Reuse the common game-session gateway where possible.
- [ ] Pair each broadcaster's own PEAK installation to their Twitch identity.
- [ ] Keep one broadcaster-scoped authenticated session per paired streamer.
- [ ] Report connection, plugin version, lobby, and host/client role.
- [ ] Implement bounded reconnect and credential revocation.
- [ ] Confirm clients cannot impersonate another broadcaster or player.

**Exit criteria:** An enabled Spectrum member can pair PEAK and appear accurately in Control Center. No Twitch secret is stored in the plugin.

## Stage 6 — Implement typed host/client delivery

- [ ] Define a versioned PEAK action envelope compatible with existing Spectrum Game Mods conventions.
- [ ] Add stable event and per-target delivery IDs.
- [ ] Validate expiry, broadcaster/session binding, and sender peer.
- [ ] Add bounded in-memory queues.
- [ ] Add host forwarding for non-host origins.
- [ ] Add host-side player resolution and item allowlist validation.
- [ ] Add terminal acknowledgements and retry-safe duplicate handling.
- [ ] Add explicit handling for expired, rejected, disconnected, and retryable outcomes.

**Exit criteria:** A test command travels from the authenticated client to the host, is executed once, and receives a host-confirmed acknowledgement.

## Stage 7 — Twitch Channel Point redemption integration

- [ ] Add PEAK routing/configuration to Control Center.
- [ ] Reuse Event Hub's existing EventSub ingress and signature validation.
- [ ] Route only active, enabled broadcasters' redemptions.
- [ ] Configure stable reward-to-item mappings.
- [ ] Deliver to the paired broadcaster's player only.
- [ ] Verify the player is in the active lobby before execution.
- [ ] Test duplicate EventSub delivery and reconnect.
- [ ] Test broadcaster not in lobby and player disconnect during delivery.

**Exit criteria:** A real test redemption targets only the correct paired player and produces one host-confirmed shared item.

## Stage 8 — Charity donation integration

- [ ] Define a dedicated charity-to-game event contract.
- [ ] Keep donation provider credentials and donation validation in the charity worker/app.
- [ ] Generate a stable donation event ID.
- [ ] Resolve the intended active PEAK lobby/lobbies.
- [ ] Snapshot eligible connected players for the event.
- [ ] Create one unique target delivery per eligible player.
- [ ] Have the host coordinate exactly one item delivery per eligible player.
- [ ] Track success/failure independently per target.
- [ ] Exclude disconnected/unresolved players; do not substitute another player.
- [ ] Test duplicate donation events and partial disconnects.

**Exit criteria:** One donation event creates one shared item for each eligible connected player, without duplicates or delivery to unresolved players.

## Stage 9 — Control Center and operational polish

- [ ] PEAK-specific management page.
- [ ] Pairing/activation status.
- [ ] Active lobby and player roster.
- [ ] Host/client role and connection health.
- [ ] Approved item catalogue and reward mapping controls.
- [ ] Delivery queue and terminal result visibility.
- [ ] Manual pause/disable and credential revocation.
- [ ] Mod version compatibility status.
- [ ] Clear, actionable error messages.
- [ ] Minimal-cost operational state; no per-frame or high-frequency database writes.

**Exit criteria:** Admins can understand and control the integration without reading raw logs.

## Stage 10 — Multiplayer and regression test matrix

| Scenario | Expected result |
|---|---|
| Solo host | Local test action executes once |
| Host + one client | Both see the same shared item |
| Host + multiple clients | Each targeted delivery is visible to all |
| Redemption by host's Twitch identity | Host is the target |
| Redemption by a client Twitch identity | Correct client is the target |
| Client without paired Spectrum identity | Not eligible as a broadcaster target |
| Unresolved player identity | Rejected; no fallback target |
| Target disconnects before execution | Expired/rejected; no substitute |
| Target disconnects during queue | Terminal result recorded; no duplicate on reconnect |
| Duplicate event ID | Previous result returned; no second execution |
| Host migration | One authority continues; no double execution |
| Lobby closes | Lobby queues/state cleared or expired |
| Plugin reconnects | Session resumes safely; expired work is not replayed |
| Invalid item ID | Rejected by host allowlist |
| Charity event | One target delivery per eligible connected player |
| Partial charity disconnect | Remaining eligible players handled independently |
| Revoked access | Further commands rejected |
| Old plugin version | Clear compatibility response; no undefined execution |

## Stage 11 — Release readiness

- [ ] Pin dependency versions.
- [ ] Add automated build and static validation.
- [ ] Add package manifest and install instructions.
- [ ] Confirm no secrets or game assemblies are packaged.
- [ ] Confirm plugin is compatible with a clean supported PEAK profile.
- [ ] Confirm multiplayer requirements are documented.
- [ ] Complete the test matrix.
- [ ] Record tested PEAK and BepInEx versions.
- [ ] Prepare release notes and rollback procedure.
- [ ] Confirm Control Center and charity deployment dependencies before enabling production access.

**Release gate:** No production activation until the host/client and deduplication tests pass in a private lobby.

## Suggested milestones

1. **Research complete:** assemblies inspected and API notes written.
2. **Local proof:** plugin loads, detects lobby/players, and creates one approved shared item.
3. **Spectrum connected:** pairing and authenticated session work.
4. **Redemption complete:** targeted channel point event works end-to-end.
5. **Charity complete:** one-per-connected-player donation fan-out works.
6. **Production ready:** Control Center visibility, recovery, regression, and release checks pass.

## Change management

- Keep this repository separate from Stardew and R.E.P.O.
- Reuse shared contracts, not game-specific source code.
- Read this plan and architecture before each implementation stage.
- Update API notes when a method or dependency is verified.
- Update this checklist only when a stage's exit criteria are met.
- Do not silently change shared gateway schemas; document and test compatibility across existing game integrations first.

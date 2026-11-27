# Implementation Status

Last updated: 2026-11-27

## Current state

The project now has a minimal BepInEx plugin plus an initial Spectrum connection client. The client reuses the existing PEAK device-code pairing and authenticated WebSocket routes in the shared Spectrum Game Mods Worker.

### Implemented in repository

- BepInEx plugin entry point with stable plugin GUID and version 0.2.0.
- Local configuration for enabling the connection, storing the paired host credential, and diagnostic verbosity.
- Device-code pairing through the existing Spectrum Game Mods endpoint.
- Control Center approval using the signed-in Spectrum Twitch identity; no Twitch OAuth or client secret is implemented in the mod.
- Private host token persisted in the local BepInEx configuration after pairing.
- Authenticated WebSocket connection to the PEAK-specific host session.
- session.ready handshake and periodic ping/pong heartbeat.
- No extra JSON runtime package; uses the target framework's data-contract JSON serializer.
- Existing shared Game Mods worker routes and isolated PeakHostSession Durable Object are reused.
- PEAK managed assemblies are uploaded under Uploads/PEAK_Data/Managed for API reconnaissance.

### Verification state

- [x] Connection client source committed.
- [x] Existing PEAK pairing and WebSocket route contract inspected.
- [x] Successful PEAK plugin CI build and DLL artifact upload verified (run 45998067239).
- [ ] Plugin loaded in PEAK.
- [ ] Pairing completed in game.
- [ ] Authenticated WebSocket handshake observed in production.
- [ ] Clean shutdown/reconnect verified in PEAK.

The CI build and artifact upload now pass. The DLL is available from the latest successful GitHub Actions run; in-game pairing and loading still need to be tested before broader use. The connection feature is disabled by default; set Connection.EnableIntegration = true in the generated BepInEx config to start device pairing.

### Not implemented

- PEAK lobby/player adapter.
- PEAK host/authority detection.
- Player-to-Twitch identity mapping beyond the paired broadcaster identity.
- Any PEAK gameplay hooks or world mutations.
- Any item catalogue or item creation.
- Multiplayer RPC/network transport for gameplay actions.
- Event Hub redemption delivery.
- Charity event delivery.
- Control Center PEAK session diagnostics page verification.

These remain gated on API reconnaissance and end-to-end connection tests.

## Build compatibility notes

The project targets netstandard2.1 and uses the .NET 10 SDK. The local PEAK managed assemblies are available in the repository; the build can be configured to reference them through Config.Build.user.props. The plugin currently uses only the shared gateway contract and does not compile against unverified PEAK gameplay types.

## Next gates

1. Run and inspect the GitHub Actions build.
2. Verify the installed BepInEx pack and active Vortex plugin dependencies.
3. Test device pairing and WebSocket connection in PEAK.
4. Inspect Photon/Zorro/Assembly-CSharp types to identify lobby and player lifecycle APIs.
5. Add a verified read-only lobby/player adapter.

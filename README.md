# Spectrum PEAK Mod

Spectrum's private PEAK integration. This repository contains the PEAK-specific game adapter and its project documentation; shared Twitch identity, event routing, and charity services remain in the existing Spectrum infrastructure.

## Project status

The repository is currently at the research and planning stage. The uploaded BepInEx archive has been audited, but PEAK's game-managed assemblies and active Vortex plugin directory have not yet been uploaded. As a result, no PEAK lobby, player, networking, or item APIs have been assumed or implemented.

## Architecture boundaries

- **Control Center:** Twitch identity, access permissions, pairing, PEAK configuration, and administration.
- **Event Hub:** Twitch EventSub validation and Channel Point redemption routing.
- **Charity system:** Donation validation and donation event publication, separate from Event Hub.
- **PEAK plugin:** PEAK lobby/player integration, approved item catalogue, and game-thread execution using native multiplayer.

The intended gameplay is targeted redemption delivery to the paired broadcaster's player, and one eligible delivery per connected player for a charity event. The host should coordinate shared-world execution using PEAK's own network-aware mechanisms. These behaviours are design targets and require in-game verification.

## Documentation

- [Project status and upload audit](docs/PROJECT_STATUS.md)
- [Architecture and event flows](docs/ARCHITECTURE.md)
- [PEAK modding research and dependencies](docs/RESEARCH_AND_DEPENDENCIES.md)
- [Implementation action plan and test matrix](docs/ACTION_PLAN.md)

## Current upload

The current `Uploads/BEPIN Archive/` contains BepInEx 5.4.23.6 framework files and supporting libraries. It does not contain the PEAK managed assemblies or the active `BepInEx/plugins/` folder.

See [Project Status](docs/PROJECT_STATUS.md) for the exact additional files needed before API reconnaissance can proceed.

## Development principles

1. Reuse existing Spectrum infrastructure; do not add a second Twitch authentication or donation stack.
2. Keep PEAK-specific logic in this repository.
3. Prefer PEAK native multiplayer over a parallel networking implementation.
4. Use typed, versioned, bounded, deduplicated requests and host-confirmed acknowledgements.
5. Keep the initial item catalogue explicit and limited to approved non-combat content.
6. Do not guess PEAK APIs; record assembly/type/member evidence before implementation.
7. Keep Cloudflare storage and background activity minimal.

# PEAK Integration — Project Status

Last reviewed: 2026-11-27

## Executive summary

The repository is currently an investigation/documentation baseline. No PEAK plugin source code or game-specific integration code has been added yet.

The goal is a private Spectrum integration for PEAK that reuses the existing Spectrum Control Center, Event Hub, and charity system. The PEAK plugin should be a thin game adapter. It must not recreate Twitch authentication, the central EventSub ingress, or donation processing.

## Repository audit

Repository: https://github.com/rydrakeandlonk-rgb/spectrum-PEAK-mod

Current root:
- `README.md`
- `Uploads/BEPIN Archive/`

The uploaded archive contains:
- BepInEx 5.4.23.6 framework files.
- Harmony / HarmonyX and MonoMod support assemblies.
- BepInEx core assemblies and XML documentation.
- `doorstop_config.ini` and `winhttp.dll`.
- Vortex deployment metadata.

The uploaded tree does **not** currently contain:
- `BepInEx/plugins/` or any installed plugin DLLs.
- PEAK's `Assembly-CSharp.dll` or other game-managed assemblies.
- Unity/game managed reference assemblies.
- PEAK decompilation output.
- PEAKLib assemblies or package files.
- A Spectrum PEAK plugin project.

The archive is therefore useful for identifying the installed loader/runtime family, but it is not enough to inspect PEAK gameplay or multiplayer APIs.

## Important environment note

The public PEAK modding guide currently documents a PEAK-specific BepInEx setup and a .NET 10+ development template. The uploaded folder is named `BepInEx_win_x64_5.4.23.6`; we have not yet verified whether this is the PEAK-specific distribution or a generic BepInEx archive. Do not change the user's installation based only on the folder name. Confirm its configuration and installed game path first.

The PEAK modding community also maintains PEAKLib, which exposes modules for network prefabs, content registration, items, UI, and stats. It is a candidate dependency, not an automatic requirement. We should only reference the parts required by the final implementation and pin a tested version.

## Existing Spectrum project lessons

### Stardew Valley

Reference repository: https://github.com/rydrakeandlonk-rgb/spectrum-stardew-mod

Useful established patterns:
- Control Center owns identity, activation, permissions, and pairing approval.
- Event Hub validates and routes Twitch redemption events.
- A broadcaster-scoped game session receives only that broadcaster's commands.
- The mod uses typed messages, bounded queues, request IDs, acknowledgements, expiry, and deduplication.
- Network callbacks validate/enqueue work; game state changes happen on the game's main thread.
- A non-host client forwards its typed request to the multiplayer host.
- The host validates the sending peer, resolves the player, performs the shared-world mutation, and confirms completion.
- The gateway never supplies arbitrary game class names or constructor details.

### R.E.P.O.

Reference repository: https://github.com/rydrakeandlonk-rgb/spectrum-REPO-mod

Useful established patterns:
- BepInEx plugin lifecycle and in-game pairing UI.
- Lobby/session reporting and host/client identification.
- Host-authoritative gameplay actions.
- Stable catalogue IDs and validation rather than trusting arbitrary client-supplied game identifiers.
- The game-specific mod uses the game's existing spawn executor and multiplayer mechanisms.
- Player targeting must be resolved against actual game/network ownership, not inferred from display names alone.

These are design references, not code to copy wholesale. PEAK's actual APIs must be inspected before choosing implementation details.

## Confirmed decisions

1. Control Center remains the owner of Twitch identity, authorisation, activation, pairing, and PEAK-specific management/configuration.
2. Event Hub remains the owner of Twitch EventSub ingestion and redemption routing.
3. The charity system remains separate and owns donation data/events.
4. The PEAK plugin owns PEAK-specific lobby, player, item-catalogue, and gameplay execution logic.
5. PEAK's native multiplayer system should synchronise shared gameplay changes wherever possible.
6. The host should be the authority for shared-world execution, subject to verification of PEAK's actual networking model.
7. A redemption is intended for the player whose Twitch channel received it.
8. A charity donation is intended to produce one eligible item delivery per connected player.
9. Disconnected or unresolved players must not receive deliveries.
10. Every event needs stable identity, deduplication, expiry, and an acknowledgement path.
11. Do not introduce a new Twitch OAuth flow or a new charity backend inside the mod.
12. Keep Cloudflare storage writes and background polling minimal.

## Open questions — must be answered by source inspection

- Which Unity version and scripting backend does the current PEAK build use?
- Which BepInEx distribution/version is actually installed and supported?
- Which game assemblies define the lobby, player, item, inventory, and network behaviours?
- Which transport/library does PEAK use for multiplayer?
- How does PEAK identify the host and each connected player?
- Which stable network identity can be safely associated with a paired Twitch account?
- Which game API is the supported path for creating and synchronising a world item?
- Does PEAKLib provide a necessary API for the chosen item types or network prefab registration?
- Which items are safe and appropriate for the first supported catalogue?
- What happens on host migration, player disconnect, scene transition, lobby rejoin, and reconnect?
- Does a client need the plugin installed for all supported features, and what happens when only the host has it?

Do not mark these as solved until supported by decompiled source, API documentation, or an in-game test.

## Required next upload

Please add a separate, clearly named folder such as `Uploads/PEAK Game Assemblies/` containing the PEAK build's managed assemblies and any relevant plugin dependencies. Also upload the contents of the actual Vortex profile's `BepInEx/plugins/` directory.

At minimum, the investigation needs:
- `PEAK_Data/Managed/Assembly-CSharp.dll`
- `PEAK_Data/Managed/UnityEngine*.dll` and relevant Unity modules
- Any game networking / transport assemblies present in that directory
- `BepInEx/plugins/` plugin DLLs and their manifests/config metadata
- PEAKLib DLLs, if installed

Do not upload private tokens, credentials, personal save files, or unrelated user data. We need assemblies and plugin metadata, not your entire game profile.

If practical, also include the PEAK game version/build number and the exact BepInEx package name/version shown by Vortex.

## Current implementation status

| Area | Status |
|---|---|
| Existing Spectrum architecture reviewed | Complete |
| Stardew architecture patterns reviewed | Complete |
| R.E.P.O. architecture patterns reviewed | Complete |
| PEAK repository upload audited | Complete |
| Public PEAK modding docs reviewed | Initial review complete |
| PEAK game assemblies inspected | Blocked — not uploaded |
| PEAK lobby/player APIs verified | Not started |
| PEAK item creation path verified | Not started |
| PEAK multiplayer sync verified | Not started |
| BepInEx plugin skeleton | Not started |
| Control Center integration | Not started |
| Event Hub integration | Not started |
| Charity integration | Not started |
| Multiplayer acceptance tests | Not started |

## Scaffold update — 2026-11-27

The repository now also contains an initial BepInEx plugin scaffold under `src/Spectrum.PEAK/`, a .NET solution, build configuration, and a GitHub Actions build workflow. See [Implementation Status](IMPLEMENTATION_STATUS.md).

The scaffold has not yet been confirmed by a successful CI build or an in-game load test. The shared Spectrum Game Mods Worker and its Stardew/R.E.P.O. pairing patterns have now been inspected at the documentation and client-source level. PEAK-specific pairing and Control Center routes do not exist yet; see [Shared Game Mods Reuse](SPECTRUM_GAME_MODS_REUSE.md).

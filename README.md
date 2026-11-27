# Spectrum PEAK Mod

Spectrum's PEAK game adapter. Shared Twitch identity, pairing approval, event routing, and charity processing remain in the existing Spectrum infrastructure.

## Current status

The initial BepInEx plugin scaffold and CI build workflow are now in the repository. The plugin bootstrap has configuration and startup diagnostics, but deliberately does not yet inspect PEAK gameplay or connect to the network.

**The scaffold has been committed but has not yet been confirmed by a successful CI build or an in-game load test.** Do not install it into the live Vortex profile yet.

## Architecture boundaries

- **Control Center:** Twitch identity, access permissions, pairing, PEAK configuration, and administration.
- **Event Hub:** Twitch EventSub validation and Channel Point redemption routing.
- **Charity system:** Donation validation and donation event publication, separate from Event Hub.
- **PEAK plugin:** PEAK-specific session/player observation and safe game-side execution using verified native APIs.

The intended integration uses the shared Spectrum Game Mods service. No new Twitch OAuth flow or donation provider integration belongs in the plugin.

## Build

Requirements:
- .NET SDK 10 or newer.
- PEAK's supported BepInEx 5 installation for eventual runtime testing.

From the repository root:

```powershell
dotnet restore Spectrum.PEAK.slnx
dotnet build Spectrum.PEAK.slnx -c Release
```

Copy `Config.Build.user.props.template` to `Config.Build.user.props` and set `PEAKGameRootDir` if PEAK is installed outside the default Steam location. The local deployment switch is disabled by default.

GitHub Actions runs the same restore/build process on pushes to `main`, pull requests, and manual workflow dispatch.

## Documentation

- [Project status and repository audit](docs/PROJECT_STATUS.md)
- [Implementation status](docs/IMPLEMENTATION_STATUS.md)
- [Architecture and event flows](docs/ARCHITECTURE.md)
- [PEAK modding research and dependencies](docs/RESEARCH_AND_DEPENDENCIES.md)
- [Implementation action plan and test matrix](docs/ACTION_PLAN.md)

## Current blocker

The uploaded BepInEx archive contains the loader framework but not the PEAK managed assemblies or the active `BepInEx/plugins/` folder. We need those before implementing lobby/player observation or any PEAK-specific runtime adapter.

See [Project Status](docs/PROJECT_STATUS.md) for the exact upload list.

## Development rules

1. Reuse existing Spectrum infrastructure; do not add a second Twitch authentication or donation stack.
2. Keep PEAK-specific code in this repository.
3. Prefer PEAK's native multiplayer for shared game state.
4. Use typed, bounded, deduplicated messages and host-confirmed acknowledgements.
5. Do not guess PEAK APIs; record assembly/type/member evidence before implementation.
6. Keep operational writes and background activity minimal.

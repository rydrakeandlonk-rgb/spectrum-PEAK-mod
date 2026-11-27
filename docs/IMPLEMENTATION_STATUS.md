# Implementation Status

Last updated: 2026-11-27

## Current state

The first code scaffold has been committed. It is deliberately limited to infrastructure that does not require undocumented PEAK internals.

### Implemented in repository

- BepInEx plugin entry point with stable plugin GUID and version.
- Local BepInEx configuration for feature gating and diagnostic verbosity.
- Startup/shutdown logging.
- Game-neutral session snapshot models and an adapter interface.
- .NET 10 SDK pin and PEAK-compatible netstandard2.1 target.
- GitHub Actions restore/build workflow.
- Per-developer game-path configuration template.
- Local deployment disabled by default.

### Verification state

- [x] Source scaffold committed.
- [x] CI workflow committed.
- [ ] Successful CI build observed.
- [ ] Local build confirmed against the user's SDK.
- [ ] Plugin loaded in PEAK.
- [ ] Clean shutdown verified in PEAK.

Do not install this scaffold into the active Vortex profile until the build and load checks pass.

### Not implemented

- PEAK lobby/player adapter.
- PEAK host/authority detection.
- Any PEAK gameplay hooks or world mutations.
- Any item catalogue or item creation.
- Multiplayer RPC/network transport.
- Control Center pairing or session authentication.
- Event Hub redemption delivery.
- Charity event delivery.
- Control Center PEAK page.

These are intentionally blocked until the installed game assemblies and existing Spectrum gateway contracts have been inspected.

## Build compatibility notes

The PEAK community's maintained BepInEx template currently uses:
- .NET SDK 10.0 or newer as the build toolchain.
- `netstandard2.1` as the plugin target framework.
- BepInEx 5 package references.
- Unity 6000.0.36 module package as a fallback when local game assemblies are unavailable.

This project follows that target/build-tool split. It does not assume that the uploaded archive is the current official PEAK BepInEx pack. The uploaded archive and active Vortex deployment still need to be compared.

## Local build

1. Install .NET SDK 10.
2. Copy `Config.Build.user.props.template` to `Config.Build.user.props` if PEAK is installed outside the default Steam path.
3. Restore and build:

```powershell
dotnet restore Spectrum.PEAK.slnx
dotnet build Spectrum.PEAK.slnx -c Release
```

Automatic local deployment is disabled by default until the active PEAK/Vortex profile path is confirmed.

## Next gates

1. Obtain PEAK managed assemblies and the active `BepInEx/plugins/` folder.
2. Verify the installed BepInEx pack and game build.
3. Inspect the existing Control Center and R.E.P.O. gateway protocol before implementing a socket client.
4. Build and run the plugin in PEAK.
5. Add the first verified lobby/session adapter.

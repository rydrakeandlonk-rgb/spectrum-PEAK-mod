# Spectrum PEAK Mod

A BepInEx 5 plugin connecting PEAK to Spectrum's existing Control Center and Game Mods infrastructure.

## Current milestone: authenticated connection

Version 0.2.0 adds the initial device-pairing and WebSocket connection client.

1. Install the built Spectrum.PEAK.dll into the active PEAK BepInEx plugins folder after the CI build has passed.
2. Launch PEAK once so BepInEx creates the plugin configuration.
3. Open BepInEx/config/rydrake.spectrum.peak.cfg.
4. Set Connection.EnableIntegration to true and launch PEAK again.
5. The plugin logs a Spectrum pairing URL and one-time user code. Open the URL, sign in to the intended Spectrum Twitch account, and approve the device.
6. The mod receives and stores a private broadcaster-scoped host credential locally, then opens an authenticated WebSocket to Spectrum Game Mods.
7. The Control Center PEAK page should show the paired session as connected once the handshake is complete.

The host token is private. Do not share the BepInEx config file or publish its contents. Disabling the integration stops the connection on the next plugin load; access can also be revoked from Control Center.

## Current limitations

- This milestone proves pairing and authenticated transport only.
- It does not yet detect PEAK lobby/player state.
- It does not create or modify gameplay objects.
- It does not yet process Channel Point redemptions or charity donations.
- PEAK gameplay APIs are being inspected from the uploaded Managed assemblies before hooks are added.

## Build

Requires .NET SDK 10. The plugin targets netstandard2.1.

```powershell
dotnet restore Spectrum.PEAK.slnx
dotnet build Spectrum.PEAK.slnx -c Release
```

The GitHub Actions workflow builds the plugin on pushes to main and uploads the DLL as an artifact. Do not install a build until the corresponding workflow run succeeds.

## Architecture

- Twitch identity and authorization: Spectrum Control Center.
- Device pairing and authenticated game session: existing Spectrum Game Mods Worker.
- Twitch Channel Point events: Event Hub, to be connected in a later stage.
- Charity donation events: separate Charity Worker/app, to be connected in a later stage.
- PEAK multiplayer and gameplay integration: PEAK-specific adapter using the game's native networking.

The mod does not implement Twitch OAuth and does not contain Twitch client secrets.

See docs/IMPLEMENTATION_STATUS.md, docs/ACTION_PLAN.md, and docs/SPECTRUM_GAME_MODS_REUSE.md for implementation detail.

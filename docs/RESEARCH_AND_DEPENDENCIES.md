# PEAK Modding Research and Dependency Audit

Last reviewed: 2026-11-27

This document separates public documentation findings from facts verified against the user's installed game. Public documentation is useful for planning, but the current PEAK assemblies are still required before selecting APIs.

## 1. Public modding documentation reviewed

### PEAK Modding Wiki — initial setup

https://peak.modding-community.com/creating-a-mod/initial-setup/

The current guide describes:
- .NET SDK 10.0 or higher for mod development.
- A PEAK-specific BepInEx installation.
- Running the game once after installation to generate configuration folders.
- Using a decompiler such as ILSpy to inspect game code.
- Starting from the PEAK BepInEx template.

Important: the guide notes that the BepInEx console may be disabled in PEAK because of stability concerns. Do not make console visibility a requirement for the plugin.

### PEAK Modding Wiki — creating a project

https://peak.modding-community.com/creating-a-mod/creating-a-project/

The project guide points to the community BepInEx template and requires .NET SDK 10 or newer. Verify the template's current target framework and dependencies when scaffolding; do not assume a generic Unity/BepInEx template is equivalent.

### PEAKModding BepInEx template

https://github.com/PEAKModding/BepInExTemplate

Candidate starting point for the plugin project. Inspect its current project file, target framework, package references, build output, and packaging configuration before adopting it.

### BepInEx documentation

https://docs.bepinex.dev/

BepInEx provides plugin loading, configuration/logging, and Harmony-based runtime patching for supported Unity/.NET game configurations. The user's uploaded archive contains BepInEx 5 core and Harmony-related assemblies, but compatibility with the current PEAK build still needs confirmation.

### PEAKLib

https://github.com/PEAKModding/PEAKLib

PEAKLib is a community API. Its documented feature areas include:
- Core module: network prefab API, content registry API, bundle loader, host mod-list listener.
- Items module: Items API and Item Acceptor API.
- UI module.
- Stats module.

The repository is MIT-licensed and includes tests. It documents local build configuration through `Config.Build.user.props` and game assembly references.

Decision: PEAKLib is a candidate optional dependency. We will inspect the APIs and release compatibility before deciding whether the plugin references it. Prefer PEAK's existing supported APIs if they are sufficient. If PEAKLib is needed, pin a known tested release/version and declare it as a dependency in the package manifest.

### PEAK custom items documentation

https://peak.modding-community.com/content-creation/custom-items/

The custom-item guide uses PEAKLib.Items and a Unity project workflow. This project is not currently planning to author custom assets. Our initial scope is integration with a small explicit catalogue of existing, approved game content, so a full Unity asset authoring pipeline is not currently justified.

## 2. Current uploaded BepInEx archive

Repository path:
`Uploads/BEPIN Archive/BepInEx_win_x64_5.4.23.6/`

Observed contents include:
- `BepInEx/core/BepInEx.dll`
- `BepInEx/core/BepInEx.Harmony.dll`
- `BepInEx/core/0Harmony.dll`
- `BepInEx/core/0Harmony20.dll`
- `BepInEx/core/HarmonyXInterop.dll`
- `BepInEx/core/Mono.Cecil*.dll`
- `BepInEx/core/MonoMod.RuntimeDetour.dll`
- `BepInEx/core/MonoMod.Utils.dll`
- `doorstop_config.ini`
- `winhttp.dll`
- Vortex deployment metadata.

This confirms a BepInEx 5-family framework archive with Harmony/MonoMod support files. It does not prove that this exact package is the PEAK-specific supported pack or that it is the version currently deployed in the active game directory.

No plugin DLLs or PEAK game assemblies are present in the uploaded tree.

## 3. Required local files for the next investigation

Upload from the installed PEAK game directory:
- `PEAK_Data/Managed/Assembly-CSharp.dll`
- `PEAK_Data/Managed/UnityEngine*.dll`
- `PEAK_Data/Managed/Unity.*.dll` modules referenced by the game
- Game networking/transport assemblies present in `PEAK_Data/Managed/`
- Any additional game-specific managed assemblies referenced by `Assembly-CSharp.dll`

Upload from the active Vortex profile:
- Contents of `BepInEx/plugins/`
- Plugin manifest/dependency metadata (for example Thunderstore manifest files)
- PEAKLib assemblies if installed
- Relevant plugin configuration files only when they contain no secrets or private tokens

Also record:
- PEAK version/build number.
- Exact BepInEx package name and version shown in Vortex.
- Whether the game starts successfully with the current profile.
- Whether the profile has other PEAK mods that alter lobby/player/network behaviour.

Do not upload:
- Twitch tokens, OAuth credentials, API keys, passwords, or private pairing credentials.
- Personal save files or unrelated user files.
- Entire game directories when the small set of managed assemblies is sufficient.

## 4. Inspection workflow once files arrive

1. Inventory assembly names, versions, and target framework metadata.
2. Identify Unity version and scripting backend from the game build and assemblies.
3. Inspect the game's lobby/session and player identity types.
4. Identify the actual authority/host model and how it changes during migration.
5. Inspect the native network message/RPC path used by existing gameplay objects.
6. Identify a safe, non-combat item creation path that is network-synchronised.
7. Inspect any installed mod that already uses the relevant APIs.
8. Compare the native APIs with PEAKLib's Core and Items modules.
9. Write a short API decision record with evidence: assembly, type, method, and test status.
10. Only then scaffold the plugin and implement a minimal in-game probe.

## 5. Dependency policy

| Dependency | Current decision | Reason |
|---|---|---|
| BepInEx PEAK-supported pack | Required | Plugin loader; exact installed package/version still to verify |
| Harmony/HarmonyX | Conditional | Use only for a verified hook that cannot be achieved through a stable game API |
| PEAKLib Core | Candidate | Potentially useful for network prefabs and content registration |
| PEAKLib Items | Candidate | Potentially useful for item-specific APIs |
| Unity/game assemblies | Required references | Compile against the actual installed PEAK build |
| Extra multiplayer framework | Not planned | Reuse PEAK's native multiplayer first |
| New Twitch OAuth library | Not required | Control Center owns Twitch authentication |
| New charity provider SDK | Not required | Charity worker/app owns donation provider integration |

Avoid shipping duplicate framework DLLs that the mod loader already provides. Avoid copying game assemblies into the final mod package; use them as local compile references only.

## 6. Research limitations

Public docs and repository READMEs do not establish the exact current PEAK implementation. The current game assemblies have not yet been supplied. No PEAK class names, RPC signatures, player identifiers, or spawn methods should be guessed or hard-coded until verified.

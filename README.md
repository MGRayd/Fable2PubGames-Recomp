# Fable II - Pub Games Recompiled

An experimental static recompilation of **Fable II Pub Games** for Windows,
built with ReXGlue.

No original game files are included. You must provide your own legally
obtained Xbox 360 XBLA container. The launcher extracts it locally on your PC.

## Current status

- All three Pub Games and their tutorials are playable.
- Full-game licence behaviour is supported.
- Local patron saves and a launcher save editor are supported.
- Achievements unlock locally with an in-game themed overlay.
- Fable II heroes can be linked through the original `Fable2PubInfo.xml`
  bridge, with guarded launcher syncing and timestamped backups.
- Windowed and fullscreen launch options are available.

This is an unofficial fan project. Fable, Fable II and related names and
assets belong to their respective owners. This project is not affiliated with
or endorsed by Microsoft or Xbox Game Studios.

## Quick start

1. Download the Windows release ZIP and extract it to a writable folder.
2. Run `Fable2PubGamesLauncher.exe`.
3. Choose your own Fable II Pub Games Xbox 360 container.
4. Select **Install Game Files**.
5. Select **Play Pub Games**.

The launcher creates `data`, `saves`, `logs` and `cache` locally. These folders
are deliberately absent from the release.

## Fable II character linking

Open **Advanced Options**, choose the `saves` folder from Fable II Recompiled,
then enable **Sync progress when Pub Games closes**. The launcher imports the
complete title-save structure before play and only returns validated
`Fable2PubInfo.xml` bridge records afterward. Original bridge records are
backed up under `pubgames-sync-backups` before replacement.

Do not run Fable II and Pub Games at the same time while syncing.

## Building

Requirements:

- CMake 3.25 or newer
- Ninja
- LLVM/Clang 20 or newer
- .NET 8 SDK for the launcher
- ReXGlue SDK based on commit `bd833a2ab69d305f99ddf57418fc38ffec8af93d`
  with `patches/rexglue-pubgames.patch` applied

Place your own extracted `default.xex` in `game/default.xex`, then run ReXGlue
code generation using `Fable2PubGames_manifest.toml`. Configure and build with
the matching CMake preset. The checked-in generated sources allow normal host
builds without distributing the original XEX.

Publish the launcher with:

```powershell
.\launcher\publish.ps1
```

## Legal and content policy

Do not upload or request Xbox 360 containers, `default.xex`, extracted `data`
folders, game assets, saves or download links. Issues or discussions providing
pirated content will be removed.

## Credits

- [ReXGlue](https://github.com/rexglue/rexglue-sdk)
- [Fable 2 Recompiled](https://github.com/himdo/Fable-2-Recomp)
- [Daytona USA ReXGlue Recompilation](https://github.com/Subarasheese/daytona-xbla-recomp)

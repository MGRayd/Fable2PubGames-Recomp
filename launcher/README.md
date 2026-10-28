# Fable II Pub Games Launcher

The launcher does not include game content. On first run, the user selects their legally obtained
Xbox 360 LIVE container. The bundled read-only STFS extractor installs the contents into `data`.

## Build a release

From PowerShell:

```powershell
.\launcher\publish.ps1
```

The portable release is created in `out\release\Fable2PubGames`. Do not add `data`, `saves`,
`cache`, or `logs` to distributed builds.

## Runtime layout

```text
Fable2PubGamesLauncher.exe
Fable2PubGames.exe
Fable2PubGames.toml
rexruntime.dll
rexgpu-xenos.dll
tools\Extract-STFS.ps1
tools\Extract-STFS.LICENSE.txt
data\                         created by the launcher
```

# Last Epoch HUD

Updated build of [Ash's Last Epoch HUD](https://github.com/RCInet/LastEpoch_Mods) for the current game (Unity 6000.4.8) and MelonLoader 0.7.3 Open-Beta.

In a zone, press **F3** to open and close the mod menu. Escape closes it.

## Install

1. Install [MelonLoader 0.7.3 Open-Beta](https://github.com/LavaGang/MelonLoader/releases) on Last Epoch. Point the installer at the game folder.

2. Start the game once, wait until the main menu, then close it. MelonLoader generates its files on that first launch.

3. From the [latest release](https://github.com/Syncingoutt/LastEpoch_Mods/releases), download `LastEpoch_Hud.dll` and `LastEpoch_Hud.zip`. They are separate files. Put the DLL directly in the game `Mods` folder. Unzip `LastEpoch_Hud.zip` into that same `Mods` folder. You should have:

   ```text
   <Last Epoch>\Mods\LastEpoch_Hud.dll
   <Last Epoch>\Mods\LastEpoch_Hud
   ```

   The DLL stays next to the `LastEpoch_Hud` folder, not inside it.

4. If the mod is listed but never runs, or `MelonLoader\Latest.log` says `BadImageFormatException` or `No Support Module Loaded`, close the game. Download `UnityEngine.CoreModule.dll` from the same release and replace:

   ```text
   <Last Epoch>\MelonLoader\Il2CppAssemblies\UnityEngine.CoreModule.dll
   ```

   That file is the only MelonLoader change. Do not replace the whole MelonLoader folder. It only matches this game version. After a game update, delete `MelonLoader\Il2CppAssemblies` and let MelonLoader generate it again.

## Build

The project references assemblies from your Last Epoch install. If the game is not in the default Steam folder:

```powershell
dotnet build .\LastEpoch_Hud\LastEpoch_Hud.csproj -c Release -p:LastEpochPath="D:\SteamLibrary\steamapps\common\Last Epoch"
```

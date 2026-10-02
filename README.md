# Last Epoch HUD

Updated build of [Ash's Last Epoch HUD](https://github.com/RCInet/LastEpoch_Mods) for the current game (Unity 6000.4.8) and MelonLoader 0.7.3 Open-Beta.

In a zone, press **F3** to open and close the mod menu. Escape closes it.

## Install

1. Install [MelonLoader 0.7.3 Open-Beta](https://github.com/LavaGang/MelonLoader/releases). Point the installer at the game folder.
2. Start the game once, wait until the main menu, then close it.
3. From the [latest release](https://github.com/Syncingoutt/LastEpoch_Mods/releases), download `LastEpoch_Hud.zip` and `UnityEngine.CoreModule.dll`.
4. Put the DLL directly in the game `Mods` folder. Unzip `LastEpoch_Hud.zip` into that same `Mods` folder. You should have:

   ```text
   <Last Epoch>\Mods\LastEpoch_Hud.dll
   <Last Epoch>\Mods\LastEpoch_Hud
   ```

   The DLL stays next to the `LastEpoch_Hud` folder, not inside it.

5. Replace the downloaded `UnityEngine.CoreModule.dll` in this folder:

   ```text
   <Last Epoch>\MelonLoader\Il2CppAssemblies\
   ```

## In case of issues

1. Delete mods, delete melonloader
2. Head here: https://melon-loader.com/ (or github) and download MelonLoader 0.7.3
3. Launch the game once
4. Head into https://github.com/Syncingoutt/LastEpoch_Mods/releases/tag/v4.4.12 and download the zip and the .dll file
5. Add the LastEpoch_Hud.dll and folder into the Mods folder
6. Replace the UnityEngine.CoreModule.dll inside MelonLoader\Il2CppAssemblies
7. Head into the game and check if it works keybind F3

In case this does not work, please contact me on Discord: sync0333 (attach a log file from MelonLoader/latest.log)

## Build

The project references assemblies from your Last Epoch install. If the game is not in the default Steam folder:

```powershell
dotnet build .\LastEpoch_Hud\LastEpoch_Hud.csproj -c Release -p:LastEpochPath="D:\SteamLibrary\steamapps\common\Last Epoch"
```

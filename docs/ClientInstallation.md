## Client mod installation
To make mod work, you need two pieces. Custom server [(Impostor)](https://github.com/CrowdedMods/CrowdedMod/tree/master-refactor/docs/ServerInstallation.md) and a client-side mod (CrowdedMod).
<br/>After installing this mod, you need to also [add custom server to the client menu](https://github.com/CrowdedMods/CrowdedMod/tree/master-refactor/docs/ServerSelection.md).
### Method 1 (the easiest)
Use dropship/launcher. Coming soon.
<!-- ### Method 2 (recommended) --> 
<!-- 1. Download zip from [releases](https://github.com/CrowdedMods/CrowdedMod/releases) -->
<!-- 2. Open your Downloads folder and double click the zip file -->
<!-- 3. Select all files (Ctrl+A) and copy them (Ctrl+C) -->
<!-- 4. Open Steam library, select Among Us. Click Settings (gear icon) > Manage > Browse local files -->
<!-- 5. A window should open. Paste files here (Ctrl+V) -->
<!-- 6. Start Among Us **from  Steam**. Not from toolbar or shortcut or search -->
<!-- 7. After Among Us started, in the left upper corner there should be CrowdedMod version information displayed -->
### Method 2 (recommended)
1. Download latest [BepInEx (Reactor fork)](https://github.com/NuclearPowered/BepInEx/releases)
2. Extract all files from zipped archive and put them in Among Us directory (where Among Us.exe is)
3. Download [latest Reactor.dll for 2020.3.5s](https://nightly.link/NuclearPowered/Reactor/workflows/main/master) and put it in `YourAmongUsDirectory/BepInEx/plugins` (you probably have to unzip it first)
4. Download dll from [CurseForge](https://www.curseforge.com/among-us/all-mods/crowdedmod/files) and put it in `YourAmongUsDirectory/BepInEx/plugins` (you probably have to unzip it first)

### Method 3 (for security paranoid)
1. Set up development environment.
2. Install BepInEx x86
3. Compile CrowdedMod youself and put it in BepInEx plugins folder


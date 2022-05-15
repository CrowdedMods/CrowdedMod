## Client mod installation
To make mod work, you need two pieces. Custom server ([Impostor](https://github.com/Impostor/Impostor)) and a client-side mod (CrowdedMod).
<br/>
After installing this mod, you need to also [add custom server to the client menu](https://github.com/CrowdedMods/CrowdedMod/tree/master/docs/ServerSelection.md).
### Method 1 (recommended)
1. Download latest [BepInEx](https://github.com/NuclearPowered/BepInEx/releases/download/6.0.0-reactor.18%2Bstructfix/BepInEx-6.0.0-reactor.18+structfix.zip)
2. Extract all files from zipped archive and put them in Among Us directory (where Among Us.exe is)
3. Download [latest Reactor.dll](https://github.com/NuclearPowered/Reactor/releases) and put it in `YourAmongUsDirectory/BepInEx/plugins` (you probably have to unzip it first)
4. Download right CrowdedMod dll from [CurseForge](https://www.curseforge.com/among-us/all-mods/crowdedmod/files) or [github releases](https://github.com/CrowdedMods/CrowdedMod/releases) and put it in `YourAmongUsDirectory/BepInEx/plugins` (you probably have to unzip it first)
5. The first launch will take a while, be patient
6. Enjoy!

> To make sure everything works correctly join local game (or any other except Freeplay) and you should see `CrowdedMod` right under your ping

### Method 2 (for security paranoid)
Just compile it yourself
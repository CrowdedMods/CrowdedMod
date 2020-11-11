# 100 Players Mod
This mod unlocks the possibility for more than 10 player to join in an Among Us lobby.
The official servers won't allow for it though, so you will have to host an [Impostor](https://github.com/AeonLucid/Impostor) custom server.

## Why 100 players?
This mod allows for up to 127 players (signed byte limit) because of game limitations (That's a lot of player anyway).
Hence the name itself derives from the [Impostor project](https://github.com/AeonLucid/Impostor) discord server 
discussion, in which "100 players mod when?" has become a meme.

## Limitations
By itself this mod doesn't allow to choose more than 10 players or more than 3 impostors in the game creation menu.
You will have to change those values by other means (e.g. Cheat Engine)

## Installation
1. Here on the right sidebar, choose the latest version supporting your game version (as indicated in parentheses).
2. Download the `.dll` and `.zip` files (The mod itself and the modloader respectively).
	- Alternatively you can download this repository and compile the `.dll` mod yourself.
	- You can also download the latest `.zip` of [BepInEx](https://github.com/BepInEx/BepInEx) IL2CPP version (You can download bleeding edge builds [here](https://builds.bepis.io/projects/bepinex_be) now)
3. Extract the modloader's `.zip` into the Among Us directory (where the `Among Us.exe` executable is located)
4. Navigate to the `BepInEx/plugins` subfolder (It should be there, you just extracted it)
5. Copy the mod's `.dll` file there.
6. Start Among Us and try it out.

## Credits
- [andruzzzhka/CustomServersClient](https://github.com/andruzzzhka/CustomServersClient) - This is where I learned how to mod Among Us

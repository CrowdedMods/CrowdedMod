## Server-side installation
> :warning: **DEPRECATION WARNING:**
> AllOfUsBot is outdated and currently - buggy. Current alternative is just using changed player count when creating a game (CrowdedMod feature).
> Color selection is also buggy because of latest Impostor anti-cheat. We're working on updated guide for latest Impostor.

Crowded Mod to work properly, needs custom Among Us server.<br/>
Latest Impostor version is incompatible with CrowdedMod. To clone last compatible version do `git checkout 2ebd804d2818b4a4b4aeee7f13325b9a50e6896e` after cloning Impostor repository. You can also download it from [build server](https://ci.appveyor.com/project/Impostor/Impostor/builds/36871832/artifacts)
### Installing custom Impostor server
See also [Impostor docs](https://github.com/Impostor/Impostor/blob/dev/docs/Running-the-server.md). <br/>
After installing Impostor, you need to install [AllOfUs plugin](https://github.com/XtraCube/AllOfUsBot):
- download latest dll from releases
- create `plugins` folder inside Impostor directory and put dll in it

using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using System.Linq;
using System.Reflection;
namespace CrowdedMod {
    [BepInPlugin("pl.przebor.crowded", "Crowded Mod", "3.4")]
    public class RemovePlayerLimitPlugin : BasePlugin {

        static internal BepInEx.Logging.ManualLogSource Logger;
        static Harmony _harmony;

        public override void Load() {
            Logger = Log;
            ServersParser.Parse();

            KMOGFLPJLLK.EICIGKMJIMF = KMOGFLPJLLK.MGGHFLMODBE = Enumerable.Repeat<int>(255, 255).ToArray<int>();
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }
}

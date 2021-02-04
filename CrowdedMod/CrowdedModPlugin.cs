using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using System.Linq;

namespace CrowdedMod {
    [BepInPlugin(Id, "Crowded Mod", Version)]
    public class CrowdedModPlugin : BasePlugin
    {
        const string Id = "pl.przebor.crowded";
        private const string Version = "2.0.1";
        
#if DEBUG
        // internal to use this in other classes (not required to release)
        static internal BepInEx.Logging.ManualLogSource Logger;
#endif
        static readonly Harmony Harmony = new Harmony(Id);

        public override void Load() {
#if DEBUG
            Logger = Log;
#endif

            // GameOptionsData.RecommendedImpostors
            // to avoid IndexOutOfRangeException(s)
            KMOGFLPJLLK.EICIGKMJIMF = KMOGFLPJLLK.MGGHFLMODBE = Enumerable.Repeat(15, 15).ToArray();
            KMOGFLPJLLK.GGJLPJPNONM = Enumerable.Repeat(4, 127).ToArray();
            Harmony.PatchAll();
        }
    }
}

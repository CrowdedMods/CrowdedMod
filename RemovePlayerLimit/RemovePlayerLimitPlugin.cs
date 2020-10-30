using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using System.Linq;
using System.Reflection;

namespace RemovePlayerLimit {
    [BepInPlugin("it.andry08.removeplayerlimit", "Player Limit Remove", "0.1")]
    public class RemovePlayerLimitPlugin : BasePlugin {

        static internal BepInEx.Logging.ManualLogSource Logger;
        static Harmony _harmony;

        public override void Load() {
            Logger = Log;

            OPIJAMILNFD.DECPEFPMMMF = OPIJAMILNFD.DKLBMBAFBAE = Enumerable.Repeat<int>(255, 255).ToArray<int>();
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }
}

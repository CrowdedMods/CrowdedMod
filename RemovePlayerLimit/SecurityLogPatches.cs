using HarmonyLib;
using SecurityLogger = MAOCFFOEGFE;

namespace CrowdedMod
{
    class SecurityLogPatches
    {
        [HarmonyPatch(typeof(SecurityLogger), nameof(SecurityLogger.Awake))]
        public static class SecurityLoggerPatch
        {
            public static void Postfix(ref SecurityLogger __instance)
            {
                __instance.KJGAJKDMDHJ = new float[127]; // Timers
            }
        }
    }
}

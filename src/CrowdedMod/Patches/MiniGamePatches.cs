using HarmonyLib;

namespace CrowdedMod.Patches
{
    internal static class MiniGamePatches
    {
        [HarmonyPatch(typeof(SecurityLogger), nameof(SecurityLogger.Awake))]
        public static class SecurityLoggerPatch
        {
            public static void Postfix(ref SecurityLogger __instance)
            {
                __instance.Timers = new float[127];
            }
        }
    }
}
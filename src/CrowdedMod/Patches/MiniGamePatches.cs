using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace CrowdedMod.Patches;

internal static class MiniGamePatches
{
    [HarmonyPatch(typeof(SecurityLogger), nameof(SecurityLogger.Awake))]
    public static class SecurityLoggerPatch
    {
        public static void Postfix(SecurityLogger __instance)
        {
            __instance.Timers = new Il2CppStructArray<float>(CrowdedModPlugin.MaxPlayers);
        }
    }
}
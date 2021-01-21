using HarmonyLib;
using KeyMinigame = AMKEIECODLC;
using PlayerControl = FFGALNAPKCD;
using SecurityLogger = MAOCFFOEGFE;

namespace CrowdedMod.Patches
{
    static class MiniGamePatches
    {
        [HarmonyPatch(typeof(SecurityLogger), nameof(SecurityLogger.Awake))]
        public static class SecurityLoggerPatch
        {
            public static void Postfix(ref SecurityLogger __instance)
            {
                __instance.KJGAJKDMDHJ = new float[127]; // Timers
            }
        }

        [HarmonyPatch(typeof(KeyMinigame),nameof(KeyMinigame.Start))]
        public static class KeyMinigamePatch
        {
            public static bool Prefix(ref KeyMinigame __instance)
            {
                var localPlayer = PlayerControl.LocalPlayer;
                __instance.ECHAPHLBHDC = (localPlayer != null) ? localPlayer.PlayerId % 10 : 0;
                __instance.Slots[__instance.ECHAPHLBHDC].CIAMDNLJPBO();
                return false;
            }
        }
    }
}
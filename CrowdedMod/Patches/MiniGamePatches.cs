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

        [HarmonyPatch(typeof(KeyMinigame),nameof(KeyMinigame.Start))]
        public static class KeyMinigamePatch
        {
            public static bool Prefix(ref KeyMinigame __instance)
            {
                var localPlayer = PlayerControl.LocalPlayer;
                __instance.targetSlotId = (localPlayer != null) ? localPlayer.PlayerId % 10 : 0;
                __instance.Slots[__instance.targetSlotId].Method_62(); // SetHighlight()
                return false;
            }
        }
    }
}
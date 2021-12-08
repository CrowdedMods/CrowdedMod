using HarmonyLib;

namespace CrowdedMod.Patches;

internal static class RoleOptionsPatches
{
    [HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.IncreaseCount))]
    public static class IncreaseCountPatch
    {
        public static bool Prefix(RoleOptionSetting __instance)
        {
            if (__instance.RoleMax != 15) return true;
            if (__instance.RoleMaxCount < sbyte.MaxValue)
            {
                __instance.RoleMaxCount += 1;
                __instance.OnValueChanged.Invoke(__instance);
            }
            
            __instance.ShowRoleDetails();
            return false;
        }
    }
    
    [HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.DecreaseCount))]
    public static class DecreaseCountPatch
    {
        public static bool Prefix(RoleOptionSetting __instance)
        {
            if (__instance.RoleMax != 15) return true;
            if (__instance.RoleMaxCount > 0)
            {
                __instance.RoleMaxCount -= 1;
                __instance.OnValueChanged.Invoke(__instance);
            }
            
            __instance.ShowRoleDetails();
            return false;
        }
    }
}
using System;
using System.Linq;
using CrowdedMod.Extensions;
using CrowdedMod.Net;
using HarmonyLib;
using Reactor.Networking;
using UnhollowerBaseLib;
using UnityEngine;

namespace CrowdedMod.Patches {
    internal static class GenericPatches {
        // patched because 10 is hardcoded in `for` loop
        [HarmonyPatch(typeof(GameData), nameof(GameData.GetAvailableId))]
        public static class GameDataAvailableIdPatch
        {
            public static bool Prefix(ref GameData __instance, out sbyte __result)
            {
                for (sbyte i = 0; i < sbyte.MaxValue; i++)
                {
                    if (CheckId(__instance, i))
                    {
                        __result = i;
                        return false;
                    }
                }
    
                __result = -1;
                return false;
            }
    
            private static bool CheckId(GameData __instance, int id)
            {
                return __instance.AllPlayers.ToArray().All(x => x.PlayerId != id);
            }
        }
    
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckColor))]
        public static class PlayerControlCmdCheckColorPatch
        {
            public static bool Prefix(PlayerControl __instance, byte bodyColor)
            {
                __instance.CustomRpcSetColor(bodyColor);
                return false;
            }
        }
    
        [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.UpdateAvailableColors))]
        public static class PlayerTabUpdateAvailableColorsPatch
        {
            public static bool Prefix(PlayerTab __instance)
            {
                if (__instance.HasLocalPlayer())
                {
                    __instance.PlayerPreview.HatSlot.SetColor(PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId);
                }

                for (var i = 0; i < Palette.PlayerColors.Length; i++)
                    __instance.AvailableColors.Add(i);
                    
                return false;
            }
        }
    
        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        public static class PingShowerPatch
        {
            public static void Postfix(PingTracker __instance)
            {
                __instance.text.text += "\n<color=#FFB793>> CrowdedMod <</color>";
            }
        }
    
        [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
        public static class GameSettingMenu_Start
        {
            private static void Prefix(ref GameSettingMenu __instance)
            {
                __instance.HideForOnline = Array.Empty<Transform>();
            }
        }
    
        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        public static class GameOptionsMenu_Start
        {
            private static void Postfix(ref GameOptionsMenu __instance)
            {
                __instance.GetComponentsInChildren<NumberOption>()
                    .First(o => o.Title == StringNames.GameNumImpostors)
                    .ValidRange = new FloatRange(1,
                    (int) (CreateGameOptionsPatches.CreateOptionsPicker_Start.maxPlayers - 0.5f) / 2);
            }
        }
    }
}

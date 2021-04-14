using System.Linq;
using CrowdedMod.Net;
using HarmonyLib;
using Reactor.Networking;
using UnhollowerBaseLib;
using UnityEngine;

namespace CrowdedMod.Patches {
    internal static class GenericPatches {
        // patched because 10 is hardcoded in `for` loop
        [HarmonyPatch(typeof(GameData), nameof(GameData.GetAvailableId))]
        public static class GameDataAvailableIdPatch {
            public static bool Prefix(ref GameData __instance, out sbyte __result) {
                for (sbyte i = 0; i <= 127; i++)
                    if (checkId(__instance, i)) {
                        __result = i;
                        return false;
                    }
                __result = -1;
                return false;
            }

            static bool checkId(GameData __instance, int id) {
                foreach (var p in __instance.AllPlayers)
                    if (p.PlayerId == id)
                        return false;
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckColor))]
        public static class PlayerControlCmdCheckColorPatch {
            public static bool Prefix([HarmonyArgument(0)] byte colorId) {
                Rpc<SetColorRpc>.Instance.Send(colorId);
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.UpdateAvailableColors))]
        public static class PlayerTabUpdateAvailableColorsPatch {
            public static bool Prefix(PlayerTab __instance) {
                PlayerControl.SetPlayerMaterialColors(PlayerControl.LocalPlayer._cachedData.ColorId, __instance.DemoImage);
                for (int i = 0; i < Palette.PlayerColors.Length; i++)
                    __instance.AvailableColors.Add(i);
                return false;
            }
        }
            
        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        public static class PingShowerPatch
        {
            public static void Postfix(PingTracker __instance)
            {
                __instance.text.autoSizeTextContainer = true; // 12.4s why?
                __instance.text.text += "\n[FFB793FF]> CrowdedMod <[]";
            }
        }

        [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.OnEnable))]
        public static class GameSettingMenu_OnEnable // Credits to https://github.com/Galster-dev/GameSettingsUnlocker
        {
            static void Prefix(ref GameSettingMenu __instance)
            {
                __instance.HideForOnline = new Il2CppReferenceArray<Transform>(0);
            }
        }

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        public static class GameOptionsMenu_Start
        {
            static void Postfix(ref GameOptionsMenu __instance)
            {
                __instance.GetComponentsInChildren<NumberOption>()
                    .First(o => o.Title == StringNames.GameNumImpostors)
                    .ValidRange = new FloatRange(1, (int)(CreateGameOptionsPatches.CreateOptionsPicker_Start.maxPlayers-0.5f)/2);
            }
        }
    }
}

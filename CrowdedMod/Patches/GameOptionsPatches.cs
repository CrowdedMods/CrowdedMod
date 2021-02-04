using System;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using UnhollowerBaseLib;

using GameOptionsMenu = PHCKLDDNJNP;
using GameOptionsData = KMOGFLPJLLK;
using GameSettingsMenu = JCLABFFHPEO;
using NumberOption = PCGDGFIAJJI;
using Object = UnityEngine.Object;
using OptionBehaviour = LLKOLCLGCBD;
using TranslationController = GIGNEFLFPDE;
using PlayerControl = FFGALNAPKCD;
using AmongUsClient = FMLLKEACGIO;
using GameStartManger = PPAEIPHJPDH<ANKMIOIMNFE>;

namespace CrowdedMod.Patches
{
    public static class GameOptionsPatches
    {
        private const StringNames PlayerAmountTitle = (StringNames) 412;

        static float GetLowestConfigY(this GameOptionsMenu instance) // credits to Herysia
        {
            return instance.GetComponentsInChildren<OptionBehaviour>()
                .Min(option => option.transform.localPosition.y);
        }
        
        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString))]
        [HarmonyPatch(new [] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
        static class TranslationController_GetString
        {
            public static bool Prefix([HarmonyArgument(0)] StringNames name, ref string __result)
            {
                if (name == PlayerAmountTitle)
                {
                    __result = "Player Count";
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        static class GameOptionsMenu_Start
        {
            static void OnValueChanged(OptionBehaviour option)
            {
                if (!AmongUsClient.Instance || !AmongUsClient.Instance.BEIEANEKAFC || !PlayerControl.LocalPlayer) return;
                
                PlayerControl.GameOptions.NCJGOCGPJDO = option.GetInt(); // maxPlayers
                PlayerControl.LocalPlayer.RpcSyncSettings(PlayerControl.GameOptions);
                GameStartManger.IAINKLDJAGC.LGOBNECPLBJ = -1; // Instance.LastPlayerCount
            }
            static void Postfix(ref GameOptionsMenu __instance)
            {
                var countOption = Object.Instantiate(__instance.GetComponentInChildren<NumberOption>(), __instance.transform);
                countOption.transform.localPosition = new Vector3(
                    __instance.transform.localPosition.x,
                    __instance.GetLowestConfigY() - 0.5f,
                    __instance.transform.localPosition.z
                );
                countOption.Title = PlayerAmountTitle;
                countOption.Value = PlayerControl.GameOptions.NCJGOCGPJDO; // MaxPlayers
                var str = "";
                TranslationController_GetString.Prefix(countOption.Title, ref str);
                countOption.TitleText.Text = str;
                countOption.ValidRange = new FloatRange(4f, 15f);
                countOption.OnValueChanged = new Action<OptionBehaviour>(OnValueChanged);
                countOption.gameObject.AddComponent<OptionBehaviour>();
                countOption.enabled = true;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        static class PlayerControl_HandleRpc
        {
            static void Prefix([HarmonyArgument(0)] byte callId)
            {
                if (callId == 2) // SyncSettings
                {
                    GameStartManger.IAINKLDJAGC.LGOBNECPLBJ = -1; // Instance.LastPlayerCount
                }
            }
        }
    }
}
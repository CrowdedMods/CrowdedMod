using System.Linq;
using HarmonyLib;
using UnhollowerBaseLib;
using UnityEngine;

namespace CrowdedMod.Patches {
    internal static class GenericPatches {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckColor))]
        public static class PlayerControlCheckColorPatch {
            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte colorId) {
                __instance.RpcSetColor(colorId);
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.IsSelectedItemEquipped))]
        public static class PlayerTabIsSelectedItemEquippedPatch {
            public static bool Prefix(out bool __result)
            {
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public static class GameStartManagerUpdatePatch
        {
            private static string? fixDummyCounterColor;
            public static void Prefix(GameStartManager __instance)
            {
                if (GameData.Instance != null && __instance.LastPlayerCount != GameData.Instance.PlayerCount)
                {
                    if (__instance.LastPlayerCount > __instance.MinPlayers)
                    {
                        fixDummyCounterColor = "<color=#00FF00FF>";
                    } else if (__instance.LastPlayerCount == __instance.MinPlayers)
                    {
                        fixDummyCounterColor = "<color=#FFFF00FF>";
                    }
                    else
                    {
                        fixDummyCounterColor = "<color=#FF0000FF>";
                    }
                }
            }

            public static void Postfix(GameStartManager __instance)
            {
                if (fixDummyCounterColor != null)
                {
                    __instance.PlayerCounter.text = $"{fixDummyCounterColor}{GameData.Instance.PlayerCount}/{PlayerControl.GameOptions.MaxPlayers}";
                    fixDummyCounterColor = null;
                }
            }
        }
            
        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        public static class PingShowerPatch
        {
            public static void Postfix(PingTracker __instance)
            {
                // __instance.text.autoSizeTextContainer = true; // 12.4s why?
                __instance.text.text += "\n<color=#FFB793> CrowdedMod </color>";
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

        // Will be patched with signatures later when BepInEx reveals it
        // [HarmonyPatch(typeof(InnerNetServer), nameof(InnerNetServer.HandleNewGameJoin))]
        // public static class InnerNetSerer_HandleNewGameJoin
        // {
        //     public static bool Prefix(InnerNetServer __instance, [HarmonyArgument(0)] InnerNetServer.Player client)
        //     {
        //         if (__instance.Clients.Count >= 15)
        //         {
        //             __instance.Clients.Add(client);
        //
        //             client.LimboState = LimboStates.PreSpawn;
        //             if (__instance.HostId == -1)
        //             {
        //                 __instance.HostId = __instance.Clients.ToArray()[0].Id;
        //
        //                 if (__instance.HostId == client.Id)
        //                 {
        //                     client.LimboState = LimboStates.NotLimbo;
        //                 }
        //             }
        //
        //             var writer = MessageWriter.Get(SendOption.Reliable);
        //             try
        //             {
        //                 __instance.WriteJoinedMessage(client, writer, true);
        //                 client.Connection.Send(writer);
        //                 __instance.BroadcastJoinMessage(client, writer);
        //             }
        //             catch (Il2CppException exception)
        //             {
        //                 Debug.LogError("[CM] InnerNetServer::HandleNewGameJoin MessageWriter 2 Exception: " +
        //                                exception.Message);
        //                 // ama too stupid for this 
        //                 // Debug.LogException(exception.InnerException, __instance);
        //             }
        //             finally
        //             {
        //                 writer.Recycle();
        //             }
        //
        //             return false;
        //         }
        //
        //         return true;
        //     }
        // }

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

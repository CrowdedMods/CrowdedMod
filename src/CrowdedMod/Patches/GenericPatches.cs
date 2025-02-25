using System.Linq;
using AmongUs.GameOptions;
using CrowdedMod.Net;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Reflection;
using InnerNet;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using UnityEngine;

namespace CrowdedMod.Patches;

internal static class GenericPatches
{
    private static bool ShouldDisableColorPatch => CrowdedModPlugin.ForceDisableFreeColor ||
                                                   GameData.Instance.PlayerCount <= Palette.PlayerColors.Length;

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckColor))]
    public static class PlayerControlCmdCheckColorPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte colorId)
        {
            if (ShouldDisableColorPatch)
            {
                return true;
            }

            Rpc<SetColorRpc>.Instance.Send(__instance, colorId);
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.Update))]
    public static class PlayerTabIsSelectedItemEquippedPatch
    {
        public static void Postfix(PlayerTab __instance)
        {
            if (ShouldDisableColorPatch)
            {
                return;
            }

            __instance.currentColorIsEquipped = false;
        }
    }

    [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.UpdateAvailableColors))]
    public static class PlayerTabUpdateAvailableColorsPatch
    {
        public static bool Prefix(PlayerTab __instance)
        {
            if (ShouldDisableColorPatch)
            {
                return true;
            }

            __instance.AvailableColors.Clear();
            for (var i = 0; i < Palette.PlayerColors.Count; i++)
            {
                if (!PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.CurrentOutfit.ColorId != i)
                {
                    __instance.AvailableColors.Add(i);
                }
            }

            return false;
        }
    }

    // I did not find a use of this method, but still patching for future updates
    // maxExpectedPlayers is unknown, looks like server code tbh
    [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.AreInvalid))]
    public static class InvalidOptionsPatches
    {
        public static bool Prefix(GameOptionsData __instance, [HarmonyArgument(0)] int maxExpectedPlayers)
        {
            return __instance.MaxPlayers > maxExpectedPlayers ||
                   __instance.NumImpostors < 1 ||
                   __instance.NumImpostors + 1 > maxExpectedPlayers / 2 ||
                   __instance.KillDistance is < 0 or > 2 ||
                   __instance.PlayerSpeedMod is <= 0f or > 3f;
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public static class GameStartManagerUpdatePatch
    {
        private static string fixDummyCounterColor = string.Empty;

        public static void Prefix(GameStartManager __instance)
        {
            if (GameData.Instance == null || __instance.LastPlayerCount == GameData.Instance.PlayerCount)
            {
                return;
            }

            if (__instance.LastPlayerCount > __instance.MinPlayers)
            {
                fixDummyCounterColor = "<color=#00FF00FF>";
            }
            else if (__instance.LastPlayerCount == __instance.MinPlayers)
            {
                fixDummyCounterColor = "<color=#FFFF00FF>";
            }
            else
            {
                fixDummyCounterColor = "<color=#FF0000FF>";
            }
        }

        public static void Postfix(GameStartManager __instance)
        {
            if (string.IsNullOrEmpty(fixDummyCounterColor) ||
                GameData.Instance == null ||
                GameManager.Instance?.LogicOptions == null)
            {
                return;
            }

            __instance.PlayerCounter.text =
                $"{fixDummyCounterColor}{GameData.Instance.PlayerCount}/{GameManager.Instance.LogicOptions.MaxPlayers}";
            fixDummyCounterColor = string.Empty;
        }
    }

    [HarmonyPatch(typeof(InnerNetServer), nameof(InnerNetServer.HandleNewGameJoin))]
    public static class InnerNetSerer_HandleNewGameJoin
    {
        public static bool Prefix(InnerNetServer __instance, [HarmonyArgument(0)] InnerNetServer.Player client)
        {
            if (__instance.Clients.Count is < 15 or >= CrowdedModPlugin.MaxPlayers) return true;

            __instance.Clients.Add(client);

            client.LimboState = LimboStates.PreSpawn;
            if (__instance.HostId == -1)
            {
                __instance.HostId = __instance.Clients.ToArray()[0].Id;
            }

            if (__instance.HostId == client.Id)
            {
                client.LimboState = LimboStates.NotLimbo;
            }

            var writer = MessageWriter.Get(SendOption.Reliable);
            try
            {
                __instance.WriteJoinedMessage(client, writer, true);
                client.Connection.Send(writer);
                __instance.BroadcastJoinMessage(client, writer);
            }
            catch (Il2CppException exception)
            {
                Debug.LogError("[CM] InnerNetServer::HandleNewGameJoin MessageWriter 2 Exception: " +
                               exception.Message);
                // Debug.LogException(exception, __instance);
            }
            finally
            {
                writer.Recycle();
            }

            return false;
        }
    }

    private static void TryAdjustOptionsRecommendations(GameOptionsManager manager)
    {
        const int MaxPlayers = CrowdedModPlugin.MaxPlayers;
        var type = manager.GetGameOptions();
        var options = manager.GameHostOptions.Cast<Il2CppSystem.Object>();

        var maxRecommendation = ((Il2CppStructArray<int>)Enumerable.Repeat(MaxPlayers, MaxPlayers + 1).ToArray())
            .Cast<Il2CppSystem.Object>();
        var minRecommendation = ((Il2CppStructArray<int>)Enumerable.Repeat(4, MaxPlayers + 1).ToArray())
            .Cast<Il2CppSystem.Object>();
        var killRecommendation = ((Il2CppStructArray<int>)Enumerable.Repeat(0, MaxPlayers + 1).ToArray())
            .Cast<Il2CppSystem.Object>();


        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
        // all these fields are currently static, but we're doing a forward compat
        // static fields ignore object param so non-null instance is ok
        type.GetField("RecommendedImpostors", flags)?.SetValue(options, maxRecommendation);
        type.GetField("MaxImpostors", flags)?.SetValue(options, maxRecommendation);
        type.GetField("RecommendedKillCooldown", flags)?.SetValue(options, killRecommendation);
        type.GetField("MinPlayers", flags)?.SetValue(options, minRecommendation);
    }

    [HarmonyPatch(typeof(GameOptionsManager), nameof(GameOptionsManager.GameHostOptions), MethodType.Setter)]
    public static class GameOptionsManager_set_GameHostOptions
    {

        public static void Postfix(GameOptionsManager __instance)
        {
            try
            {
                TryAdjustOptionsRecommendations(__instance);
            }
            catch (System.Exception e)
            {
                Logger<CrowdedModPlugin>.Error($"Failed to adjust options recommendations: {e}");
            }
        }
    }

    [HarmonyPatch(typeof(GameOptionsManager), nameof(GameOptionsManager.SwitchGameMode))]
    public static class GameOptionsManager_SwitchGameMode
    {
        public static void Postfix(GameOptionsManager __instance)
        {
            try
            {
                TryAdjustOptionsRecommendations(__instance);
            }
            catch (System.Exception e)
            {
                Logger<CrowdedModPlugin>.Error($"Failed to adjust options recommendations: {e}");
            }
        }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.Awake))]
    public static class GameManager_Awake
    {
        public static void Postfix(GameManager __instance)
        {
            foreach (var category in __instance.GameSettingsList.AllCategories)
            {
                foreach (var option in category.AllGameSettings)
                {
                    if (option is IntGameSetting intOption && option.Title == StringNames.GameNumImpostors)
                    {
                        intOption.ValidRange = new IntRange(1, CrowdedModPlugin.MaxImpostors);
                        return;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class RemoveVanillaServerPatch
    {
        public static void Postfix()
        {
            CrowdedModPlugin.RemoveVanillaServer();
        }
    }
}
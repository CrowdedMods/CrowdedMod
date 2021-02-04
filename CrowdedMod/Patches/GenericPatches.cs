using System.Linq;
using HarmonyLib;
using UnhollowerBaseLib;
using UnityEngine;

using PlayerControl = FFGALNAPKCD;
using PlayerTab = MAOILGPNFND;
using GameData = EGLJNOMOGNP;
using Palette = LOCPGOACAJF;
using PingTracker = ELDIDNABIPI;
using ShipStatus = HLBNNHFCNAJ;
using GameSettingMenu = JCLABFFHPEO;
using GameOptionsMenu = PHCKLDDNJNP;
using NumberOption = PCGDGFIAJJI;

namespace CrowdedMod.Patches {
	static class GenericPatches {
		// patched because 10 is hardcoded in `for` loop
        [HarmonyPatch(typeof(GameData), nameof(GameData.GetAvailableId))]
		static class GameDataAvailableIdPatch {
			public static bool Prefix(ref GameData __instance, out sbyte __result) {
				for (sbyte i = 0; i < 15; i++)
					if (checkId(__instance, i)) {
						__result = i;
						return false;
					}
				__result = -1;
				return false;
			}

			static bool checkId(GameData __instance, int id) {
				foreach (var p in __instance.AllPlayers)
					if (p.JKOMCOJCAID == id)
						return false;
				return true;
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckColor), typeof(byte))]
		static class PlayerControlCheckColorPatch {
			public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte colorId) {
				__instance.RpcSetColor(colorId);
				return false;
			}
		}

		[HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.UpdateAvailableColors))]
		static class PlayerTabUpdateAvailableColorsPatch {
			public static bool Prefix(PlayerTab __instance) {
				PlayerControl.SetPlayerMaterialColors(PlayerControl.LocalPlayer.NDGFFHMFGIG.EHAHBDFODKC, __instance.DemoImage);
				for (int i = 0; i < Palette.OPKIKLENHFA.Length; i++)
					__instance.LGAIKONLBIG.Add(i);
				return false;
			}
		}
		
		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.GetSpawnLocation))]
        	public static class ShipStatusGetSpawnLocationPatch
		{
		    public static void Prefix(ShipStatus __instance, [HarmonyArgument(0)] ref int playerId, [HarmonyArgument(1)] ref int numPlayer)
		    {
			playerId %= 10;
			if (numPlayer > 10) numPlayer = 10;
		    }
		}
	
        [HarmonyPriority(Priority.VeryHigh)] // to show this message first, or be overrided if any plugins do
        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        static class PingShowerPatch
        {
            public static void Postfix(PingTracker __instance)
            {
                __instance.text.Text += "\n[FFB793FF]> CrowdedMod <";
            }
        }

        [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.OnEnable))]
        static class GameSettingMenu_OnEnable // Credits to https://github.com/Galster-dev/GameSettingsUnlocker
        {
	        static void Prefix(ref GameSettingMenu __instance)
	        {
		        __instance.HideForOnline = new Il2CppReferenceArray<Transform>(0);
	        }
        }

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        static class GameOptionsMenu_Start
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

using HarmonyLib;

namespace RemovePlayerLimit {
	class GenericPatches {
		static sbyte UniquePlayerId = 0;
		
		[HarmonyPatch(typeof(GameData), nameof(GameData.GetAvailableId))]
		public static class GameDataAvailableIdPatch {
			public static bool Prefix(ref GameData __instance, ref sbyte __result) {
				//RemovePlayerLimitPlugin.Logger.LogInfo("Assigned unique player id: " + UniquePlayerId);
				__result = UniquePlayerId++;
				//TODO: check other players
				return false;
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckColor), typeof(byte))]
		public static class PlayerControlCheckColorPatch {
			public static bool Prefix(PlayerControl __instance, byte GBOFPFOGPJO) {
				__instance.RpcSetColor(GBOFPFOGPJO);
				return false;
			}
		}

		[HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.JBBOGJHCEOI))]
		public static class PlayerTabSelectColorPatch {
			public static void Postfix(PlayerTab __instance) {
				__instance.UpdateAvailableColors();
			}
		}

		[HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.UpdateAvailableColors))]
		public static class PlayerTabUpdateAvailableColorsPatch {
			public static bool Prefix(PlayerTab __instance) {
				PlayerControl.SetPlayerMaterialColors(PlayerControl.LocalPlayer.FIPOLMBOBHM.LHKAPPDILFP, __instance.DemoImage);
				for (int i = 0; i < KMGFBENDNFO.FOJPMGJFKMB.Length; i++)
					__instance.FKAFOGKGPLM.Add(i);
				return false;
			}
		}
	}
}

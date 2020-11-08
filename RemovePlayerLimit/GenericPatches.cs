using HarmonyLib;

namespace RemovePlayerLimit {
	class GenericPatches {
		//static sbyte UniquePlayerId = 0;

		[HarmonyPatch(typeof(GameData), nameof(GameData.GetAvailableId))]
		public static class GameDataAvailableIdPatch {
			public static bool Prefix(ref GameData __instance, ref sbyte __result) {
				//__result = UniquePlayerId++;
				for (int i = 0; i < 128; i++)
					if (checkId(__instance, i)) {
						__result = (sbyte)i;
						return false;
					}
				__result = -1;
				return false;
			}

			static bool checkId(GameData __instance, int id) {
				foreach (GameData.IHEKEPMDGIJ p in __instance.AllPlayers)
					if (p.FIOIBHIDDOC == id)
						return false;
				return true;
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckColor), typeof(byte))]
		public static class PlayerControlCheckColorPatch {
			public static bool Prefix(PlayerControl __instance, byte GBOFPFOGPJO) {
				__instance.RpcSetColor(GBOFPFOGPJO);
				return false;
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

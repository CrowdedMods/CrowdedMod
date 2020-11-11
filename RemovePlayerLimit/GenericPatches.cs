using HarmonyLib;

namespace RemovePlayerLimit {
	class GenericPatches {
		//static sbyte UniquePlayerId = 0;

		[HarmonyPatch(typeof(BAGGGBBOHOH), nameof(BAGGGBBOHOH.GetAvailableId))]
		public static class GameDataAvailableIdPatch {
			public static bool Prefix(ref BAGGGBBOHOH __instance, ref sbyte __result) {
				//__result = UniquePlayerId++;
				for (int i = 0; i < 128; i++)
					if (checkId(__instance, i)) {
						__result = (sbyte)i;
						return false;
					}
				__result = -1;
				return false;
			}

			static bool checkId(BAGGGBBOHOH __instance, int id) {
				foreach (BAGGGBBOHOH.FGMBFCIIILC p in __instance.AllPlayers)
					if (p.PAGHECLPIMH == id)
						return false;
				return true;
			}
		}

		[HarmonyPatch(typeof(GLHCHLEDNBA), nameof(GLHCHLEDNBA.CheckColor), typeof(byte))]
		public static class PlayerControlCheckColorPatch {
			public static bool Prefix(GLHCHLEDNBA __instance, byte AGLNDGIHLPG) {
				__instance.RpcSetColor(AGLNDGIHLPG);
				return false;
			}
		}

		[HarmonyPatch(typeof(DICIDCLJJFH), nameof(DICIDCLJJFH.UpdateAvailableColors))]
		public static class PlayerTabUpdateAvailableColorsPatch {
			public static bool Prefix(DICIDCLJJFH __instance) {
				GLHCHLEDNBA.SetPlayerMaterialColors(GLHCHLEDNBA.LocalPlayer.HMPLOOHMKEN.LMDCNHODEAN, __instance.DemoImage);
				for (int i = 0; i < KPNJLIGHOEI.FLKMIOFABCO.Length; i++)
					__instance.JBGOCGMNPBP.Add(i);
				return false;
			}
		}
	}
}

using UnityEngine;
using HarmonyLib;
using System.Linq;
using Hazel;

using PlayerControl = FFGALNAPKCD;
using MeetingHud = OOCJALPKPEP;
using PlayerVoteArea = HDJGDMFCHDN;
using Palette = LOCPGOACAJF;
using GameData = EGLJNOMOGNP;
using PlayerInfo = EGLJNOMOGNP.DCJMABDDJCF;
using Effects = MFGGDFBIKLF;
using AmongUsClient = FMLLKEACGIO;

namespace CrowdedMod.Patches {
	static class MeetingHudPatches {
		static string lastTimerText;
		static int currentPage = 0;
		static int maxPages => (int)Mathf.Ceil(GameData.Instance.AllPlayers.Count / 10f);

		[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
		static class VoteGuiPatch {
			public static void Postfix(MeetingHud __instance) {
				if (Input.GetKeyDown(KeyCode.UpArrow) || Input.mouseScrollDelta.y > 0f)
					currentPage = Mathf.Clamp(currentPage - 1, 0, maxPages - 1);
				else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.mouseScrollDelta.y < 0f)
					currentPage = Mathf.Clamp(currentPage + 1, 0, maxPages - 1);

				if (__instance.TimerText.Text != lastTimerText)
					__instance.TimerText.Text = lastTimerText = __instance.TimerText.Text + $" ({currentPage + 1}/{maxPages})";

				PlayerVoteArea[] playerButtons = __instance.HBDFFAHBIGI.OrderBy(x => x.isDead).ToArray();
				int i = 0;
				foreach (PlayerVoteArea button in playerButtons) {
					if (i >= currentPage * 10 && i < (currentPage + 1) * 10) {
						button.gameObject.SetActive(true);

						int relativeIndex = i % 10;
						button.transform.localPosition = __instance.VoteOrigin +
						                                 new Vector3(
							                                 __instance.VoteButtonOffsets.x * (relativeIndex % 2),
							                                 __instance.VoteButtonOffsets.y * (relativeIndex / 2), 
							                                 -1f
							                             );
					} else
						button.gameObject.SetActive(false);
					i++;
				}
			}
		}
	}
}

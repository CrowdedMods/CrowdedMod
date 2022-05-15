using UnityEngine;
using HarmonyLib;
using System.Linq;

namespace CrowdedMod.Patches {
    internal static class MeetingHudPatches 
    {
        private static string lastTimerText = "";
        private static int currentPage;
        private static int maxPages => (int)Mathf.Ceil(GameData.Instance.AllPlayers.Count / 15f);

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        public static class VoteGuiPatch {
            public static void Postfix(MeetingHud __instance) {
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.mouseScrollDelta.y > 0f)
                    currentPage = Mathf.Clamp(currentPage - 1, 0, maxPages - 1);
                else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.mouseScrollDelta.y < 0f)
                    currentPage = Mathf.Clamp(currentPage + 1, 0, maxPages - 1);

                if (__instance.TimerText.text != lastTimerText)
                    __instance.TimerText.text = lastTimerText = __instance.TimerText.text + $" ({currentPage + 1}/{maxPages})";

                PlayerVoteArea[] playerButtons = __instance.playerStates.OrderBy(x => x.AmDead).ToArray();
                int i = 0;
                foreach (PlayerVoteArea button in playerButtons) {
                    if (i >= currentPage * 15 && i < (currentPage + 1) * 15) {
                        button.gameObject.SetActive(true);

                        int relativeIndex = i % 15;
                        button.transform.localPosition = __instance.VoteOrigin +
                                                         new Vector3(
                                                             __instance.VoteButtonOffsets.x * (relativeIndex % 3),
                                                             __instance.VoteButtonOffsets.y * (relativeIndex / 3), 
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

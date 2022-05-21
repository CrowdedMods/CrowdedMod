using UnityEngine;
using HarmonyLib;
using System.Linq;

namespace CrowdedMod.Patches {
    internal static class MeetingHudPatches 
    {
        private static string lastTimerText = "";
        private static int currentPage;
        private const int maxPerPage = 15;
        private static int maxPages => (int)Mathf.Ceil(GameData.Instance.AllPlayers.Count / (float)maxPerPage);

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        public static class VoteGuiPatch {
            public static void Postfix(MeetingHud __instance) {
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.mouseScrollDelta.y > 0f)
                    currentPage = Mathf.Clamp(currentPage - 1, 0, maxPages - 1);
                else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.mouseScrollDelta.y < 0f)
                    currentPage = Mathf.Clamp(currentPage + 1, 0, maxPages - 1);

                if (__instance.TimerText.text != lastTimerText)
                    __instance.TimerText.text = lastTimerText = __instance.TimerText.text + $" ({currentPage + 1}/{maxPages})";

                var playerButtons = __instance.playerStates.OrderBy(x => x.AmDead).ToArray();
                var i = 0;
                foreach (var button in playerButtons) {
                    if (i >= currentPage * maxPerPage && i < (currentPage + 1) * maxPerPage) {
                        button.gameObject.SetActive(true);

                        var relativeIndex = i % maxPerPage;
                        var row = relativeIndex / 3;
                        button.transform.localPosition = __instance.VoteOrigin +
                                                         new Vector3(
                                                             __instance.VoteButtonOffsets.x * (relativeIndex % 3),
                                                             __instance.VoteButtonOffsets.y * row, 
                                                             -0.9f - row * 0.01f
                                                         );
                    } else
                        button.gameObject.SetActive(false);
                    i++;
                }
            }
        }
    }
}

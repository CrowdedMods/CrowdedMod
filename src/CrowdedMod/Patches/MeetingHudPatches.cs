using UnityEngine;
using HarmonyLib;
using System.Linq;
using CrowdedMod.Net;
using Reactor.Networking;

namespace CrowdedMod.Patches {
    internal static class MeetingHudPatches 
    {
        private static string lastTimerText;
        private static int currentPage;
        private static int maxPages => (int)Mathf.Ceil(GameData.Instance.AllPlayers.Count / 10f);

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        public static class VoteGuiPatch {
            public static void Postfix(MeetingHud __instance) {
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.mouseScrollDelta.y > 0f)
                    currentPage = Mathf.Clamp(currentPage - 1, 0, maxPages - 1);
                else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.mouseScrollDelta.y < 0f)
                    currentPage = Mathf.Clamp(currentPage + 1, 0, maxPages - 1);

                if (__instance.TimerText.text != lastTimerText)
                    __instance.TimerText.text = lastTimerText = __instance.TimerText.text + $" ({currentPage + 1}/{maxPages})";

                PlayerVoteArea[] playerButtons = __instance.playerStates.OrderBy(x => x.isDead).ToArray();
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

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
        public static class MeetingHudCheckForEndVotingPatch {
            public static bool Prefix(MeetingHud __instance) {
                if (!__instance.playerStates.All(ps => ps.isDead || ps.didVote)) return false;
                byte[] self = calculateVotes(__instance.playerStates);

                int maxIdx = indexOfMax(self) - 1;
                GameData.PlayerInfo exiled = GameData.Instance.GetPlayerById((byte)maxIdx);
                byte[] states = __instance.playerStates.Select(s => s.GetState()).ToArray();
                byte[] votes = __instance.playerStates.Select(s => (byte)s.votedFor).ToArray();

                Rpc<VotingCompleteRpc>.Instance.Send(__instance, new VotingCompleteRpc.Data (
                        states,
                        votes,
                        exiled?.PlayerId ?? byte.MaxValue
                    ));
                return false;
            }

            private static byte[] calculateVotes(PlayerVoteArea[] states) {
                byte[] self = new byte[states.Length + 1];
                foreach (var playerVoteArea in states)
                {
                    if (playerVoteArea.didVote &&
                        playerVoteArea.votedFor + 1 >= 0 &&
                        playerVoteArea.votedFor + 1 < self.Length)
                    {
                        self[playerVoteArea.votedFor + 1]++;
                    }
                }
                return self;
            }
    
            private static int indexOfMax(byte[] array) {
                int result = -1;
                int maxNum = int.MinValue;
                for (int i = 0; i < array.Length; i++) {
                    int curNum = array[i];
                    if (curNum > maxNum) {
                        result = i;
                        maxNum = curNum;
                    } else if (curNum == maxNum) {
                        result = -1;
                    }
                }
                return result;
            }
        }
    }
}

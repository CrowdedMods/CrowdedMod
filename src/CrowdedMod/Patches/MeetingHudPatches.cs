using UnityEngine;
using HarmonyLib;
using System.Linq;
using Hazel;

namespace CrowdedMod.Patches {
    internal static class MeetingHudPatches 
    {
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
                    __instance.TimerText.Text = (lastTimerText = __instance.TimerText.Text + $" ({currentPage + 1}/{maxPages})");

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

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Method_76))] // CheckForEndVoting()
        static class MeetingHudCheckForEndVotingPatch {
            public static bool Prefix(MeetingHud __instance) {
                if (!__instance.playerStates.All(ps => ps.isDead || ps.didVote)) return false;
                byte[] self = calculateVotes(__instance.playerStates);

                int maxIdx = indexOfMax(self, out bool tie) - 1;
                GameData.PlayerInfo exiled = GameData.Instance.GetPlayerById((byte)maxIdx);
                byte[] states = __instance.playerStates.Select(s => s.GetState()).ToArray();
                byte[] votes = __instance.playerStates.Select(s => (byte)s.votedFor).ToArray();
                if (AmongUsClient.Instance.AmHost)
                {
                    MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(__instance.NetId, 23, SendOption.Reliable);
                    messageWriter.WriteBytesAndSize(states);
                    messageWriter.WriteBytesAndSize(votes); //Added because of the state's 4 bit vote id limit
                    messageWriter.Write(exiled?.PlayerId ?? byte.MaxValue);
                    messageWriter.Write(tie);
                    messageWriter.EndMessage();
                }

                MeetingHudHandleRpcPatch.VotingComplete(__instance, states, votes, exiled, tie);
                return false;
            }

            static byte[] calculateVotes(PlayerVoteArea[] states) {
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
    
            static int indexOfMax(byte[] array, out bool tie) {
                tie = false;
                int result = -1;
                int maxNum = int.MinValue;
                for (int i = 0; i < array.Length; i++) {
                    int curNum = array[i];
                    if (curNum > maxNum) {
                        result = i;
                        maxNum = curNum;
                        tie = false;
                    } else if (curNum == maxNum) {
                        result = -1;
                        tie = true;
                    }
                }
                return result;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.HandleRpc), typeof(byte), typeof(MessageReader))]
        static class MeetingHudHandleRpcPatch {
            static bool Prefix(MeetingHud __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader) {
                if (callId == 23)
                {
                    byte[] states = reader.ReadBytesAndSize();
                    byte[] votes = reader.ReadBytesAndSize();
                    GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(reader.ReadByte());
                    bool tie = reader.ReadBoolean();
                    VotingComplete(__instance, states, votes, playerById, tie);
                    return false;
                }
                return true;
            }

            public static void VotingComplete(MeetingHud __instance, byte[] states, byte[] votes, GameData.PlayerInfo exiled, bool tie) {
                if (__instance.state == MeetingHud.VoteStates.Results) {
                    return;
                }
                __instance.state = MeetingHud.VoteStates.Results;
                __instance.resultsStartedAt = __instance.discussionTimer;
                __instance.exiledPlayer = exiled;
                __instance.wasTie = tie;
                __instance.SkipVoteButton.gameObject.SetActive(false);
                __instance.SkippedVoting.gameObject.SetActive(true);

                PopulateResults(__instance, states, votes);
                __instance.Method_23(); // SetupProceedButton
            }

            static void PopulateResults(MeetingHud __instance, byte[] states, byte[] votes) {
                __instance.TitleText.Text = "Voting Results";
                int num = 0;
                for (int i = 0; i < __instance.playerStates.Length; i++) {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    playerVoteArea.ClearForResults();
                    int num2 = 0;
                    for (int j = 0; j < states.Length; j++) {
                        if ((states[j] & 128) == 0) { //!isDead
                            GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById((byte)__instance.playerStates[j].TargetPlayerId);
                            int votedFor = (sbyte)votes[j];

                            SpriteRenderer spriteRenderer = Object.Instantiate(__instance.PlayerVotePrefab);
                            if (PlayerControl.GameOptions.AnonymousVotes)
                                PlayerControl.SetPlayerMaterialColors(Palette.DisabledGrey, spriteRenderer);
                            else
                                PlayerControl.SetPlayerMaterialColors(playerById.ColorId, spriteRenderer);
                            spriteRenderer.transform.localScale = Vector3.zero;

                            if (playerVoteArea.TargetPlayerId == votedFor) {
                                spriteRenderer.transform.SetParent(playerVoteArea.transform);
                                spriteRenderer.transform.localPosition = __instance.CounterOrigin + new Vector3(__instance.CounterOffsets.x * num2, 0f, 0f);
                                __instance.StartCoroutine(Effects.Bloop(num2 * 0.5f, spriteRenderer.transform, 1f, 0.5f));
                                num2++;
                            } else if (i == 0 && votedFor == -1) {
                                spriteRenderer.transform.SetParent(__instance.SkippedVoting.transform);
                                spriteRenderer.transform.localPosition = __instance.CounterOrigin + new Vector3(__instance.CounterOffsets.x * num, 0f, 0f);
                                __instance.StartCoroutine(Effects.Bloop(num * 0.5f, spriteRenderer.transform, 1f, 0.5f));
                                num++;
                            }
                        }
                    }
                }
            }
        }
    }
}

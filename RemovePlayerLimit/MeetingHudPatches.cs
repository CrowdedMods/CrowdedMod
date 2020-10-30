using UnityEngine;
using HarmonyLib;
using System.Linq;
using Hazel;

namespace RemovePlayerLimit {
	class MeetingHudPatches {
		static string lastTimerText;
		static int currentPage = 0;
		static int maxPages {
			get => (int)Mathf.Ceil(PlayerControl.AllPlayerControls.Count / 10f);
		}

		//[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote), typeof(byte), typeof(sbyte))]
		public static class MeetingHudCastVotePatch {
			public static bool Prefix(MeetingHud __instance, byte KLCOOECLPOK, sbyte BDPGFEHNPBD) {
				RemovePlayerLimitPlugin.Logger.LogDebug("Received castvote from player " + KLCOOECLPOK + " for " + BDPGFEHNPBD);
				return true;
			}
		}

		[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
		public static class VoteGuiPatch {
			public static void Postfix(MeetingHud __instance) {
				if (Input.GetKeyDown(KeyCode.UpArrow) || Input.mouseScrollDelta.y > 0f)
					currentPage = Mathf.Clamp(currentPage - 1, 0, maxPages - 1);
				else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.mouseScrollDelta.y < 0f)
					currentPage = Mathf.Clamp(currentPage + 1, 0, maxPages - 1);

				if (__instance.TimerText.Text != lastTimerText)
					__instance.TimerText.Text = (lastTimerText = __instance.TimerText.Text + $" ({currentPage + 1}/{maxPages})");

				PlayerVoteArea[] playerButtons = __instance.FALDLDJHDDJ.OrderBy(x => x.isDead).ToArray();
				int i = 0;
				foreach (PlayerVoteArea button in playerButtons) {
					if (i >= currentPage * 10 && i < (currentPage + 1) * 10) {
						button.gameObject.SetActive(true);

						int relativeIndex = i % 10;
						button.transform.localPosition = __instance.VoteOrigin + new Vector3(__instance.VoteButtonOffsets.x * (relativeIndex % 2), __instance.VoteButtonOffsets.y * (relativeIndex / 2), -1f);
					} else
						button.gameObject.SetActive(false);
					i++;
				}
			}
		}


		[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OIDNFMKACJP))]
		public static class MeetingHudCheckForEndVotingPatch {
			public static bool Prefix(MeetingHud __instance) {
				if (__instance.FALDLDJHDDJ.All((PlayerVoteArea ps) => ps.isDead || ps.didVote)) {
					byte[] self = calculateVotes(__instance.FALDLDJHDDJ);

					int maxIdx = indexOfMax(self, out bool tie);
					GameData.IHEKEPMDGIJ exiled = GameData.Instance.GetPlayerById((byte)maxIdx);
					byte[] states = __instance.FALDLDJHDDJ.Select(s => s.GetState()).ToArray();
					byte[] votes = __instance.FALDLDJHDDJ.Select(s => (byte)s.votedFor).ToArray();

					MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(__instance.NetId, 23, SendOption.Reliable);
					messageWriter.WriteBytesAndSize(states);
					messageWriter.WriteBytesAndSize(votes); //Added because of the state's 4 bit vote id limit
					messageWriter.Write((exiled != null) ? exiled.FIOIBHIDDOC : byte.MaxValue);
					messageWriter.Write(tie);
					messageWriter.EndMessage();

					MeetingHudHandleRpcPatch.VotingComplete(__instance, states, votes, exiled, tie);
				}
				//RemovePlayerLimitPlugin.Logger.LogDebug("OIDNFMKACJP end");
				return false;
			}

			static byte[] calculateVotes(PlayerVoteArea[] states) {
				byte[] self = new byte[states.Length];
				for (int i = 0; i < states.Length; i++) {
					PlayerVoteArea playerVoteArea = states[i];
					if (playerVoteArea.didVote)
						if (playerVoteArea.votedFor >= 0 && playerVoteArea.votedFor < states.Length)
							self[playerVoteArea.votedFor]++;
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

		[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.HandleRpc), typeof(byte), typeof(Hazel.MessageReader))]
		public static class MeetingHudHandleRpcPatch {
			public static bool Prefix(MeetingHud __instance, byte DNHFGLIHAFB, Hazel.MessageReader IGFFAFNNIAB) {
				switch (DNHFGLIHAFB) {
					case 22:
						__instance.Close();
						break;
					case 23: {
						byte[] states = IGFFAFNNIAB.ReadBytesAndSize();
						byte[] votes = IGFFAFNNIAB.ReadBytesAndSize();
						GameData.IHEKEPMDGIJ playerById = GameData.Instance.GetPlayerById(IGFFAFNNIAB.ReadByte());
						bool tie = IGFFAFNNIAB.ReadBoolean();
						VotingComplete(__instance, states, votes, playerById, tie);
						break;
					}
					case 24: {
						byte srcPlayerId = IGFFAFNNIAB.ReadByte();
						sbyte suspectPlayerId = IGFFAFNNIAB.ReadSByte();
						__instance.CastVote(srcPlayerId, suspectPlayerId);
						break;
					}
					case 25:
						__instance.ClearVote();
						break;
				}
				return false;
			}

			public static void VotingComplete(MeetingHud __instance, byte[] states, byte[] votes, GameData.IHEKEPMDGIJ exiled, bool tie) {
				if (__instance.FGFCFMNBKON == MeetingHud.IONNOOOEADE.Results) {
					return;
				}
				__instance.FGFCFMNBKON = MeetingHud.IONNOOOEADE.Results;
				__instance.JMHKAFCOGCM = __instance.discussionTimer;
				__instance.BJOCHIGMCBL = exiled;
				__instance.CEIGAJCFLEM = tie;
				__instance.SkipVoteButton.gameObject.SetActive(false);
				__instance.SkippedVoting.gameObject.SetActive(true);

				PopulateResults(__instance, states, votes);
				__instance.BFHIPKBGFGF();
			}

			public static void PopulateResults(MeetingHud __instance, byte[] states, byte[] votes) {
				//DestroyableSingleton<Telemetry>.LINLGEBAKJC.WriteMeetingEnded(__instance.discussionTimer);
				__instance.TitleText.Text = "Voting Results";
				int num = 0;
				for (int i = 0; i < __instance.FALDLDJHDDJ.Length; i++) {
					PlayerVoteArea playerVoteArea = __instance.FALDLDJHDDJ[i];
					playerVoteArea.ClearForResults();
					int num2 = 0;
					for (int j = 0; j < states.Length; j++) {
						if ((states[j] & 128) == 0) { //!isDead
							GameData.IHEKEPMDGIJ playerById = GameData.Instance.GetPlayerById((byte)__instance.FALDLDJHDDJ[j].TargetPlayerId);
							int votedFor = (int)votes[j];

							SpriteRenderer spriteRenderer = UnityEngine.Object.Instantiate<SpriteRenderer>(__instance.PlayerVotePrefab);
							PlayerControl.SetPlayerMaterialColors((int)playerById.LHKAPPDILFP, spriteRenderer);
							spriteRenderer.transform.localScale = Vector3.zero;

							if ((int)playerVoteArea.TargetPlayerId == votedFor) {
								spriteRenderer.transform.SetParent(playerVoteArea.transform);
								spriteRenderer.transform.localPosition = __instance.BEDJEPCINAI + new Vector3(__instance.LNFHFGONEGA.x * (float)num2, 0f, 0f);
								__instance.StartCoroutine(FBBJKJLHFKF.KGIPENFLALI((float)num2 * 0.5f, spriteRenderer.transform, 0.5f));
								num2++;
							} else if (i == 0 && votedFor == -1) {
								spriteRenderer.transform.SetParent(__instance.SkippedVoting.transform);
								spriteRenderer.transform.localPosition = __instance.BEDJEPCINAI + new Vector3(__instance.LNFHFGONEGA.x * (float)num, 0f, 0f);
								__instance.StartCoroutine(FBBJKJLHFKF.KGIPENFLALI((float)num * 0.5f, spriteRenderer.transform, 0.5f));
								num++;
							}
						}
					}
				}
			}
		}
	}
}

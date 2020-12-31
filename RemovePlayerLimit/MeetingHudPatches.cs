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

namespace CrowdedMod {
	class MeetingHudPatches {
		static string lastTimerText;
		static int currentPage = 0;
		static int maxPages {
			get => (int)Mathf.Ceil(PlayerControl.AllPlayerControls.Count / 10f);
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

				PlayerVoteArea[] playerButtons = __instance.HBDFFAHBIGI.OrderBy(x => x.isDead).ToArray();
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

		[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.IOCEGFONOOK))]
		public static class MeetingHudCheckForEndVotingPatch {
			public static bool Prefix(MeetingHud __instance) {
				if (__instance.HBDFFAHBIGI.All((PlayerVoteArea ps) => ps.isDead || ps.didVote)) {
					byte[] self = calculateVotes(__instance.HBDFFAHBIGI);

					int maxIdx = indexOfMax(self, out bool tie) - 1;
					PlayerInfo exiled = GameData.Instance.GetPlayerById((byte)maxIdx);
					byte[] states = __instance.HBDFFAHBIGI.Select(s => s.GetState()).ToArray();
					byte[] votes = __instance.HBDFFAHBIGI.Select(s => (byte)s.votedFor).ToArray();
					if (AmongUsClient.Instance.BEIEANEKAFC)
					{
					MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(__instance.NetId, 23, SendOption.Reliable);
					messageWriter.WriteBytesAndSize(states);
					messageWriter.WriteBytesAndSize(votes); //Added because of the state's 4 bit vote id limit
					messageWriter.Write((exiled != null) ? exiled.JKOMCOJCAID : byte.MaxValue);
					messageWriter.Write(tie);
					messageWriter.EndMessage();
					}

					MeetingHudHandleRpcPatch.VotingComplete(__instance, states, votes, exiled, tie);
				}
				return false;
			}

			static byte[] calculateVotes(PlayerVoteArea[] states) {
				byte[] self = new byte[states.Length + 1];
				for (int i = 0; i < states.Length; i++) {
					PlayerVoteArea playerVoteArea = states[i];
					if (playerVoteArea.didVote)
						if (playerVoteArea.votedFor + 1 >= 0 && playerVoteArea.votedFor + 1 < self.Length)
							self[playerVoteArea.votedFor + 1]++;
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
			public static bool Prefix(MeetingHud __instance, byte HKHMBLJFLMC, Hazel.MessageReader ALMCIJKELCP) {
				switch (HKHMBLJFLMC) {
					case 22:
						__instance.Close();
						break;
					case 23: {
						byte[] states = ALMCIJKELCP.ReadBytesAndSize();
						byte[] votes = ALMCIJKELCP.ReadBytesAndSize();
						PlayerInfo playerById = GameData.Instance.GetPlayerById(ALMCIJKELCP.ReadByte());
						bool tie = ALMCIJKELCP.ReadBoolean();
						VotingComplete(__instance, states, votes, playerById, tie);
						break;
					}
					case 24: {
						byte srcPlayerId = ALMCIJKELCP.ReadByte();
						sbyte suspectPlayerId = ALMCIJKELCP.ReadSByte();
						__instance.CastVote(srcPlayerId, suspectPlayerId);
						break;
					}
					case 25:
						__instance.ClearVote();
						break;
				}
				return false;
			}

			public static void VotingComplete(MeetingHud __instance, byte[] states, byte[] votes, PlayerInfo exiled, bool tie) {
				if (__instance.DCCFKHIPIOF == MeetingHud.BAMDJGFKOFP.Results) {
					return;
				}
				__instance.DCCFKHIPIOF = MeetingHud.BAMDJGFKOFP.Results;
				__instance.EKGJAHLFJFP = __instance.discussionTimer;
				__instance.LCJLLGKMINO = exiled;
				__instance.GEJDOOANNJD = tie;
				__instance.SkipVoteButton.gameObject.SetActive(false);
				__instance.SkippedVoting.gameObject.SetActive(true);

				PopulateResults(__instance, states, votes);
				__instance.GOJIAJFHPIB();
			}

			public static void PopulateResults(MeetingHud __instance, byte[] states, byte[] votes) {
				__instance.TitleText.Text = "Voting Results";
				int num = 0;
				for (int i = 0; i < __instance.HBDFFAHBIGI.Length; i++) {
					PlayerVoteArea playerVoteArea = __instance.HBDFFAHBIGI[i];
					playerVoteArea.ClearForResults();
					int num2 = 0;
					for (int j = 0; j < states.Length; j++) {
						if ((states[j] & 128) == 0) { //!isDead
							PlayerInfo playerById = GameData.Instance.GetPlayerById((byte)__instance.HBDFFAHBIGI[j].HOBAOICNHFH);
							int votedFor = (sbyte)votes[j];

							SpriteRenderer spriteRenderer = UnityEngine.Object.Instantiate<SpriteRenderer>(__instance.PlayerVotePrefab);
							if (PlayerControl.GameOptions.AGGKAFILPGD)
								PlayerControl.SetPlayerMaterialColors(Palette.ICNEJBPIBDB, spriteRenderer);
							else
								PlayerControl.SetPlayerMaterialColors((int)playerById.EHAHBDFODKC, spriteRenderer);
							spriteRenderer.transform.localScale = Vector3.zero;

							if ((int)playerVoteArea.HOBAOICNHFH == votedFor) {
								spriteRenderer.transform.SetParent(playerVoteArea.transform);
								spriteRenderer.transform.localPosition = __instance.FGJMDFDIKEK + new Vector3(__instance.IOHLPLMJHIB.x * (float)num2, 0f, 0f);
								__instance.StartCoroutine(Effects.NJOHOOJGMBC((float)num2 * 0.5f, spriteRenderer.transform, 1f, 0.5f));
								num2++;
							} else if (i == 0 && votedFor == -1) {
								spriteRenderer.transform.SetParent(__instance.SkippedVoting.transform);
								spriteRenderer.transform.localPosition = __instance.FGJMDFDIKEK + new Vector3(__instance.IOHLPLMJHIB.x * (float)num, 0f, 0f);
								__instance.StartCoroutine(Effects.NJOHOOJGMBC((float)num * 0.5f, spriteRenderer.transform, 1f, 0.5f));
								num++;
							}
						}
					}
				}
			}
		}
	}
}

using UnityEngine;
using HarmonyLib;
using System.Linq;
using Hazel;
using System.Net.NetworkInformation;

namespace RemovePlayerLimit {
	class MeetingHudPatches {
		static string lastTimerText;
		static int currentPage = 0;
		static int maxPages {
			get => (int)Mathf.Ceil(GLHCHLEDNBA.AllPlayerControls.Count / 10f);
		}

		[HarmonyPatch(typeof(GPOHFPAIEMA), nameof(GPOHFPAIEMA.Update))]
		public static class VoteGuiPatch {
			public static void Postfix(GPOHFPAIEMA __instance) {
				if (Input.GetKeyDown(KeyCode.UpArrow) || Input.mouseScrollDelta.y > 0f)
					currentPage = Mathf.Clamp(currentPage - 1, 0, maxPages - 1);
				else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.mouseScrollDelta.y < 0f)
					currentPage = Mathf.Clamp(currentPage + 1, 0, maxPages - 1);

				if (__instance.TimerText.Text != lastTimerText)
					__instance.TimerText.Text = (lastTimerText = __instance.TimerText.Text + $" ({currentPage + 1}/{maxPages})");

				LJEHDCNEKBG[] playerButtons = __instance.OMJGIAMFODK.OrderBy(x => x.isDead).ToArray();
				int i = 0;
				foreach (LJEHDCNEKBG button in playerButtons) {
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

		[HarmonyPatch(typeof(GPOHFPAIEMA), nameof(GPOHFPAIEMA.HMHBIBHOCDO))]
		public static class MeetingHudCheckForEndVotingPatch {
			public static bool Prefix(GPOHFPAIEMA __instance) {
				if (__instance.OMJGIAMFODK.All((LJEHDCNEKBG ps) => ps.isDead || ps.didVote)) {
					byte[] self = calculateVotes(__instance.OMJGIAMFODK);

					int maxIdx = indexOfMax(self, out bool tie) - 1;
					BAGGGBBOHOH.FGMBFCIIILC exiled = BAGGGBBOHOH.Instance.GetPlayerById((byte)maxIdx);
					byte[] states = __instance.OMJGIAMFODK.Select(s => s.GetState()).ToArray();
					byte[] votes = __instance.OMJGIAMFODK.Select(s => (byte)s.votedFor).ToArray();

					MessageWriter messageWriter = JNFEHNLGIFF.Instance.StartRpc(__instance.NetId, 23, SendOption.Reliable);
					messageWriter.WriteBytesAndSize(states);
					messageWriter.WriteBytesAndSize(votes); //Added because of the state's 4 bit vote id limit
					messageWriter.Write((exiled != null) ? exiled.PAGHECLPIMH : byte.MaxValue);
					messageWriter.Write(tie);
					messageWriter.EndMessage();

					MeetingHudHandleRpcPatch.VotingComplete(__instance, states, votes, exiled, tie);
				}
				return false;
			}

			static byte[] calculateVotes(LJEHDCNEKBG[] states) {
				byte[] self = new byte[states.Length + 1];
				for (int i = 0; i < states.Length; i++) {
					LJEHDCNEKBG playerVoteArea = states[i];
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

		[HarmonyPatch(typeof(GPOHFPAIEMA), nameof(GPOHFPAIEMA.HandleRpc), typeof(byte), typeof(Hazel.MessageReader))]
		public static class MeetingHudHandleRpcPatch {
			public static bool Prefix(GPOHFPAIEMA __instance, byte OPCIDHAAHLI, Hazel.MessageReader DNCHPFLHHEN) {
				switch (OPCIDHAAHLI) {
					case 22:
						__instance.Close();
						break;
					case 23: {
						byte[] states = DNCHPFLHHEN.ReadBytesAndSize();
						byte[] votes = DNCHPFLHHEN.ReadBytesAndSize();
						BAGGGBBOHOH.FGMBFCIIILC playerById = BAGGGBBOHOH.Instance.GetPlayerById(DNCHPFLHHEN.ReadByte());
						bool tie = DNCHPFLHHEN.ReadBoolean();
						VotingComplete(__instance, states, votes, playerById, tie);
						break;
					}
					case 24: {
						byte srcPlayerId = DNCHPFLHHEN.ReadByte();
						sbyte suspectPlayerId = DNCHPFLHHEN.ReadSByte();
						__instance.CastVote(srcPlayerId, suspectPlayerId);
						break;
					}
					case 25:
						__instance.ClearVote();
						break;
				}
				return false;
			}

			public static void VotingComplete(GPOHFPAIEMA __instance, byte[] states, byte[] votes, BAGGGBBOHOH.FGMBFCIIILC exiled, bool tie) {
				if (__instance.FDLEFBMLMFM == GPOHFPAIEMA.DHLNNGGOJNI.Results) {
					return;
				}
				__instance.FDLEFBMLMFM = GPOHFPAIEMA.DHLNNGGOJNI.Results;
				__instance.NFIBDGIOEIA = __instance.discussionTimer;
				__instance.GKGOFBGECAK = exiled;
				__instance.GBOFEFNNKCF = tie;
				__instance.SkipVoteButton.gameObject.SetActive(false);
				__instance.SkippedVoting.gameObject.SetActive(true);

				PopulateResults(__instance, states, votes);
				__instance.LLMHJIDIDFK();
			}

			public static void PopulateResults(GPOHFPAIEMA __instance, byte[] states, byte[] votes) {
				__instance.TitleText.Text = "Voting Results";
				int num = 0;
				for (int i = 0; i < __instance.OMJGIAMFODK.Length; i++) {
					LJEHDCNEKBG playerVoteArea = __instance.OMJGIAMFODK[i];
					playerVoteArea.ClearForResults();
					int num2 = 0;
					for (int j = 0; j < states.Length; j++) {
						if ((states[j] & 128) == 0) { //!isDead
							BAGGGBBOHOH.FGMBFCIIILC playerById = BAGGGBBOHOH.Instance.GetPlayerById((byte)__instance.OMJGIAMFODK[j].AGGEFFKBKLE);
							int votedFor = (sbyte)votes[j];

							SpriteRenderer spriteRenderer = UnityEngine.Object.Instantiate<SpriteRenderer>(__instance.PlayerVotePrefab);
							if (GLHCHLEDNBA.GameOptions.IAFJLBELLDA)
								GLHCHLEDNBA.SetPlayerMaterialColors(KPNJLIGHOEI.BAOLBIKEKEK, spriteRenderer);
							else
								GLHCHLEDNBA.SetPlayerMaterialColors((int)playerById.LMDCNHODEAN, spriteRenderer);
							spriteRenderer.transform.localScale = Vector3.zero;

							if ((int)playerVoteArea.AGGEFFKBKLE == votedFor) {
								spriteRenderer.transform.SetParent(playerVoteArea.transform);
								spriteRenderer.transform.localPosition = __instance.GOCOEAPLJFA + new Vector3(__instance.OHFLCOGINJN.x * (float)num2, 0f, 0f);
								__instance.StartCoroutine(JDBGLNAEBLG.GBIKBPPNKEB((float)num2 * 0.5f, spriteRenderer.transform, 1f, 0.5f));
								num2++;
							} else if (i == 0 && votedFor == -1) {
								spriteRenderer.transform.SetParent(__instance.SkippedVoting.transform);
								spriteRenderer.transform.localPosition = __instance.GOCOEAPLJFA + new Vector3(__instance.OHFLCOGINJN.x * (float)num, 0f, 0f);
								__instance.StartCoroutine(JDBGLNAEBLG.GBIKBPPNKEB((float)num * 0.5f, spriteRenderer.transform, 1f, 0.5f));
								num++;
							}
						}
					}
				}
			}
		}
	}
}

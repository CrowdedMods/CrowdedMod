using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CrowdedMod.Extensions
{
    public static class MeetingHudExtensions
    {
        public static void CustomVotingComplete(this MeetingHud __instance, byte[] states, byte[] votes,
            GameData.PlayerInfo exiled, bool tie)
        {
            if (__instance.state == MeetingHud.VoteStates.Results)
            {
                return;
            }

            __instance.state = MeetingHud.VoteStates.Results;
            __instance.resultsStartedAt = __instance.discussionTimer;
            __instance.exiledPlayer = exiled;
            __instance.wasTie = tie;
            __instance.SkipVoteButton.gameObject.SetActive(false);
            __instance.SkippedVoting.gameObject.SetActive(true);

            PopulateResults(__instance, states, votes);
            __instance.SetupProceedButton();
        }

        private static void PopulateResults(MeetingHud __instance, byte[] states, byte[] votes)
        {
            __instance.TitleText.text = TranslationController.Instance.GetString(StringNames.MeetingVotingResults, Array.Empty<Il2CppSystem.Object>());
            int num = 0;
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                playerVoteArea.ClearForResults();
                int num2 = 0;
                for (int j = 0; j < states.Length; j++)
                {
                    if ((states[j] & 128) == 0) // !isDead
                    {
                        GameData.PlayerInfo playerById =
                            GameData.Instance.GetPlayerById((byte) __instance.playerStates[j].TargetPlayerId);
                        int votedFor = (sbyte) votes[j];

                        SpriteRenderer spriteRenderer = Object.Instantiate(__instance.PlayerVotePrefab);
                        if (PlayerControl.GameOptions.AnonymousVotes)
                            PlayerControl.SetPlayerMaterialColors(Palette.DisabledGrey, spriteRenderer);
                        else
                            PlayerControl.SetPlayerMaterialColors(playerById.ColorId, spriteRenderer);
                        spriteRenderer.transform.localScale = Vector3.zero;

                        if (playerVoteArea.TargetPlayerId == votedFor)
                        {
                            Transform transform;
                            (transform = spriteRenderer.transform).SetParent(playerVoteArea.transform);
                            transform.localPosition = __instance.CounterOrigin +
                                                      new Vector3(__instance.CounterOffsets.x * num2, 0f, 0f);
                            __instance.StartCoroutine(Effects.Bloop(num2 * 0.5f, transform, 1f, 0.5f));
                            num2++;
                        }
                        else if (i == 0 && votedFor == -1)
                        {
                            Transform transform;
                            (transform = spriteRenderer.transform).SetParent(__instance.SkippedVoting.transform);
                            transform.localPosition = __instance.CounterOrigin +
                                                      new Vector3(__instance.CounterOffsets.x * num, 0f, 0f);
                            __instance.StartCoroutine(Effects.Bloop(num * 0.5f, transform, 1f, 0.5f));
                            num++;
                        }
                    }
                }
            }
        }
    }
}
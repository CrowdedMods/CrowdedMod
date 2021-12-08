using System;
using Reactor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CrowdedMod.Extensions;

public static class MeetingHudExtensions
{
    public static void CustomVotingComplete(this MeetingHud __instance, MeetingHud.VoterState[] states, GameData.PlayerInfo exiled, bool tie)
    {
        if (__instance.state == MeetingHud.VoteStates.Results) return;
    
        __instance.state = MeetingHud.VoteStates.Results;
        __instance.resultsStartedAt = __instance.discussionTimer;
        __instance.exiledPlayer = exiled;
        __instance.wasTie = tie;
        __instance.SkipVoteButton.gameObject.SetActive(false);
        __instance.SkippedVoting.gameObject.SetActive(true);
    
        PopulateResults(__instance, states);
        __instance.SetupProceedButton();
                
        if (DestroyableSingleton<HudManager>.Instance.Chat.IsOpen)
        {
            DestroyableSingleton<HudManager>.Instance.Chat.ForceClosed();
            ControllerManager.Instance.CloseOverlayMenu(DestroyableSingleton<HudManager>.Instance.Chat.name);
        }
                
        ControllerManager.Instance.CloseOverlayMenu(__instance.name);
        ControllerManager.Instance.OpenOverlayMenu(__instance.name, null, __instance.ProceedButtonUi);
    }
    
    private static void PopulateResults(MeetingHud __instance, MeetingHud.VoterState[] states)
    {
        __instance.TitleText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingVotingResults, Array.Empty<Il2CppSystem.Object>());
                
        var num = 0;
        for (var i = 0; i < __instance.playerStates.Length; i++)
        {
            var playerVoteArea = __instance.playerStates[i];
            playerVoteArea.ClearForResults();
                    
            var num2 = 0;
            foreach (var voterState in states)
            {
                var playerById = GameData.Instance.GetPlayerById(voterState.VoterId);
                if (playerById is null)
                {
                    Logger<CrowdedModPlugin>.Error($"Couldn't find player info for voter: {voterState.VoterId}");
                    continue;
                }
    
                if (i == 0 && voterState.SkippedVote)
                {
                    __instance.BloopAVoteIcon(playerById, num, __instance.SkippedVoting.transform);
                    num++;
                }
                else if (voterState.VotedForId == playerVoteArea.TargetPlayerId)
                {
                    __instance.BloopAVoteIcon(playerById, num2, playerVoteArea.transform);
                    num2++;
                }
            }
        }
    }
}
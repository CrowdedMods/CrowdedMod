using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using System.Linq;
using CrowdedMod.Net;
using Reactor;
using Reactor.Networking;

namespace CrowdedMod.Patches;

internal static class MeetingHudPatches
{
    private static string lastTimerText;
    private static int currentPage;
    private static int maxPages => (int) Mathf.Ceil(GameData.Instance.AllPlayers.Count / 15f);
    
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    public static class VoteGuiPatch
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.mouseScrollDelta.y > 0f)
                currentPage = Mathf.Clamp(currentPage - 1, 0, maxPages - 1);
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.mouseScrollDelta.y < 0f)
                currentPage = Mathf.Clamp(currentPage + 1, 0, maxPages - 1);
    
            if (__instance.TimerText.text != lastTimerText)
                __instance.TimerText.text =
                    lastTimerText = __instance.TimerText.text + $" ({currentPage + 1}/{maxPages})";
    
            var playerButtons = __instance.playerStates.OrderBy(x => x.AmDead).ToArray();
            var i = 0;
                    
            foreach (var button in playerButtons)
            {
                if (i >= currentPage * 15 && i < (currentPage + 1) * 15)
                {
                    button.gameObject.SetActive(true);
    
                    var relativeIndex = i % 15;
                    button.transform.localPosition = __instance.VoteOrigin +
                                                     new Vector3(
                                                         __instance.VoteButtonOffsets.x * (relativeIndex % 3),
                                                         __instance.VoteButtonOffsets.y * (relativeIndex / 3),
                                                         -1f
                                                     );
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
    
                i++;
            }
        }
    }
    
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
    public static class MeetingHudCheckForEndVotingPatch
    {
        public static bool Prefix(MeetingHud __instance)
        {
            try
            {
                if (!__instance.playerStates.All(ps => ps.AmDead || ps.DidVote)) return false;
    
                var self = CalculateVotes(__instance.playerStates);
                var max = MaxPair(self, out var tie);
    
                var exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(v => !tie && v.PlayerId == max.Key);
                var states = __instance.playerStates.Select(x => new MeetingHud.VoterState {
                    VoterId = x.TargetPlayerId,
                    VotedForId = x.VotedFor
                }).ToArray();
    
                Rpc<VotingCompleteRpc>.Instance.Send(__instance, new VotingCompleteRpc.Data(states, exiled, tie));
    
                return false;
            }
            catch (Exception ex)
            {
                Logger<CrowdedModPlugin>.Error($"There's an error when checking for end in voting: \n{ex}");
                return true;
            }
        }
    
        private static Dictionary<byte, int> CalculateVotes(IEnumerable<PlayerVoteArea> states)
        {
            var dictionary = new Dictionary<byte, int>();
                    
            foreach (var playerVoteArea in states)
            {
                if (playerVoteArea.VotedFor is 252 or 255 or 254) continue;
    
                dictionary[playerVoteArea.VotedFor] = dictionary.TryGetValue(playerVoteArea.VotedFor, out var num) ? num + 1 : 1;
            }
                    
            return dictionary;
        }
    
        private static KeyValuePair<byte, int> MaxPair(Dictionary<byte, int> self, out bool tie)
        {
            tie = true;
            var result = new KeyValuePair<byte, int>(byte.MaxValue, int.MinValue);
                
            foreach (var keyValuePair in self)
            {
                if (keyValuePair.Value > result.Value)
                {
                    result = keyValuePair;
                    tie = false;
                }
                else if (keyValuePair.Value == result.Value)
                {
                    tie = true;
                }
            }
            return result;
        }
    }
}
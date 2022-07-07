using System;
using System.Collections.Generic;
using System.Linq;
using Reactor;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace CrowdedMod.Components;

[RegisterInIl2Cpp]
public class MeetingHudPagingBehaviour : AbstractPagingBehaviour
{
    public MeetingHudPagingBehaviour(IntPtr ptr) : base(ptr)
    {
    }

    public MeetingHud MeetingHud = null!;

    [HideFromIl2Cpp]
    public IEnumerable<PlayerVoteArea> Targets => MeetingHud.playerStates.OrderBy(p => p.AmDead);
    public override int MaxPage => Targets.Count() / MaxPerPage;

    public override void Update()
    {
        base.Update();
        MeetingHud.TimerText.text += $" ({Page + 1}/{MaxPage + 1})";
    }

    public override void OnPageChanged()
    {
        var i = 0;

        foreach (var button in Targets) {
            if (i >= Page * MaxPerPage && i < (Page + 1) * MaxPerPage) {
                button.gameObject.SetActive(true);

                var relativeIndex = i % MaxPerPage;
                var row = relativeIndex / 3;
                var buttonTransform = button.transform;
                buttonTransform.localPosition = MeetingHud.VoteOrigin +
                                          new Vector3(
                                              MeetingHud.VoteButtonOffsets.x * (relativeIndex % 3),
                                              MeetingHud.VoteButtonOffsets.y * row, 
                                              buttonTransform.localPosition.z
                                          );
            } else {
                button.gameObject.SetActive(false);
            }
            i++;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using UnityEngine;
using Reactor.Networking.Rpc;
using Reactor.Utilities;

namespace CrowdedMod.Components;

[RegisterInIl2Cpp]
public class MeetingHudPagingBehaviour : AbstractPagingBehaviour
{
    public MeetingHudPagingBehaviour(IntPtr ptr) : base(ptr)
    {
    }

    internal MeetingHud meetingHud = null!;

    [HideFromIl2Cpp] public IEnumerable<PlayerVoteArea> Targets => meetingHud.playerStates.OrderBy(p => p.AmDead);
    public override int MaxPageIndex
    {
        get
        {
            if (maxPageIndex == -1)
            {
                maxPageIndex = (Targets.Count() - 1) / MaxPerPage;
            }
            return maxPageIndex;
        }
    }

    private int maxPageIndex = -1;

    public override void Start()
    {
        maxPageIndex = -1;
        OnPageChanged();
    }

    public override void Update()
    {
        base.Update();

        if (meetingHud.state is MeetingHud.VoteStates.Animating or MeetingHud.VoteStates.Proceeding ||
            meetingHud.TimerText.text.Contains($" ({PageIndex + 1}/{MaxPageIndex + 1})"))
            return;

        meetingHud.TimerText.text += $" ({PageIndex + 1}/{MaxPageIndex + 1})";
    }

    public override void OnPageChanged()
    {
        var i = 0;

        foreach (var button in Targets)
        {
            if (i >= PageIndex * MaxPerPage && i < (PageIndex + 1) * MaxPerPage)
            {
                button.gameObject.SetActive(true);

                var relativeIndex = i % MaxPerPage;
                var row = relativeIndex / 3;
                var col = relativeIndex % 3;
                var buttonTransform = button.transform;
                buttonTransform.localPosition = meetingHud.VoteOrigin +
                                                new Vector3(
                                                    meetingHud.VoteButtonOffsets.x * col,
                                                    meetingHud.VoteButtonOffsets.y * row,
                                                    buttonTransform.localPosition.z
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
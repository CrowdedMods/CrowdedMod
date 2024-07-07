using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace CrowdedMod.Components;

[RegisterInIl2Cpp]
public class VitalsPagingBehaviour : AbstractPagingBehaviour
{
    public VitalsPagingBehaviour(IntPtr ptr) : base(ptr) { }

    public VitalsMinigame vitalsMinigame = null!;

    [HideFromIl2Cpp]
    public IEnumerable<VitalsPanel> Targets => vitalsMinigame.vitals.ToArray();
    public override int MaxPageIndex => (Targets.Count() - 1) / MaxPerPage;
    private TextMeshPro PageText = null!;

    public override void Start()
    {
        PageText = Instantiate(HudManager.Instance.KillButton.cooldownTimerText, vitalsMinigame.transform);
        PageText.name = PAGE_INDEX_GAME_OBJECT_NAME;
        PageText.enableWordWrapping = false;
        PageText.gameObject.SetActive(true);
        PageText.transform.localPosition = new Vector3(2.7f, -2f, -1f);
        PageText.transform.localScale *= 0.5f;
        OnPageChanged();
    }

    public override void OnPageChanged()
    {
        if (PlayerTask.PlayerHasTaskOfType<HudOverrideTask>(PlayerControl.LocalPlayer))
            return;

        PageText.text = $"({PageIndex + 1}/{MaxPageIndex + 1})";
        var i = 0;

        foreach (var panel in Targets)
        {
            if (i >= PageIndex * MaxPerPage && i < (PageIndex + 1) * MaxPerPage)
            {
                panel.gameObject.SetActive(true);
                var relativeIndex = i % MaxPerPage;
                var row = relativeIndex / 3;
                var col = relativeIndex % 3;
                var panelTransform = panel.transform;
                panelTransform.localPosition = new Vector3(
                                                    vitalsMinigame.XStart + vitalsMinigame.XOffset * col,
                                                    vitalsMinigame.YStart + vitalsMinigame.YOffset * row,
                                                    panelTransform.localPosition.z
                                                );
            }
            else
                panel.gameObject.SetActive(false);

            i++;
        }
    }
}

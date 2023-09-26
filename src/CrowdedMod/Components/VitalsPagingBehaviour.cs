using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using TMPro;
using Reactor.Utilities.Attributes;

namespace CrowdedMod.Components;

[RegisterInIl2Cpp]
public class VitalsPagingBehaviour : AbstractPagingBehaviour
{
    public VitalsPagingBehaviour(IntPtr ptr) : base(ptr) {}

    public VitalsMinigame vitalsMinigame = null!;

    [HideFromIl2Cpp]
    public IEnumerable<VitalsPanel> Targets => vitalsMinigame.vitals.ToArray();
    public override int MaxPageIndex => (Targets.Count() - 1) / MaxPerPage;
    private TextMeshPro PageText = null!;

    public override void Start()
    {
        PageText = Instantiate(HudManager.Instance.KillButton.cooldownTimerText, vitalsMinigame.transform);
        PageText.name = "MenuPageCount";
        PageText.enableWordWrapping = false;
        PageText.gameObject.SetActive(true);
        PageText.transform.localPosition = new(2.7f, -2f, -1f);
        PageText.transform.localScale *= 0.5f;
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
                panel.transform.localPosition = new(
                                                    vitalsMinigame.XStart + (vitalsMinigame.XOffset * col),
                                                    vitalsMinigame.YStart + (vitalsMinigame.YOffset * row),
                                                    panel.transform.position.z
                                                );
            }
            else
                panel.gameObject.SetActive(false);

            i++;
        }
    }
}

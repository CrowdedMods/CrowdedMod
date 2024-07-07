using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace CrowdedMod.Components;

[RegisterInIl2Cpp]
public class ShapeShifterPagingBehaviour : AbstractPagingBehaviour
{
    public ShapeShifterPagingBehaviour(IntPtr ptr) : base(ptr)
    {
    }

    public ShapeshifterMinigame shapeshifterMinigame = null!;
    [HideFromIl2Cpp]
    public IEnumerable<ShapeshifterPanel> Targets => shapeshifterMinigame.potentialVictims.ToArray();

    public override int MaxPageIndex => (Targets.Count() - 1) / MaxPerPage;
    private TextMeshPro PageText = null!;

    public override void Start()
    {
        PageText = Instantiate(HudManager.Instance.KillButton.cooldownTimerText, shapeshifterMinigame.transform);
        PageText.name = PAGE_INDEX_GAME_OBJECT_NAME;
        PageText.enableWordWrapping = false;
        PageText.gameObject.SetActive(true);
        PageText.transform.localPosition = new Vector3(4.1f, -2.36f, -1f);
        PageText.transform.localScale *= 0.5f;
        OnPageChanged();
    }

    public override void OnPageChanged()
    {
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
                var buttonTransform = panel.transform;
                buttonTransform.localPosition = new Vector3(
                                                    shapeshifterMinigame.XStart + shapeshifterMinigame.XOffset * col,
                                                    shapeshifterMinigame.YStart + shapeshifterMinigame.YOffset * row,
                                                    buttonTransform.localPosition.z
                                                );
            }
            else
            {
                panel.gameObject.SetActive(false);
            }

            i++;
        }
    }
}
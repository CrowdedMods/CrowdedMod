using System;
using System.Collections.Generic;
using System.Linq;
using Reactor;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace CrowdedMod.Components;

[RegisterInIl2Cpp]
public class ShapeShifterPagingBehaviour : AbstractPagingBehaviour
{
    public ShapeShifterPagingBehaviour(IntPtr ptr) : base(ptr)
    {
    }

    public ShapeshifterMinigame ShapeshifterMinigame = null!;
    [HideFromIl2Cpp]
    public IEnumerable<ShapeshifterPanel> Targets => ShapeshifterMinigame.potentialVictims.ToArray();

    public override int MaxPage => Targets.Count() / MaxPerPage;

    public override void OnPageChanged()
    {
        var i = 0;

        foreach (var panel in Targets)
        {
            if (i >= Page * MaxPerPage && i < (Page + 1) * MaxPerPage) {
                panel.gameObject.SetActive(true);

                var relativeIndex = i % MaxPerPage;
                var row = relativeIndex / 3;
                var buttonTransform = panel.transform;
                buttonTransform.localPosition = new Vector3(
                                                    ShapeshifterMinigame.XStart + ShapeshifterMinigame.XOffset * (relativeIndex % 3),
                                                    ShapeshifterMinigame.YStart + ShapeshifterMinigame.YOffset * row, 
                                                    buttonTransform.localPosition.z
                                                );
            } else {
                panel.gameObject.SetActive(false);
            }

            i++;
        }
    }
}
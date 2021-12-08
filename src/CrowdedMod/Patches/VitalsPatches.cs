using System;
using UnityEngine;
using HarmonyLib;
using System.Linq;

namespace CrowdedMod.Patches {
    internal static class VitalsPatches
    {
        private const int maxPerPage = 15;
        private static int currentPage;
        private static int maxPages => (int) Mathf.Ceil((float) PlayerControl.AllPlayerControls.Count / maxPerPage);
    
        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
        public static class VitalsGuiPatchUpdate
        {
            public static void Postfix(VitalsMinigame __instance)
            {
                if (PlayerTask.PlayerHasTaskOfType<HudOverrideTask>(PlayerControl.LocalPlayer))
                    return;
                    
                // Allow to switch pages
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.mouseScrollDelta.y > 0f)
                    currentPage = Mathf.Clamp(currentPage - 1, 0, maxPages - 1);
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.mouseScrollDelta.y < 0f)
                    currentPage = Mathf.Clamp(currentPage + 1, 0, maxPages - 1);
    
                // Place dead players at the beginning, disconnected at the end
                var vitalsPanels =
                    __instance.vitals.OrderBy(x => (x.IsDead ? 0 : 1) + (x.IsDiscon ? 2 : 0))
                        .ToArray(); // Sorted by: Dead -> Alive -> dead&disc -> alive&disc
                    
                // Show/hide/move each panel
                for (var i = 0; i < vitalsPanels.Length; i++)
                {
                    var panel = vitalsPanels[i];
                        
                    if (i >= currentPage * maxPerPage && i < (currentPage + 1) * maxPerPage)
                    {
                        panel.gameObject.SetActive(true);
                        var relativeIndex = i % maxPerPage; 
                            
                        panel.transform.localPosition = new Vector3(
                            __instance.XStart + __instance.XOffset * (relativeIndex % 3),
                            __instance.YStart - relativeIndex / 3 * __instance.YOffset, 
                            0f
                        );
                    }
                    else
                    {
                        panel.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}

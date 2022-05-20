using UnityEngine;
using HarmonyLib;

namespace CrowdedMod.Patches {
    internal static class VitalsPatches
    {
        private static int currentPage;
        private const int maxPerPage = 15;
        private static int maxPages => (int)Mathf.Ceil((float)PlayerControl.AllPlayerControls.Count / maxPerPage);
        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
        public static class VitalsGuiPatchUpdate
        {
            public static void Postfix(VitalsMinigame __instance)
            {
                if (PlayerTask.PlayerHasTaskOfType<HudOverrideTask>(PlayerControl.LocalPlayer))
                    return;
                //Allow to switch pages
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.mouseScrollDelta.y > 0f)
                    currentPage = Mathf.Clamp(currentPage - 1, 0, maxPages - 1);
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.mouseScrollDelta.y < 0f)
                    currentPage = Mathf.Clamp(currentPage + 1, 0, maxPages - 1);

                var i = 0;
                //Show/hide/move each panel
                foreach (var panel in __instance.vitals)
                {
                    if (i >= currentPage * maxPerPage && i < (currentPage + 1) * maxPerPage)
                    {
                        panel.gameObject.SetActive(true);
                        var relativeIndex = i % maxPerPage;
                        panel.transform.localPosition = new Vector3(
                            __instance.XStart + __instance.XOffset * (relativeIndex % 3), 
                            __instance.YStart + __instance.YOffset * (relativeIndex / 3), 
                            -1f
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
    }
}

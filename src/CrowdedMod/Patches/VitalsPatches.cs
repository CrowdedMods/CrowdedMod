using System;
using UnityEngine;
using HarmonyLib;
using System.Linq;

namespace CrowdedMod.Patches {
    internal static class VitalsPatches
    {
        private static int currentPage;
        private const int maxPerPage = 10;
        private static int maxPages => (int)Mathf.Ceil((float)PlayerControl.AllPlayerControls.Count / maxPerPage);

        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
        public static class VitalsGuiPatchBegin
        {
            public static void Postfix(VitalsMinigame __instance)
            {
                //Fix the name of each player (better multi color handling)
                VitalsPanel[] vitalsPanels = __instance.vitals;
                foreach (var color in Palette.ShortColorNames)
                {
                    string colorString = TranslationController.Instance.GetString(color, Array.Empty<Il2CppSystem.Object>());
                    VitalsPanel[] colorFiltered = vitalsPanels.Where(panel => panel.Text.text.Equals(colorString)).ToArray();
                    if (colorFiltered.Length <= 1)
                        continue;
                    int i = 1;
                    foreach (VitalsPanel panel in colorFiltered)
                    {
                        panel.Text.text += i;
                        i++;
                    }
                }
            }
        }
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

                //Place dead players at the beginning, disconnected at the end
                VitalsPanel[] vitalsPanels = __instance.vitals.OrderBy(x => (x.IsDead ? 0 : 1) + (x.IsDiscon ? 2 : 0)).ToArray();//VitalsPanel[] //Sorted by: Dead -> Alive -> dead&disc -> alive&disc
                int i = 0;

                //Show/hide/move each panel
                foreach (VitalsPanel panel in vitalsPanels)
                {
                    if (i >= currentPage * maxPerPage && i < (currentPage + 1) * maxPerPage)
                    {
                        panel.gameObject.SetActive(true);
                        int relativeIndex = i % maxPerPage;
                        // /!\ -2.7f hardcoded, can we get it the same way as MeetingHud.VoteOrigin ?
                        var transform = panel.transform;
                        var localPosition = transform.localPosition;
                        localPosition = new Vector3(-2.7f + 0.6f * relativeIndex, localPosition.y, localPosition.z);
                        transform.localPosition = localPosition;
                    }
                    else
                        panel.gameObject.SetActive(false);
                    i++;
                }
            }
        }
    }
}

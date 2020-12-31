using UnityEngine;
using HarmonyLib;
using System;
using System.Linq;
using Hazel;
using PlayerControl = FFGALNAPKCD;
using VitalsMinigame = JOFALPIHHOI;
using VitalsPanel = KMDJIBIMJIH;

namespace CrowdedMod {
	class VitalsPatches
	{
		static int currentPage = 0;
		static int maxPerPage = 10;
		static int maxPages
		{
			get => (int)Mathf.Ceil(PlayerControl.AllPlayerControls.Count / (float)maxPerPage);
		}
		[HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
		public static class VitalsGuiPatchBegin
		{
			static string[] colors = { "RED", "BLUE", "GRN", "PINK", "ORNG", "YLOW", "BLAK", "WHTE", "PURP", "BRWN", "CYAN", "LIME"};
			public static void Postfix(VitalsMinigame __instance)
			{
				//Fix the name of each player (better multi color handling)
				VitalsPanel[] vitalsPanels = __instance.MILGJPIGONF;
				foreach (string color in colors)
				{
					VitalsPanel[] colorFiltered = vitalsPanels.Where(panel => panel.Text.Text.Equals(color)).ToArray();
					if (colorFiltered.Length <= 1)
						continue;
					int i = 1;
					foreach (VitalsPanel panel in colorFiltered)
                    {
						panel.Text.Text = panel.Text.Text + i;
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
				//Allow to switch pages
				if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.mouseScrollDelta.y > 0f)
					currentPage = Mathf.Clamp(currentPage - 1, 0, maxPages - 1);
				else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.mouseScrollDelta.y < 0f)
					currentPage = Mathf.Clamp(currentPage + 1, 0, maxPages - 1);

				//Place dead players at the begining, disconnected at the end
				VitalsPanel[] vitalsPanels = __instance.MILGJPIGONF.OrderBy(x => (x.IsDead ? 0 : 1) + (x.IsDiscon ? 2 : 0)).ToArray();//VitalsPanel[] //Sorted by: Dead -> Alive -> dead&disc -> alive&disc
				int i = 0;

				//Show/hide/move each panel
				foreach (VitalsPanel panel in vitalsPanels)
				{
					if (i >= currentPage * maxPerPage && i < (currentPage + 1) * maxPerPage)
					{
						panel.gameObject.SetActive(true);
						int relativeIndex = i % maxPerPage;
						// /!\ -2.7f hardcoded, can we get it the same way as MeetinHud.VoteOrigin ?
						panel.transform.localPosition = new Vector3(-2.7f + 0.6f * relativeIndex, panel.transform.localPosition.y, panel.transform.localPosition.z);
					}
					else
						panel.gameObject.SetActive(false);
					i++;
				}
			}
		}
	}
}

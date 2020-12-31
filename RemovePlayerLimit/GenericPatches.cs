using HarmonyLib;
using System.Collections.Generic;
using UnhollowerBaseLib;
using PlayerControl = FFGALNAPKCD;
using PlayerTab = MAOILGPNFND;
using PlayerInfo = EGLJNOMOGNP.DCJMABDDJCF;
using GameData = EGLJNOMOGNP;
using Palette = LOCPGOACAJF;
using RegionMenu = CLIGCNHFBCO;
using RegionInfo = OIBMKGDLGOG;
using ServerInfo = PLFDMKKDEMI;
using ServerManager = AOBNFCIHAJL;
using ObjectPoolBehavior = FJBFFDFFBFO;
using PassiveButton = HHMBANDDIOA;
using KeyMinigame = AMKEIECODLC;
using VersionShower = BOCOFLHKCOJ;
using PingTracker = ELDIDNABIPI;

namespace CrowdedMod {
	static class GenericPatches {
        static RegionInfo[] _defaultRegions = new RegionInfo[3];
        public static ServersParser.ParseResult parseStatus;
        static bool _firstRun = true;
        static readonly Dictionary<ServersParser.ParseResult, string> parseErrorMessages = new Dictionary<ServersParser.ParseResult, string>()
        {
            { ServersParser.ParseResult.Comment, "[FF0000FF]`servers.txt` Successfully loaded,\nbut no servers found there" },
            { ServersParser.ParseResult.FileNotFound, "[FF0000FF]`servers.txt` not found\n(has to be in Among Us directory)" },
            { ServersParser.ParseResult.InvalidData, "[FF0000FF]Invalid `servers.txt` data\nSee https://github.com/CrowdedMods/CrowdedMod/wiki/servers.txt" }
        };

        [HarmonyPatch(typeof(GameData), nameof(GameData.GetAvailableId))]
		public static class GameDataAvailableIdPatch {
			public static bool Prefix(ref GameData __instance, ref sbyte __result) {
				for (int i = 0; i < 128; i++)
					if (checkId(__instance, i)) {
						__result = (sbyte)i;
						return false;
					}
				__result = -1;
				return false;
			}

			static bool checkId(GameData __instance, int id) {
				foreach (PlayerInfo p in __instance.AllPlayers)
					if (p.JKOMCOJCAID == id)
						return false;
				return true;
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckColor), typeof(byte))]
		public static class PlayerControlCheckColorPatch {
			public static bool Prefix(PlayerControl __instance, byte POCIJABNOLE) {
				__instance.RpcSetColor(POCIJABNOLE);
				return false;
			}
		}

		[HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.UpdateAvailableColors))]
		public static class PlayerTabUpdateAvailableColorsPatch {
			public static bool Prefix(PlayerTab __instance) {
				PlayerControl.SetPlayerMaterialColors(PlayerControl.LocalPlayer.NDGFFHMFGIG.EHAHBDFODKC, __instance.DemoImage);
				for (int i = 0; i < Palette.OPKIKLENHFA.Length; i++)
					__instance.LGAIKONLBIG.Add(i);
				return false;
			}
		}
        // CUSTOMSERVERSCLIENT
        [HarmonyPatch(typeof(RegionMenu), nameof(RegionMenu.OnEnable))]
        public static class RegionMenuOnEnablePatch
        {
            public static bool Prefix(ref RegionMenu __instance)
            {
                ClearOnClickAction(__instance.ButtonPool);

                if (_firstRun)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        _defaultRegions[i] = ServerManager.DefaultRegions[i];
                    }

                    _firstRun = false;
                }

                if (ServerManager.DefaultRegions.Count != 3 + ServersParser.servers.Count)
                {
                    var regions = new RegionInfo[3 + ServersParser.servers.Count];

                    for (int i = 0; i < 3; i++)
                    {
                        regions[i] = _defaultRegions[i];
                    }
                    for (int i = 0; i < ServersParser.servers.Count; i++)
                    {
                        Il2CppReferenceArray<ServerInfo> servers = new ServerInfo[1] { new ServerInfo(ServersParser.servers[i].name, ServersParser.servers[i].ip, (ushort)ServersParser.servers[i].port) };

                        regions[i + 3] = new RegionInfo(ServersParser.servers[i].name, "0", servers);
                    }

                    ServerManager.DefaultRegions = regions;
                }

                return true;
            }

            public static void ClearOnClickAction(ObjectPoolBehavior buttonPool)
            {
                foreach (var button in buttonPool.activeChildren)
                {
                    var buttonComponent = button.GetComponent<PassiveButton>();
                    if (buttonComponent != null)
                        buttonComponent.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                }

                foreach (var button in buttonPool.inactiveChildren)
                {
                    var buttonComponent = button.GetComponent<PassiveButton>();
                    if (buttonComponent != null)
                        buttonComponent.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                }
            }
        }
        [HarmonyPatch(typeof(KeyMinigame),nameof(KeyMinigame.Start))]
        public static class KeyMinigamePatch
        {
            public static void Postfix(ref KeyMinigame __instance)
            {
                PlayerControl localPlayer = PlayerControl.LocalPlayer;
                __instance.ECHAPHLBHDC = (int)((localPlayer != null) ? localPlayer.PlayerId % 10 : 0);
            }
        }

        [HarmonyPatch(typeof(VersionShower), "Start")]
        public static class VersionShowerPatch
        {
            public static void Postfix(VersionShower __instance)
            {
                __instance.text.Text = "Among Us " + __instance.text.Text + " \n[3DAD2BFF]Crowded Mod v3.4 by Przebot#2448 \nForked from andry08";
                if(parseStatus != ServersParser.ParseResult.Success)
                {
                    __instance.text.Text += $"\n\n{parseErrorMessages[parseStatus]}";
                }
            }
        }

        [HarmonyPatch(typeof(PingTracker), "Update")]
        public static class PingShowerPatch
        {
            public static void Postfix(PingTracker __instance)
            {
                __instance.text.Text += "\n[FFB793FF]> CrowdedMod <";
            }
        }
    }
}

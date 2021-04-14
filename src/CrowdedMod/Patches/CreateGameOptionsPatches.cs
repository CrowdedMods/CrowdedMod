using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace CrowdedMod.Patches
{
    internal static class CreateGameOptionsPatches
    {
        [HarmonyPatch(typeof(CreateOptionsPicker), nameof(CreateOptionsPicker.Start))]
        public static class CreateOptionsPicker_Start // Credits to XtraCube (mostly)
        {
            public const byte maxPlayers = 127;
            
            public static void Postfix(CreateOptionsPicker __instance)
            {
                if (__instance.mode != SettingsMode.Host) return;
                var offset = __instance.MaxPlayerButtons[1].transform.position.x - __instance.MaxPlayerButtons[0].transform.position.x;

                #region MaxPlayers stuff
                
                List<SpriteRenderer> playerButtons = __instance.MaxPlayerButtons.ToList();
                
                SpriteRenderer plusButton = Object.Instantiate(playerButtons.Last(), playerButtons.Last().transform.parent);
                plusButton.GetComponentInChildren<TextMeshPro>().text = "+";
                plusButton.name = "255";
                plusButton.transform.position = playerButtons.Last().transform.position + new Vector3(offset*2, 0, 0);
                var passiveButton = plusButton.GetComponent<PassiveButton>();
                passiveButton.OnClick.RemoveAllListeners();
                passiveButton.OnClick.AddListener((UnityAction)plusListener);
                
                void plusListener()
                {
                    byte curHighest = byte.Parse(playerButtons[__instance.MaxPlayerButtons.Length - 2].name);
                    int delta = Mathf.Clamp(curHighest + 7, curHighest, maxPlayers) - curHighest;
                    if (delta == 0) return; // fast skip
                    for (byte i = 1; i < 8; i++)
                    {
                        SpriteRenderer button = __instance.MaxPlayerButtons[i];
                        button.name = 
                            button.GetComponentInChildren<TextMeshPro>().text = 
                                (byte.Parse(button.name) + delta).ToString();
                    }
                    __instance.SetMaxPlayersButtons(__instance.GetTargetOptions().MaxPlayers);
                }
                
                SpriteRenderer minusButton = Object.Instantiate(playerButtons.Last(), playerButtons.Last().transform.parent);
                minusButton.GetComponentInChildren<TextMeshPro>().text = "-";
                minusButton.name = "255";
                minusButton.transform.position = playerButtons.First().transform.position;
                var minusPassiveButton = minusButton.GetComponent<PassiveButton>();
                minusPassiveButton.OnClick.RemoveAllListeners();
                minusPassiveButton.OnClick.AddListener((UnityAction)minusListener);
                
                void minusListener()
                {
                    byte curLowest = byte.Parse(playerButtons[1].name);
                    int delta = curLowest - Mathf.Clamp(curLowest - 7, 4, curLowest);
                    if (delta == 0) return; // fast skip
                    for (byte i = 1; i < 8; i++)
                    {
                        SpriteRenderer button = __instance.MaxPlayerButtons[i];
                        button.name = 
                            button.GetComponentInChildren<TextMeshPro>().text = 
                                (byte.Parse(button.name) - delta).ToString();
                    }
                    __instance.SetMaxPlayersButtons(__instance.GetTargetOptions().MaxPlayers);
                }
                
                playerButtons.ForEach(b =>
                {
                    var button = b.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    void defaultListener()
                    {
                        byte value = byte.Parse(button.name);
                        var targetOptions = __instance.GetTargetOptions();
                        if (value <= targetOptions.NumImpostors)
                        {
                            targetOptions.NumImpostors = value - 1;
                            __instance.UpdateImpostorsButtons(targetOptions.NumImpostors);
                        }
                        __instance.SetMaxPlayersButtons(value);
                    } 
                    button.OnClick.AddListener((UnityAction)defaultListener);
                    button.transform.position += new Vector3(offset, 0, 0);
                });
                
                playerButtons.Insert(0, minusButton);
                playerButtons.Add(plusButton);
                __instance.MaxPlayerButtons = playerButtons.ToArray();
                
                #endregion

                #region Impostor stuff

                var impostorButtons = __instance.ImpostorButtons.ToList();
                
                for (byte i = 4; i < 11; i++)
                {
                    SpriteRenderer button = Object.Instantiate(impostorButtons.Last(), impostorButtons.Last().transform.parent);
                    button.GetComponent<PassiveButton>().name = button.GetComponentInChildren<TextMeshPro>().text = i.ToString();
                    button.transform.position += new Vector3(offset, 0, 0);
                    impostorButtons.Add(button);
                }
                
                impostorButtons.ForEach(b =>
                {
                    var button = b.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    void defaultListener()
                    {
                        byte value = byte.Parse(button.name);
                        if (value >= __instance.GetTargetOptions().MaxPlayers)
                        {
                            return;
                        }
                        __instance.SetImpostorButtons(byte.Parse(button.name));
                    }
                    button.OnClick.AddListener((UnityAction)defaultListener);
                });

                __instance.ImpostorButtons = impostorButtons.ToArray();
                __instance.SetImpostorButtons(__instance.GetTargetOptions().NumImpostors);
                
                #endregion
            }
        }

        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.GameHostOptions), MethodType.Getter)]
        public static class SaveManager_get_GameHostOptions
        {
            public static bool Prefix(out GameOptionsData __result)
            {
                SaveManager.hostOptionsData ??= SaveManager.LoadGameOptions("gameHostOptions");

                // patched because of impostor clamping
                SaveManager.hostOptionsData.NumImpostors = 
                    Mathf.Clamp(SaveManager.hostOptionsData.NumImpostors, 1, SaveManager.hostOptionsData.MaxPlayers - 1);
                SaveManager.hostOptionsData.KillDistance = Mathf.Clamp(SaveManager.hostOptionsData.KillDistance, 0, 2);

                __result = SaveManager.hostOptionsData;
                return false;
            }
        }
    }
}
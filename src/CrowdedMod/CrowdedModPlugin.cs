using System.Linq;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using Reactor;

namespace CrowdedMod;

[BepInAutoPlugin("pl.przebor.crowded")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
public partial class CrowdedModPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);

    public override void Load()
    {
        GameOptionsData.RecommendedImpostors = GameOptionsData.MaxImpostors = Enumerable.Repeat(127, 127).ToArray();
        GameOptionsData.MinPlayers = Enumerable.Repeat(4, 127).ToArray();

        Harmony.PatchAll();
    }
}
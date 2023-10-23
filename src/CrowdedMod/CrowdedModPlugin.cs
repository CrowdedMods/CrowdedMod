using System.Linq;
using AmongUs.GameOptions;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;

namespace CrowdedMod;

[BepInAutoPlugin("xyz.crowdedmods.crowdedmod")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
[BepInDependency("gg.reactor.debugger", BepInDependency.DependencyFlags.SoftDependency)] // fix debugger overwriting MinPlayers
public partial class CrowdedModPlugin : BasePlugin
{
    public const int MaxPlayers = 127;
    public const int MaxImpostors = 127 / 2;

    private Harmony Harmony { get; } = new (Id);

    public override void Load()
    {
        NormalGameOptionsV07.RecommendedImpostors = NormalGameOptionsV07.MaxImpostors = Enumerable.Repeat(127, 127).ToArray();
        NormalGameOptionsV07.MinPlayers = Enumerable.Repeat(4, 127).ToArray();

        Harmony.PatchAll();
    }
}
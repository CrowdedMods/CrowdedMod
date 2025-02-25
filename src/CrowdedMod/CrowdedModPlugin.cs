using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;

namespace CrowdedMod;

[BepInAutoPlugin("xyz.crowdedmods.crowdedmod")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
[BepInDependency("gg.reactor.debugger", BepInDependency.DependencyFlags.SoftDependency)]
public partial class CrowdedModPlugin : BasePlugin
{
    public const int MaxPlayers = 254; // allegedly. should stick to 127 tho
    public const int MaxImpostors = MaxPlayers / 2;

    public static bool ForceDisableFreeColor { get; set; } = false;

    private Harmony Harmony { get; } = new(Id);

    public override void Load()
    {
        ReactorCredits.Register<CrowdedModPlugin>(ReactorCredits.AlwaysShow);

        Harmony.PatchAll();
    }
}
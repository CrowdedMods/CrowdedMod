using System.Linq;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using Reactor;

namespace CrowdedMod {
    [BepInAutoPlugin]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    [BepInDependency("gg.reactor.debugger", BepInDependency.DependencyFlags.SoftDependency)] // fix debugger overwriting MinPlayers
    public partial class CrowdedModPlugin : BasePlugin
    {
        public const int MaxPlayers = 127;
        public const int MaxImpostors = 127 / 2;
        
        private Harmony Harmony { get; } = new (Id);

        public override void Load()
        {
            GameOptionsData.RecommendedImpostors = GameOptionsData.MaxImpostors = Enumerable.Repeat(127, 127).ToArray();
            GameOptionsData.MinPlayers = Enumerable.Repeat(4, 127).ToArray();

            Harmony.PatchAll();
        }
    }
}

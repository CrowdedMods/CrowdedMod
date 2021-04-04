using System.Linq;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using Reactor;

namespace CrowdedMod {
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class CrowdedModPlugin : BasePlugin
    {
        public const string Id = "pl.przebor.crowded";

        public Harmony Harmony { get; } = new Harmony(Id);

        public override void Load()
        {
            GameOptionsData.RecommendedImpostors = GameOptionsData.MaxImpostors = Enumerable.Repeat(127, 127).ToArray();
            GameOptionsData.MinPlayers = Enumerable.Repeat(4, 127).ToArray();
            RegisterCustomRpcAttribute.Register(this);
            
            Harmony.PatchAll();
        }
    }
}

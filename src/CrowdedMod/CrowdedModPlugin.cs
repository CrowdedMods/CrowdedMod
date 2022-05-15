using System.Linq;
using System.Reflection;
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
        public Harmony Harmony { get; } = new (Id);

        public override void Load()
        {
            GameOptionsData.RecommendedImpostors = GameOptionsData.MaxImpostors = Enumerable.Repeat(127, 127).ToArray();
            GameOptionsData.MinPlayers = Enumerable.Repeat(4, 127).ToArray();

            Harmony.PatchAll();
#if DEBUG
            // Disable regionInfo watcher because it goes BRRR on multiple instances
            var stupidWatcher = typeof(ReactorPlugin).GetProperty("RegionInfoWatcher", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(PluginSingleton<ReactorPlugin>.Instance);
            stupidWatcher!.GetType().GetMethod("Dispose")!.Invoke(stupidWatcher, new object[]{});
#endif
        }
    }
}

using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor;

[BepInPlugin("com.rareshonour.hostguard", "HostGuard", "1.2.0")]
[BepInDependency(ReactorPlugin.Id)]
public class HostGuardPlugin : BasePlugin
{
    public static ManualLogSource Logger { get; private set; } = null!;
    private Harmony _harmony = null!;

    public override void Load()
    {
        Logger = Log;
        HostGuardConfig.Initialize(Config);
        _harmony = new Harmony("com.rareshonour.hostguard");
        _harmony.PatchAll();
        Logger.LogInfo("HostGuard 1.2.0 loaded.");
    }
}
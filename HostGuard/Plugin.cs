using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor;

[BepInPlugin("com.rareshonour.hostguard", "HostGuard", "1.2.1")]
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
        Logger.LogInfo("HostGuard 1.2.1 loaded.");
        Logger.LogInfo($"[HostGuard] Config: BanInsteadOfKick={HostGuardConfig.BanInsteadOfKick.Value}, ContainsMode={HostGuardConfig.ContainsMode.Value}, AnnounceKick={HostGuardConfig.AnnounceKick.Value}");
        Logger.LogInfo($"[HostGuard] Config: BannedWords=[{HostGuardConfig.BannedWords.Value}], BadNameWords=[{HostGuardConfig.BadNameWords.Value}]");
        Logger.LogInfo($"[HostGuard] Config: Whitelist=[{HostGuardConfig.WhitelistedCodes.Value}], BanListUrl={(!string.IsNullOrEmpty(HostGuardConfig.BanListUrl.Value) ? "set" : "not set")}");
    }
}
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor;
using Reactor.Networking.Attributes;

[BepInPlugin("com.rareshonour.hostguard", "HostGuard", "2.0.0")]
[BepInDependency(ReactorPlugin.Id)]
[ReactorModFlags(Reactor.Networking.ModFlags.None)]
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
        Logger.LogInfo("HostGuard 2.0.0 loaded.");
        Logger.LogInfo($"[HostGuard] ChatFilter: Words=[{HostGuardConfig.BannedWords.Value}], Contains={HostGuardConfig.ContainsMode.Value}, Ban={HostGuardConfig.BanForBannedWords.Value}");
        Logger.LogInfo($"[HostGuard] NameFilter: BadWords=[{HostGuardConfig.BadNameWords.Value}], Ban={HostGuardConfig.BanForBadName.Value}");
        Logger.LogInfo($"[HostGuard] NameFilter: DefaultNames={HostGuardConfig.KickDefaultNames.Value}, Ban={HostGuardConfig.BanForDefaultName.Value}");
        Logger.LogInfo($"[HostGuard] Whitelist=[{HostGuardConfig.WhitelistedCodes.Value}], BanList={(!string.IsNullOrEmpty(HostGuardConfig.BanListUrl.Value) ? "set" : "not set")}");
    }
}

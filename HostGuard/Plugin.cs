using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor;
using Reactor.Networking.Attributes;

[BepInPlugin("com.rareshonour.hostguard", "HostGuard", "3.0.0")]
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
        Blacklist.Load();
        _harmony = new Harmony("com.rareshonour.hostguard");
        _harmony.PatchAll();

        Reactor.Patches.ReactorVersionShower.TextUpdated += (text) =>
        {
            text.text += "\nHostGuard 3.0.0";
        };

        Logger.LogInfo("HostGuard 3.0.0 loaded.");
        Logger.LogInfo($"[HostGuard] ChatFilter: Words=[{HostGuardConfig.BannedWords.Value}], Contains={HostGuardConfig.ContainsMode.Value}, Ban={HostGuardConfig.BanForBannedWords.Value}");
        Logger.LogInfo($"[HostGuard] NameFilter: BadWords=[{HostGuardConfig.BadNameWords.Value}], Ban={HostGuardConfig.BanForBadName.Value}");
        Logger.LogInfo($"[HostGuard] NameFilter: DefaultNames={HostGuardConfig.KickDefaultNames.Value}, Ban={HostGuardConfig.BanForDefaultName.Value}");
        Logger.LogInfo($"[HostGuard] AutoStart: Enabled={HostGuardConfig.AutoStartEnabled.Value}, Count={HostGuardConfig.AutoStartPlayerCount.Value}");
        Logger.LogInfo($"[HostGuard] Whitelist=[{HostGuardConfig.WhitelistedCodes.Value}], BanList={(!string.IsNullOrEmpty(HostGuardConfig.BanListUrl.Value) ? "set" : "not set")}, Blacklist={Blacklist.GetAll().Count}");
        Logger.LogInfo($"[HostGuard] BotProtection: Names=[{HostGuardConfig.KnownBotNames.Value}], Ban={HostGuardConfig.BanKnownBots.Value}, Urls=[{HostGuardConfig.KnownBotUrls.Value}]");
        Logger.LogInfo($"[HostGuard] FloodProtection: Enabled={HostGuardConfig.FloodProtectionEnabled.Value}, JoinThreshold={HostGuardConfig.FloodJoinThreshold.Value}/{HostGuardConfig.FloodJoinWindowSeconds.Value}s");
        Logger.LogInfo($"[HostGuard] AntiCheat: Enabled={HostGuardConfig.AntiCheatEnabled.Value}, Ban={HostGuardConfig.BanOnInvalidRpc.Value}");
        Logger.LogInfo($"[HostGuard] CosmeticDetection: Enabled={HostGuardConfig.CosmeticDetectionEnabled.Value}, AutoLock={HostGuardConfig.AutoLockOnFlood.Value}, JoinNotify={HostGuardConfig.VerboseJoinNotifications.Value}");
    }
}

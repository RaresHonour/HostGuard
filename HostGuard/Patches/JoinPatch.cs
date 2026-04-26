using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using InnerNet;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
public static class JoinPatch
{
    static void Postfix(ClientData data)
    {
        if (!AmongUsClient.Instance.AmHost || data == null) return;

        LobbyLock.CheckAutoUnlock();

        string name = data.PlayerName;
        string code = data.FriendCode;
        float level = 0;
        try { level = data.PlayerLevel; } catch { }

        HostGuardPlugin.Logger.LogInfo($"[HostGuard] Player joined: {name} ({code}) [ID: {data.Id}] Level={level}");

        // No friend code = guest/bot
        if (string.IsNullOrEmpty(code) || !code.Contains('#'))
        {
            HostGuardPlugin.Logger.LogWarning($"[HostGuard] Banning {name} — no friend code.");
            ChatHelper.SendLocalMessage($"[Bot] Banned {name} — no friend code.");
            AmongUsClient.Instance.KickPlayer(data.Id, true);
            return;
        }

        // Session blacklist
        if (FloodGuard.IsSessionBanned(code))
        {
            HostGuardPlugin.Logger.LogWarning($"[HostGuard] Banning {name} ({code}) — session blacklisted.");
            AmongUsClient.Instance.KickPlayer(data.Id, true);
            return;
        }

        // Whitelist bypass
        if (HostGuardConfig.GetWhitelistedCodes().Contains(code))
        {
            HostGuardPlugin.Logger.LogInfo($"[HostGuard] Whitelist bypass: {name} ({code})");
            CheckAutoStart();
            return;
        }

        // Known bot name check (gated by BotProtection master toggle)
        var botNames = HostGuardConfig.GetKnownBotNamesList();
        if (HostGuardConfig.BotProtectionEnabled.Value && botNames.Any(b => string.Equals(b, name, System.StringComparison.OrdinalIgnoreCase)))
        {
            bool ban = HostGuardConfig.BanKnownBots.Value;
            HostGuardPlugin.Logger.LogWarning($"[HostGuard] {(ban ? "Banning" : "Kicking")} {name} ({code}) — known bot name.");
            ChatHelper.SendLocalMessage($"[Bot] {(ban ? "Banned" : "Kicked")} known bot: {name}");
            AmongUsClient.Instance.KickPlayer(data.Id, ban);
            return;
        }

        // Flood protection
        if (HostGuardConfig.FloodProtectionEnabled.Value && FloodGuard.ShouldBlockJoin(data.Id))
        {
            HostGuardPlugin.Logger.LogWarning($"[HostGuard] Banning {name} ({code}) — flood protection triggered.");
            ChatHelper.SendLocalMessage($"[Flood] Banned {name} — flood attack.");
            FloodGuard.SessionBan(code);
            AmongUsClient.Instance.KickPlayer(data.Id, true);
            return;
        }

        // Blacklist check (synchronous — no async!)
        if (Blacklist.Contains(code))
        {
            HostGuardPlugin.Logger.LogWarning($"[HostGuard] Banning {name} ({code}) — found in local blacklist.");
            AmongUsClient.Instance.KickPlayer(data.Id, true);
            return;
        }

        // Ban list check (synchronous — use cached data, don't await)
        var banned = BanListManager.GetCachedBannedCodes();
        if (banned != null && banned.Contains(code))
        {
            HostGuardPlugin.Logger.LogWarning($"[HostGuard] Banning {name} ({code}) — found in ban list.");
            AmongUsClient.Instance.KickPlayer(data.Id, true);
            return;
        }

        // Bad name check (gated by NameFilter master toggle)
        List<string> badWords = HostGuardConfig.GetBadNameWordsList();
        if (HostGuardConfig.NameFilterEnabled.Value && badWords.Count > 0)
        {
            string lowerName = name.ToLower();
            string? match = badWords.FirstOrDefault(w => lowerName.Contains(w));
            if (match != null)
            {
                bool ban = HostGuardConfig.BanForBadName.Value;
                HostGuardPlugin.Logger.LogWarning($"[HostGuard] {(ban ? "Banning" : "Kicking")} {name} ({code}) — bad name: '{match}'");
                AmongUsClient.Instance.KickPlayer(data.Id, ban);
                return;
            }
        }

        // Default name check (gated by NameFilter master toggle)
        if (HostGuardConfig.NameFilterEnabled.Value && HostGuardConfig.KickDefaultNames.Value && DefaultNameChecker.IsDefaultName(name))
        {
            bool ban = HostGuardConfig.BanForDefaultName.Value;
            HostGuardPlugin.Logger.LogWarning($"[HostGuard] {(ban ? "Banning" : "Kicking")} {name} ({code}) — default name detected.");
            AmongUsClient.Instance.KickPlayer(data.Id, ban);
            return;
        }

        // Minimum level check
        if (HostGuardConfig.MinLevelEnabled.Value)
        {
            int minLvl = HostGuardConfig.MinLevel.Value;
            if (level < minLvl)
            {
                bool ban = HostGuardConfig.BanForLowLevel.Value;
                HostGuardPlugin.Logger.LogWarning($"[HostGuard] {(ban ? "Banning" : "Kicking")} {name} ({code}) — level {level} below minimum {minLvl}.");
                ChatHelper.SendLocalMessage($"[Level] {(ban ? "Banned" : "Kicked")} {name} — level {level} < {minLvl}");
                AmongUsClient.Instance.KickPlayer(data.Id, ban);
                return;
            }
        }

        // Player passed all checks
        CheckAutoStart();
    }

    static void CheckAutoStart()
    {
        if (!HostGuardConfig.AutoStartEnabled.Value) return;

        int target = HostGuardConfig.AutoStartPlayerCount.Value;
        if (target <= 0)
        {
            var options = GameOptionsManager.Instance?.CurrentGameOptions;
            if (options != null)
                target = options.MaxPlayers;
            else
                return;
        }

        int current = GameData.Instance != null ? GameData.Instance.PlayerCount : 0;
        if (current >= target)
        {
            var gsm = GameStartManager.Instance;
            if (gsm != null && gsm.countDownTimer > 5f)
            {
                HostGuardPlugin.Logger.LogInfo($"[HostGuard] Auto-starting: {current}/{target} players reached.");
                gsm.countDownTimer = 5f;
            }
        }
    }
}

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

        string name = data.PlayerName;
        string code = data.FriendCode;

        HostGuardPlugin.Logger.LogInfo($"[HostGuard] Player joined: {name} ({code}) [ID: {data.Id}]");

        if (HostGuardConfig.GetWhitelistedCodes().Contains(code))
        {
            HostGuardPlugin.Logger.LogInfo($"[HostGuard] Whitelist bypass: {name} ({code})");
            CheckAutoStart();
            return;
        }

        _ = CheckJoinAsync(data, name, code);
    }

    static async System.Threading.Tasks.Task CheckJoinAsync(ClientData data, string name, string code)
    {
        try
        {
            // Blacklist check
            if (Blacklist.Contains(code))
            {
                HostGuardPlugin.Logger.LogWarning($"[HostGuard] Kicking {name} ({code}) — found in local blacklist.");
                AmongUsClient.Instance.KickPlayer(data.Id, true);
                return;
            }

            // Ban list check
            var banned = await BanListManager.FetchBannedCodesAsync();
            if (banned.Contains(code))
            {
                HostGuardPlugin.Logger.LogWarning($"[HostGuard] Banning {name} ({code}) — found in ban list.");
                AmongUsClient.Instance.KickPlayer(data.Id, true);
                return;
            }

            // Bad name check
            List<string> badWords = HostGuardConfig.GetBadNameWordsList();
            if (badWords.Count > 0)
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

            // Default name check
            if (HostGuardConfig.KickDefaultNames.Value && DefaultNameChecker.IsDefaultName(name))
            {
                bool ban = HostGuardConfig.BanForDefaultName.Value;
                HostGuardPlugin.Logger.LogWarning($"[HostGuard] {(ban ? "Banning" : "Kicking")} {name} ({code}) — default name detected.");
                AmongUsClient.Instance.KickPlayer(data.Id, ban);
                return;
            }

            // Player passed all checks — check autostart
            CheckAutoStart();
        }
        catch (System.Exception ex)
        {
            HostGuardPlugin.Logger.LogError($"[HostGuard] Error checking player {name} ({code}): {ex.Message}");
        }
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

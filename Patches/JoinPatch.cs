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
            return;
        }

        _ = CheckBanListAsync(data, name, code);

        List<string> badWords = HostGuardConfig.GetBadNameWordsList();
        if (badWords.Count > 0)
        {
            string lowerName = name.ToLower();
            string? match = badWords.FirstOrDefault(w => lowerName.Contains(w));
            if (match != null)
            {
                HostGuardPlugin.Logger.LogWarning($"[HostGuard] Kicking {name} ({code}) — bad name: '{match}'");
                AmongUsClient.Instance.KickPlayer(data.Id, false);
            }
        }
    }

    static async System.Threading.Tasks.Task CheckBanListAsync(ClientData data, string name, string code)
    {
        var banned = await BanListManager.FetchBannedCodesAsync();
        HostGuardPlugin.Logger.LogInfo($"[HostGuard] Ban list loaded: {banned.Count} codes");
        if (banned.Contains(code))
        {
            HostGuardPlugin.Logger.LogWarning($"[HostGuard] Banning {name} ({code}) — found in ban list.");
            AmongUsClient.Instance.KickPlayer(data.Id, true);
        }
    }
}
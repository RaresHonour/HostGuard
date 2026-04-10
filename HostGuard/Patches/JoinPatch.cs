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

        _ = CheckJoinAsync(data, name, code);
    }

    static async System.Threading.Tasks.Task CheckJoinAsync(ClientData data, string name, string code)
    {
        var banned = await BanListManager.FetchBannedCodesAsync();
        if (banned.Contains(code))
        {
            HostGuardPlugin.Logger.LogWarning($"[HostGuard] Banning {name} ({code}) — found in ban list.");
            AmongUsClient.Instance.KickPlayer(data.Id, true);
            return;
        }

        List<string> badWords = HostGuardConfig.GetBadNameWordsList();
        if (badWords.Count > 0)
        {
            string lowerName = name.ToLower();
            string? match = badWords.FirstOrDefault(w => lowerName.Contains(w));
            if (match != null)
            {
                bool ban = HostGuardConfig.BanInsteadOfKick.Value;
                HostGuardPlugin.Logger.LogWarning($"[HostGuard] {(ban ? "Banning" : "Kicking")} {name} ({code}) — bad name: '{match}'");
                AmongUsClient.Instance.KickPlayer(data.Id, ban);
            }
        }
    }
}
using System;
using HarmonyLib;
using InnerNet;

/// <summary>
/// CreatePlayer prefix — blocks player creation for banned/invalid players only.
/// </summary>
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CreatePlayer))]
public static class CreatePlayerPatch
{
    static bool Prefix(ClientData clientData)
    {
        try
        {
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
                return true;

            string name = clientData?.PlayerName ?? "?";
            string code = clientData?.FriendCode ?? "?";
            int id = clientData?.Id ?? -1;
            CrashLog.Write($"[CreatePlayer] {name} ({code}) [ID: {id}]");

            if (string.IsNullOrEmpty(code) || !code.Contains('#'))
            {
                CrashLog.Write($"[CreatePlayer] BLOCKED — no friend code");
                AmongUsClient.Instance.KickPlayer(id, true);
                return false;
            }
            if (FloodGuard.IsSessionBanned(code))
            {
                CrashLog.Write($"[CreatePlayer] BLOCKED — session banned");
                AmongUsClient.Instance.KickPlayer(id, true);
                return false;
            }
            if (Blacklist.Contains(code))
            {
                CrashLog.Write($"[CreatePlayer] BLOCKED — blacklisted");
                AmongUsClient.Instance.KickPlayer(id, true);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            CrashLog.Write($"[CreatePlayer] ERROR: {ex.Message}");
            return true;
        }
    }

    static void Postfix(ClientData clientData)
    {
        try { CrashLog.Write($"[CreatePlayer] COMPLETED: {clientData?.PlayerName ?? "?"}"); }
        catch { }
    }
}

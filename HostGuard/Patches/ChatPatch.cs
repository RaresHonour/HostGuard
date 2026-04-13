using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
public static class ChatPatch
{
    static void Postfix(PlayerControl sourcePlayer, string chatText)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (sourcePlayer == null || sourcePlayer.AmOwner) return;
        if (AmongUsClient.Instance.IsGameStarted) return;

        string friendCode = sourcePlayer.Data.FriendCode;
        if (HostGuardConfig.GetWhitelistedCodes().Contains(friendCode)) return;

        string msg = chatText.ToLower().Trim();
        HostGuardPlugin.Logger.LogInfo($"[HostGuard] Chat from {sourcePlayer.Data.PlayerName} ({friendCode}): {chatText}");
        List<string> banned = HostGuardConfig.GetBannedWordsList();

        bool triggered = HostGuardConfig.ContainsMode.Value
            ? banned.Any(w => msg.Contains(w))
            : banned.Contains(msg);

        if (!triggered) return;

        bool ban = HostGuardConfig.BanForBannedWords.Value;
        HostGuardPlugin.Logger.LogWarning($"[HostGuard] {(ban ? "Banned" : "Kicked")} {sourcePlayer.Data.PlayerName} ({friendCode}) for: '{chatText}'");

        var client = AmongUsClient.Instance.GetClient(sourcePlayer.OwnerId);
        if (client != null)
            AmongUsClient.Instance.KickPlayer(client.Id, ban);
    }
}

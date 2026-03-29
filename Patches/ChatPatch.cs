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

        string msg = chatText.ToLower().Trim();
        HostGuardPlugin.Logger.LogInfo($"[HostGuard] Chat from {sourcePlayer.Data.PlayerName} ({sourcePlayer.Data.FriendCode}): {chatText}");
        List<string> banned = HostGuardConfig.GetBannedWordsList();

        bool triggered = HostGuardConfig.ContainsMode.Value
            ? banned.Any(w => msg.Contains(w))
            : banned.Contains(msg);

        if (!triggered) return;

        string friendCode = sourcePlayer.Data.FriendCode;
        if (HostGuardConfig.GetWhitelistedCodes().Contains(friendCode)) return;

        HostGuardPlugin.Logger.LogWarning($"[HostGuard] Kicked {sourcePlayer.Data.PlayerName} ({friendCode}) for: '{chatText}'");

        var client = AmongUsClient.Instance.GetClient(sourcePlayer.OwnerId);
        if (client != null)
            AmongUsClient.Instance.KickPlayer(client.Id, HostGuardConfig.BanInsteadOfKick.Value);
    }
}
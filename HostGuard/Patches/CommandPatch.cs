using System.Linq;
using HarmonyLib;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
public static class CommandPatch
{
    static void Postfix(PlayerControl sourcePlayer, string chatText)
    {
        if (!sourcePlayer.AmOwner || !AmongUsClient.Instance.AmHost) return;

        string msg = chatText.Trim();
        string lower = msg.ToLower();

        if (lower.StartsWith("!allowcode "))
            HandleAllowCode(msg.Substring("!allowcode ".Length).Trim());
        else if (lower.StartsWith("!removecode "))
            HandleRemoveCode(msg.Substring("!removecode ".Length).Trim());
        else if (lower.StartsWith("!allow "))
            HandleAllow(msg.Substring("!allow ".Length).Trim());
        else if (lower.StartsWith("!remove "))
            HandleRemove(msg.Substring("!remove ".Length).Trim());
    }

    static void HandleAllow(string name)
    {
        var target = PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(p => p.Data.PlayerName.Equals(name, System.StringComparison.OrdinalIgnoreCase));
        if (target == null) { HostGuardPlugin.Logger.LogWarning($"[HostGuard] !allow: '{name}' not found."); return; }
        HostGuardConfig.AddToWhitelist(target.Data.FriendCode);
        HostGuardPlugin.Logger.LogInfo($"[HostGuard] Added {name} ({target.Data.FriendCode}) to whitelist.");
    }

    static void HandleRemove(string name)
    {
        var target = PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(p => p.Data.PlayerName.Equals(name, System.StringComparison.OrdinalIgnoreCase));
        if (target == null) { HostGuardPlugin.Logger.LogWarning($"[HostGuard] !remove: '{name}' not found."); return; }
        HostGuardConfig.RemoveFromWhitelist(target.Data.FriendCode);
        HostGuardPlugin.Logger.LogInfo($"[HostGuard] Removed {name} ({target.Data.FriendCode}) from whitelist.");
    }

    static void HandleAllowCode(string friendCode)
    {
        if (string.IsNullOrWhiteSpace(friendCode)) { HostGuardPlugin.Logger.LogWarning("[HostGuard] !allowcode: no code provided."); return; }
        HostGuardConfig.AddToWhitelist(friendCode);
        HostGuardPlugin.Logger.LogInfo($"[HostGuard] Added {friendCode} to whitelist.");
    }

    static void HandleRemoveCode(string friendCode)
    {
        if (string.IsNullOrWhiteSpace(friendCode)) { HostGuardPlugin.Logger.LogWarning("[HostGuard] !removecode: no code provided."); return; }
        HostGuardConfig.RemoveFromWhitelist(friendCode);
        HostGuardPlugin.Logger.LogInfo($"[HostGuard] Removed {friendCode} from whitelist.");
    }
}
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

        // Whitelist commands
        if (lower.StartsWith("!allowcode "))
            HandleAllowCode(msg.Substring("!allowcode ".Length).Trim());
        else if (lower.StartsWith("!removecode "))
            HandleRemoveCode(msg.Substring("!removecode ".Length).Trim());
        else if (lower.StartsWith("!allow "))
            HandleAllow(msg.Substring("!allow ".Length).Trim());
        else if (lower.StartsWith("!remove "))
            HandleRemove(msg.Substring("!remove ".Length).Trim());
        // Manual kick/ban
        else if (lower.StartsWith("!kick "))
            HandleManualKick(msg.Substring("!kick ".Length).Trim(), false);
        else if (lower.StartsWith("!ban "))
            HandleManualKick(msg.Substring("!ban ".Length).Trim(), true);
        // Toggle commands
        else if (lower == "!defaultnames on")
            SetBool(HostGuardConfig.KickDefaultNames, true, "Default name filter enabled.");
        else if (lower == "!defaultnames off")
            SetBool(HostGuardConfig.KickDefaultNames, false, "Default name filter disabled.");
        else if (lower == "!badnames on")
            SetBool(HostGuardConfig.BanForBadName, true, "Bad names will now be BANNED.");
        else if (lower == "!badnames off")
            SetBool(HostGuardConfig.BanForBadName, false, "Bad names will now be kicked (not banned).");
        else if (lower == "!badchat on")
            SetBool(HostGuardConfig.BanForBannedWords, true, "Banned words in chat will now result in a BAN.");
        else if (lower == "!badchat off")
            SetBool(HostGuardConfig.BanForBannedWords, false, "Banned words in chat will now result in a kick.");
        else if (lower == "!defaultnames ban")
            SetBool(HostGuardConfig.BanForDefaultName, true, "Default names will now be BANNED.");
        else if (lower == "!defaultnames kick")
            SetBool(HostGuardConfig.BanForDefaultName, false, "Default names will now be kicked (not banned).");
        else if (lower == "!contains on")
            SetBool(HostGuardConfig.ContainsMode, true, "Contains mode ON: messages containing a banned word will trigger.");
        else if (lower == "!contains off")
            SetBool(HostGuardConfig.ContainsMode, false, "Contains mode OFF: only exact matches will trigger.");
        // Rules commands
        else if (lower == "!rules")
            ShowRules();
        else if (lower.StartsWith("!setrules "))
            SetRules(msg.Substring("!setrules ".Length).Trim());
        // Whitelist view
        else if (lower == "!whitelist")
            ShowWhitelist();
        // Status
        else if (lower == "!status")
            ShowStatus();
        else if (lower == "!help")
            ShowHelp();
    }

    static void SetBool(BepInEx.Configuration.ConfigEntry<bool> entry, bool value, string message)
    {
        entry.Value = value;
        HostGuardPlugin.Logger.LogInfo($"[HostGuard] {message}");
        ChatHelper.SendLocalMessage(message);
    }

    static void ShowRules()
    {
        string rules = HostGuardConfig.RulesMessage.Value;
        ChatHelper.SendLocalMessage(rules);
    }

    static void SetRules(string newRules)
    {
        if (string.IsNullOrWhiteSpace(newRules))
        {
            ChatHelper.SendLocalMessage("Usage: !setrules <message>");
            return;
        }
        HostGuardConfig.RulesMessage.Value = newRules;
        ChatHelper.SendLocalMessage($"Rules updated: {newRules}");
    }

    static void ShowStatus()
    {
        string status = $"[HostGuard Status]\n" +
            $"Default name filter: {(HostGuardConfig.KickDefaultNames.Value ? "ON" : "OFF")} ({(HostGuardConfig.BanForDefaultName.Value ? "ban" : "kick")})\n" +
            $"Bad name filter: {(HostGuardConfig.GetBadNameWordsList().Count > 0 ? "ON" : "OFF")} ({(HostGuardConfig.BanForBadName.Value ? "ban" : "kick")})\n" +
            $"Chat filter: {(HostGuardConfig.GetBannedWordsList().Count > 0 ? "ON" : "OFF")} ({(HostGuardConfig.BanForBannedWords.Value ? "ban" : "kick")})\n" +
            $"Contains mode: {(HostGuardConfig.ContainsMode.Value ? "ON" : "OFF")}\n" +
            $"Ban list: {(!string.IsNullOrEmpty(HostGuardConfig.BanListUrl.Value) ? "set" : "not set")}\n" +
            $"Whitelisted: {HostGuardConfig.GetWhitelistedCodes().Count} players";
        ChatHelper.SendLocalMessage(status);
    }

    static void ShowHelp()
    {
        string help = "[HostGuard Commands]\n" +
            "!kick <name|code> - Kick a player\n" +
            "!ban <name|code> - Ban a player\n" +
            "!status - Show current settings\n" +
            "!rules - Show rules message\n" +
            "!setrules <msg> - Set rules message\n" +
            "!defaultnames on/off - Toggle default name filter\n" +
            "!defaultnames ban/kick - Set default name action\n" +
            "!badnames on/off - Toggle ban for bad names\n" +
            "!badchat on/off - Toggle ban for banned words\n" +
            "!contains on/off - Toggle contains mode\n" +
            "!whitelist - Show whitelisted players\n" +
            "!allow <name> - Whitelist a player\n" +
            "!remove <name> - Remove from whitelist\n" +
            "!allowcode <code> - Whitelist by friend code\n" +
            "!removecode <code> - Remove by friend code";
        ChatHelper.SendLocalMessage(help);
    }

    static void HandleManualKick(string identifier, bool ban)
    {
        string action = ban ? "Banned" : "Kicked";

        // Try by friend code first
        var byCode = PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(p => p.Data.FriendCode.Equals(identifier, System.StringComparison.OrdinalIgnoreCase));

        // Fall back to name
        var target = byCode ?? PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(p => p.Data.PlayerName.Equals(identifier, System.StringComparison.OrdinalIgnoreCase));

        if (target == null)
        {
            ChatHelper.SendLocalMessage($"Player '{identifier}' not found.");
            return;
        }

        if (target.AmOwner)
        {
            ChatHelper.SendLocalMessage("You can't kick yourself.");
            return;
        }

        var client = AmongUsClient.Instance.GetClient(target.OwnerId);
        if (client == null)
        {
            ChatHelper.SendLocalMessage($"Could not find connection for {target.Data.PlayerName}.");
            return;
        }

        AmongUsClient.Instance.KickPlayer(client.Id, ban);
        ChatHelper.SendLocalMessage($"{action} {target.Data.PlayerName} ({target.Data.FriendCode}).");
    }

    static void ShowWhitelist()
    {
        var codes = HostGuardConfig.GetWhitelistedCodes();
        if (codes.Count == 0)
        {
            ChatHelper.SendLocalMessage("Whitelist is empty.");
            return;
        }
        ChatHelper.SendLocalMessage($"[Whitelisted ({codes.Count})]\n{string.Join("\n", codes)}");
    }

    static void HandleAllow(string name)
    {
        var target = PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(p => p.Data.PlayerName.Equals(name, System.StringComparison.OrdinalIgnoreCase));
        if (target == null)
        {
            ChatHelper.SendLocalMessage($"Player '{name}' not found.");
            return;
        }
        HostGuardConfig.AddToWhitelist(target.Data.FriendCode);
        ChatHelper.SendLocalMessage($"Added {name} ({target.Data.FriendCode}) to whitelist.");
    }

    static void HandleRemove(string name)
    {
        var target = PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(p => p.Data.PlayerName.Equals(name, System.StringComparison.OrdinalIgnoreCase));
        if (target == null)
        {
            ChatHelper.SendLocalMessage($"Player '{name}' not found.");
            return;
        }
        HostGuardConfig.RemoveFromWhitelist(target.Data.FriendCode);
        ChatHelper.SendLocalMessage($"Removed {name} ({target.Data.FriendCode}) from whitelist.");
    }

    static void HandleAllowCode(string friendCode)
    {
        if (string.IsNullOrWhiteSpace(friendCode))
        {
            ChatHelper.SendLocalMessage("Usage: !allowcode <CODE#XXXX>");
            return;
        }
        HostGuardConfig.AddToWhitelist(friendCode);
        ChatHelper.SendLocalMessage($"Added {friendCode} to whitelist.");
    }

    static void HandleRemoveCode(string friendCode)
    {
        if (string.IsNullOrWhiteSpace(friendCode))
        {
            ChatHelper.SendLocalMessage("Usage: !removecode <CODE#XXXX>");
            return;
        }
        HostGuardConfig.RemoveFromWhitelist(friendCode);
        ChatHelper.SendLocalMessage($"Removed {friendCode} from whitelist.");
    }
}

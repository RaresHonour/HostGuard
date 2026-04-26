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

        // Commands with arguments
        string args;
        if (TryGetArgs(lower, msg, "!kick", "!k", out args))
            HandleManualKick(args, false);
        else if (TryGetArgs(lower, msg, "!ban", "!b", out args))
            HandleManualKick(args, true);
        else if (TryGetArgs(lower, msg, "!whitelist", "!wl", out args))
            HandleWhitelist(args);
        else if (TryGetArgs(lower, msg, "!unwhitelist", "!uwl", out args))
            HandleUnwhitelist(args);
        else if (TryGetArgs(lower, msg, "!blacklist", "!bl", out args))
            HandleBlacklist(args);
        else if (TryGetArgs(lower, msg, "!unblacklist", "!ubl", out args))
            HandleUnblacklist(args);
        else if (TryGetArgs(lower, msg, "!info", "!i", out args))
            HandleInfo(args);
        else if (TryGetArgs(lower, msg, "!setrules", "!sr", out args))
            SetRules(args);
        else if (TryGetArgs(lower, msg, "!autostart", "!as", out args))
            HandleAutoStart(args);
        else if (TryGetArgs(lower, msg, "!addword", "!aw", out args))
            HandleAddWord(args);
        else if (TryGetArgs(lower, msg, "!removeword", "!rw", out args))
            HandleRemoveWord(args);
        else if (TryGetArgs(lower, msg, "!addname", "!an", out args))
            HandleAddName(args);
        else if (TryGetArgs(lower, msg, "!removename", "!rn", out args))
            HandleRemoveName(args);
        // Exact match commands (no args)
        else if (Cmd(lower, "!lock", "!lk"))
            HandleLock();
        else if (Cmd(lower, "!unlock", "!ulk"))
            HandleUnlock();
        else if (Cmd(lower, "!whitelist", "!wl"))
            ShowWhitelist();
        else if (Cmd(lower, "!blacklist", "!bl"))
            ShowBlacklist();
        else if (Cmd(lower, "!kickall", "!ka"))
            HandleKickAll();
        else if (Cmd(lower, "!status", "!s"))
            ShowStatus();
        else if (Cmd(lower, "!help", "!h"))
            ShowHelp();
        else if (Cmd(lower, "!rules", "!r"))
            ShowRules();
        else if (Cmd(lower, "!words", "!wds"))
            ShowWords();
        else if (Cmd(lower, "!namelist", "!nl"))
            ShowNameList();
        // Toggle commands
        else if (lower == "!defaultnames on" || lower == "!dn on")
            SetBool(HostGuardConfig.KickDefaultNames, true, "Default name filter enabled.");
        else if (lower == "!defaultnames off" || lower == "!dn off")
            SetBool(HostGuardConfig.KickDefaultNames, false, "Default name filter disabled.");
        else if (lower == "!defaultnames ban" || lower == "!dn ban")
            SetBool(HostGuardConfig.BanForDefaultName, true, "Default names will now be BANNED.");
        else if (lower == "!defaultnames kick" || lower == "!dn kick")
            SetBool(HostGuardConfig.BanForDefaultName, false, "Default names will now be kicked (not banned).");
        else if (lower == "!badnames on" || lower == "!bn on")
            SetBool(HostGuardConfig.BanForBadName, true, "Bad names will now be BANNED.");
        else if (lower == "!badnames off" || lower == "!bn off")
            SetBool(HostGuardConfig.BanForBadName, false, "Bad names will now be kicked (not banned).");
        else if (lower == "!badchat on" || lower == "!bc on")
            SetBool(HostGuardConfig.BanForBannedWords, true, "Banned words in chat will now result in a BAN.");
        else if (lower == "!badchat off" || lower == "!bc off")
            SetBool(HostGuardConfig.BanForBannedWords, false, "Banned words in chat will now result in a kick.");
        else if (lower == "!contains on" || lower == "!cm on")
            SetBool(HostGuardConfig.ContainsMode, true, "Contains mode ON: messages containing a banned word will trigger.");
        else if (lower == "!contains off" || lower == "!cm off")
            SetBool(HostGuardConfig.ContainsMode, false, "Contains mode OFF: only exact matches will trigger.");
        // Bot protection toggles
        else if (lower == "!botnames on" || lower == "!bot on")
            SetBool(HostGuardConfig.BanKnownBots, true, "Known bots will now be BANNED.");
        else if (lower == "!botnames off" || lower == "!bot off")
            SetBool(HostGuardConfig.BanKnownBots, false, "Known bots will now be kicked (not banned).");
        // Flood protection toggles
        else if (lower == "!flood on" || lower == "!fp on")
            SetBool(HostGuardConfig.FloodProtectionEnabled, true, "Flood protection enabled.");
        else if (lower == "!flood off" || lower == "!fp off")
            SetBool(HostGuardConfig.FloodProtectionEnabled, false, "Flood protection disabled.");
        // Anti-cheat toggles
        else if (lower == "!anticheat on" || lower == "!ac on")
            SetBool(HostGuardConfig.AntiCheatEnabled, true, "Anti-cheat enabled.");
        else if (lower == "!anticheat off" || lower == "!ac off")
            SetBool(HostGuardConfig.AntiCheatEnabled, false, "Anti-cheat disabled.");
        else if (lower == "!anticheat ban" || lower == "!ac ban")
            SetBool(HostGuardConfig.BanOnInvalidRpc, true, "Cheaters will now be BANNED.");
        else if (lower == "!anticheat kick" || lower == "!ac kick")
            SetBool(HostGuardConfig.BanOnInvalidRpc, false, "Cheaters will now be kicked (not banned).");
        // Cosmetic detection toggles
        else if (lower == "!cosmetic on" || lower == "!cos on")
            SetBool(HostGuardConfig.CosmeticDetectionEnabled, true, "Cosmetic bot detection enabled.");
        else if (lower == "!cosmetic off" || lower == "!cos off")
            SetBool(HostGuardConfig.CosmeticDetectionEnabled, false, "Cosmetic bot detection disabled.");
        // Auto-lock toggles
        else if (lower == "!autolock on" || lower == "!al on")
            SetBool(HostGuardConfig.AutoLockOnFlood, true, "Auto-lock on flood enabled.");
        else if (lower == "!autolock off" || lower == "!al off")
            SetBool(HostGuardConfig.AutoLockOnFlood, false, "Auto-lock on flood disabled.");
        // Join notifications toggles
        else if (lower == "!notify on" || lower == "!n on")
            SetBool(HostGuardConfig.VerboseJoinNotifications, true, "Verbose join notifications enabled.");
        else if (lower == "!notify off" || lower == "!n off")
            SetBool(HostGuardConfig.VerboseJoinNotifications, false, "Verbose join notifications disabled.");
    }

    // --- Helpers ---

    static bool Cmd(string lower, string full, string alias)
        => lower == full || lower == alias;

    static bool TryGetArgs(string lower, string msg, string full, string alias, out string args)
    {
        if (lower.StartsWith(full + " "))
        {
            args = msg.Substring(full.Length + 1).Trim();
            return args.Length > 0;
        }
        if (lower.StartsWith(alias + " "))
        {
            args = msg.Substring(alias.Length + 1).Trim();
            return args.Length > 0;
        }
        args = "";
        return false;
    }

    static bool IsFriendCode(string input) => input.Contains('#');

    static PlayerControl FindPlayer(string identifier)
    {
        if (IsFriendCode(identifier))
            return PlayerControl.AllPlayerControls.ToArray()
                .FirstOrDefault(p => p.Data.FriendCode.Equals(identifier, System.StringComparison.OrdinalIgnoreCase));

        return PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(p => p.Data.PlayerName.Equals(identifier, System.StringComparison.OrdinalIgnoreCase));
    }

    static void SetBool(BepInEx.Configuration.ConfigEntry<bool> entry, bool value, string message)
    {
        entry.Value = value;
        HostGuardPlugin.Logger.LogInfo($"[HostGuard] {message}");
        ChatHelper.SendLocalMessage(message);
    }

    // --- Lobby lock ---

    static void HandleLock()
    {
        LobbyLock.Lock();
        ChatHelper.SendLocalMessage("Lobby locked (set to private).");
    }

    static void HandleUnlock()
    {
        LobbyLock.Unlock();
        ChatHelper.SendLocalMessage("Lobby unlocked (set to public).");
    }

    // --- Manual kick/ban ---

    static void HandleManualKick(string identifier, bool ban)
    {
        var target = FindPlayer(identifier);
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

        string action = ban ? "Banned" : "Kicked";
        AmongUsClient.Instance.KickPlayer(client.Id, ban);
        ChatHelper.SendLocalMessage($"{action} {target.Data.PlayerName} ({target.Data.FriendCode}).");
    }

    static void HandleKickAll()
    {
        var players = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => !p.AmOwner)
            .ToList();

        if (players.Count == 0)
        {
            ChatHelper.SendLocalMessage("No players to kick.");
            return;
        }

        int count = 0;
        foreach (var player in players)
        {
            var client = AmongUsClient.Instance.GetClient(player.OwnerId);
            if (client != null)
            {
                AmongUsClient.Instance.KickPlayer(client.Id, false);
                count++;
            }
        }
        ChatHelper.SendLocalMessage($"Kicked {count} player(s).");
    }

    // --- Whitelist ---

    static void HandleWhitelist(string identifier)
    {
        if (IsFriendCode(identifier))
        {
            HostGuardConfig.AddToWhitelist(identifier);
            ChatHelper.SendLocalMessage($"Added {identifier} to whitelist.");
            return;
        }

        var target = FindPlayer(identifier);
        if (target == null)
        {
            ChatHelper.SendLocalMessage($"Player '{identifier}' not found in lobby.");
            return;
        }
        HostGuardConfig.AddToWhitelist(target.Data.FriendCode);
        ChatHelper.SendLocalMessage($"Added {target.Data.PlayerName} ({target.Data.FriendCode}) to whitelist.");
    }

    static void HandleUnwhitelist(string identifier)
    {
        if (IsFriendCode(identifier))
        {
            if (HostGuardConfig.RemoveFromWhitelist(identifier))
                ChatHelper.SendLocalMessage($"Removed {identifier} from whitelist.");
            else
                ChatHelper.SendLocalMessage($"{identifier} is not whitelisted.");
            return;
        }

        var target = FindPlayer(identifier);
        if (target == null)
        {
            ChatHelper.SendLocalMessage($"Player '{identifier}' not found in lobby.");
            return;
        }
        if (HostGuardConfig.RemoveFromWhitelist(target.Data.FriendCode))
            ChatHelper.SendLocalMessage($"Removed {target.Data.PlayerName} ({target.Data.FriendCode}) from whitelist.");
        else
            ChatHelper.SendLocalMessage($"{target.Data.PlayerName} is not whitelisted.");
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

    // --- Blacklist ---

    static void HandleBlacklist(string identifier)
    {
        if (IsFriendCode(identifier))
        {
            if (Blacklist.Add(identifier))
                ChatHelper.SendLocalMessage($"Added {identifier} to blacklist.");
            else
                ChatHelper.SendLocalMessage($"{identifier} is already blacklisted.");
            return;
        }

        var target = FindPlayer(identifier);
        if (target == null)
        {
            ChatHelper.SendLocalMessage($"Player '{identifier}' not found in lobby.");
            return;
        }
        if (Blacklist.Add(target.Data.FriendCode))
            ChatHelper.SendLocalMessage($"Added {target.Data.PlayerName} ({target.Data.FriendCode}) to blacklist.");
        else
            ChatHelper.SendLocalMessage($"{target.Data.PlayerName} is already blacklisted.");
    }

    static void HandleUnblacklist(string identifier)
    {
        if (IsFriendCode(identifier))
        {
            if (Blacklist.Remove(identifier))
                ChatHelper.SendLocalMessage($"Removed {identifier} from blacklist.");
            else
                ChatHelper.SendLocalMessage($"{identifier} is not blacklisted.");
            return;
        }

        var target = FindPlayer(identifier);
        if (target == null)
        {
            ChatHelper.SendLocalMessage($"Player '{identifier}' not found in lobby.");
            return;
        }
        if (Blacklist.Remove(target.Data.FriendCode))
            ChatHelper.SendLocalMessage($"Removed {target.Data.PlayerName} ({target.Data.FriendCode}) from blacklist.");
        else
            ChatHelper.SendLocalMessage($"{target.Data.PlayerName} is not blacklisted.");
    }

    static void ShowBlacklist()
    {
        var all = Blacklist.GetAll();
        if (all.Count == 0)
        {
            ChatHelper.SendLocalMessage("Blacklist is empty.");
            return;
        }
        ChatHelper.SendLocalMessage($"[Blacklisted ({all.Count})]\n{string.Join("\n", all)}");
    }

    // --- Info ---

    static void HandleInfo(string identifier)
    {
        var target = FindPlayer(identifier);
        if (target == null)
        {
            ChatHelper.SendLocalMessage($"Player '{identifier}' not found.");
            return;
        }

        var data = target.Data;
        string info = $"[Player Info]\n" +
            $"Name: {data.PlayerName}\n" +
            $"Friend Code: {data.FriendCode}\n" +
            $"Player ID: {data.PlayerId}";
        ChatHelper.SendLocalMessage(info);
    }

    // --- Rules ---

    static void ShowRules()
    {
        ChatHelper.SendLocalMessage(HostGuardConfig.RulesMessage.Value);
    }

    static void SetRules(string newRules)
    {
        HostGuardConfig.RulesMessage.Value = newRules;
        ChatHelper.SendLocalMessage($"Rules updated: {newRules}");
    }

    // --- AutoStart ---

    static void HandleAutoStart(string args)
    {
        string lower = args.ToLower();
        if (lower == "on")
        {
            SetBool(HostGuardConfig.AutoStartEnabled, true, "Auto-start enabled.");
            return;
        }
        if (lower == "off")
        {
            SetBool(HostGuardConfig.AutoStartEnabled, false, "Auto-start disabled.");
            return;
        }

        if (int.TryParse(args, out int count) && count > 0)
        {
            HostGuardConfig.AutoStartPlayerCount.Value = count;
            HostGuardConfig.AutoStartEnabled.Value = true;
            ChatHelper.SendLocalMessage($"Auto-start enabled at {count} players.");
        }
        else
        {
            ChatHelper.SendLocalMessage("Usage: !autostart on/off or !autostart <number>");
        }
    }

    // --- Word management ---

    static void HandleAddWord(string word)
    {
        if (HostGuardConfig.AddBannedWord(word))
            ChatHelper.SendLocalMessage($"Added '{word}' to banned words.");
        else
            ChatHelper.SendLocalMessage($"'{word}' is already in banned words.");
    }

    static void HandleRemoveWord(string word)
    {
        if (HostGuardConfig.RemoveBannedWord(word))
            ChatHelper.SendLocalMessage($"Removed '{word}' from banned words.");
        else
            ChatHelper.SendLocalMessage($"'{word}' not found in banned words.");
    }

    static void HandleAddName(string word)
    {
        if (HostGuardConfig.AddBadNameWord(word))
            ChatHelper.SendLocalMessage($"Added '{word}' to bad name words.");
        else
            ChatHelper.SendLocalMessage($"'{word}' is already in bad name words.");
    }

    static void HandleRemoveName(string word)
    {
        if (HostGuardConfig.RemoveBadNameWord(word))
            ChatHelper.SendLocalMessage($"Removed '{word}' from bad name words.");
        else
            ChatHelper.SendLocalMessage($"'{word}' not found in bad name words.");
    }

    static void ShowWords()
    {
        var words = HostGuardConfig.GetBannedWordsList();
        if (words.Count == 0)
        {
            ChatHelper.SendLocalMessage("Banned words list is empty.");
            return;
        }
        ChatHelper.SendLocalMessage($"[Banned Words ({words.Count})]\n{string.Join(", ", words)}");
    }

    static void ShowNameList()
    {
        var words = HostGuardConfig.GetBadNameWordsList();
        if (words.Count == 0)
        {
            ChatHelper.SendLocalMessage("Bad name words list is empty.");
            return;
        }
        ChatHelper.SendLocalMessage($"[Bad Name Words ({words.Count})]\n{string.Join(", ", words)}");
    }

    // --- Status ---

    static void ShowStatus()
    {
        int autoCount = HostGuardConfig.AutoStartPlayerCount.Value;
        string autoTarget = autoCount > 0 ? $"{autoCount}" : "lobby max";
        string status = $"[HostGuard Status]\n" +
            $"Default name filter: {(HostGuardConfig.KickDefaultNames.Value ? "ON" : "OFF")} ({(HostGuardConfig.BanForDefaultName.Value ? "ban" : "kick")})\n" +
            $"Bad name filter: {(HostGuardConfig.GetBadNameWordsList().Count > 0 ? "ON" : "OFF")} ({(HostGuardConfig.BanForBadName.Value ? "ban" : "kick")})\n" +
            $"Chat filter: {(HostGuardConfig.GetBannedWordsList().Count > 0 ? "ON" : "OFF")} ({(HostGuardConfig.BanForBannedWords.Value ? "ban" : "kick")})\n" +
            $"Contains mode: {(HostGuardConfig.ContainsMode.Value ? "ON" : "OFF")}\n" +
            $"Bot protection: ON ({(HostGuardConfig.BanKnownBots.Value ? "ban" : "kick")})\n" +
            $"Flood protection: {(HostGuardConfig.FloodProtectionEnabled.Value ? "ON" : "OFF")}\n" +
            $"Anti-cheat: {(HostGuardConfig.AntiCheatEnabled.Value ? "ON" : "OFF")} ({(HostGuardConfig.BanOnInvalidRpc.Value ? "ban" : "kick")})\n" +
            $"Cosmetic detection: {(HostGuardConfig.CosmeticDetectionEnabled.Value ? "ON" : "OFF")}\n" +
            $"Auto-lock on flood: {(HostGuardConfig.AutoLockOnFlood.Value ? "ON" : "OFF")} ({HostGuardConfig.AutoLockDurationSeconds.Value}s)\n" +
            $"Lobby locked: {(LobbyLock.IsLocked ? "YES" : "no")}\n" +
            $"Join notifications: {(HostGuardConfig.VerboseJoinNotifications.Value ? "ON" : "OFF")}\n" +
            $"Auto-start: {(HostGuardConfig.AutoStartEnabled.Value ? "ON" : "OFF")} ({autoTarget})\n" +
            $"Ban list: {(!string.IsNullOrEmpty(HostGuardConfig.BanListUrl.Value) ? "set" : "not set")}\n" +
            $"Whitelisted: {HostGuardConfig.GetWhitelistedCodes().Count} players\n" +
            $"Blacklisted: {Blacklist.GetAll().Count} players";
        ChatHelper.SendLocalMessage(status);
    }

    // --- Help ---

    static void ShowHelp()
    {
        string help = "[HostGuard Commands]\n" +
            "!kick/!k <name|code> - Kick a player\n" +
            "!ban/!b <name|code> - Ban a player\n" +
            "!kickall/!ka - Kick all players\n" +
            "!info/!i <name|code> - Player info\n" +
            "!status/!s - Show settings\n" +
            "!rules/!r - Show rules\n" +
            "!setrules/!sr <msg> - Set rules\n" +
            "!autostart/!as on/off/<n> - Auto-start\n" +
            "!defaultnames/!dn on/off/ban/kick\n" +
            "!badnames/!bn on/off\n" +
            "!badchat/!bc on/off\n" +
            "!contains/!cm on/off\n" +
            "!botnames/!bot on/off - Bot ban/kick\n" +
            "!flood/!fp on/off - Flood protection\n" +
            "!anticheat/!ac on/off/ban/kick\n" +
            "!cosmetic/!cos on/off - Cosmetic detect\n" +
            "!lock/!lk - Lock lobby (private)\n" +
            "!unlock/!ulk - Unlock lobby (public)\n" +
            "!autolock/!al on/off - Auto-lock floods\n" +
            "!notify/!n on/off - Join notifications\n" +
            "!whitelist/!wl [name|code] - View/add\n" +
            "!unwhitelist/!uwl <name|code>\n" +
            "!blacklist/!bl [name|code] - View/add\n" +
            "!unblacklist/!ubl <name|code>\n" +
            "!addword/!aw <word> - Add banned word\n" +
            "!removeword/!rw <word> - Remove word\n" +
            "!words/!wds - List banned words\n" +
            "!addname/!an <word> - Add bad name word\n" +
            "!removename/!rn <word> - Remove name\n" +
            "!namelist/!nl - List bad name words";
        ChatHelper.SendLocalMessage(help);
    }
}

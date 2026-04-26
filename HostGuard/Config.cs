using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

public static class HostGuardConfig
{
    // Section master toggles
    public static ConfigEntry<bool> NameFilterEnabled = null!;
    public static ConfigEntry<bool> ChatFilterEnabled = null!;
    public static ConfigEntry<bool> BotProtectionEnabled = null!;

    // Chat filter
    public static ConfigEntry<string> BannedWords = null!;
    public static ConfigEntry<bool> ContainsMode = null!;
    public static ConfigEntry<bool> BanForBannedWords = null!;

    // Name filter
    public static ConfigEntry<string> BadNameWords = null!;
    public static ConfigEntry<bool> BanForBadName = null!;
    public static ConfigEntry<bool> KickDefaultNames = null!;
    public static ConfigEntry<bool> BanForDefaultName = null!;
    public static ConfigEntry<bool> StrictDefaultNameCasing = null!;

    // Autostart
    public static ConfigEntry<bool> AutoStartEnabled = null!;
    public static ConfigEntry<int> AutoStartPlayerCount = null!;

    // Ban list
    public static ConfigEntry<string> BanListUrl = null!;

    // Rules
    public static ConfigEntry<bool> SendRulesOnLobbyStart = null!;
    public static ConfigEntry<string> RulesMessage = null!;

    // Whitelist
    public static ConfigEntry<string> WhitelistedCodes = null!;

    // Bot protection
    public static ConfigEntry<string> KnownBotNames = null!;
    public static ConfigEntry<bool> BanKnownBots = null!;
    public static ConfigEntry<string> KnownBotUrls = null!;

    // Flood protection
    public static ConfigEntry<bool> FloodProtectionEnabled = null!;
    public static ConfigEntry<int> FloodJoinThreshold = null!;
    public static ConfigEntry<int> FloodJoinWindowSeconds = null!;
    public static ConfigEntry<int> RapidLeaveThreshold = null!;
    public static ConfigEntry<bool> MeetingSpamKick = null!;
    public static ConfigEntry<int> MeetingSpamThreshold = null!;
    public static ConfigEntry<int> MeetingSpamWindowSeconds = null!;

    // Anti-cheat
    public static ConfigEntry<bool> AntiCheatEnabled = null!;
    public static ConfigEntry<bool> BanOnInvalidRpc = null!;
    public static ConfigEntry<int> ChatRateLimit = null!;
    public static ConfigEntry<int> ChatRateLimitWindowSeconds = null!;

    // Cosmetic detection
    public static ConfigEntry<bool> CosmeticDetectionEnabled = null!;
    public static ConfigEntry<string> SuspiciousColorIds = null!;
    public static ConfigEntry<bool> BanForSuspiciousCosmetics = null!;

    // Lobby lock
    public static ConfigEntry<bool> AutoLockOnFlood = null!;
    public static ConfigEntry<int> AutoLockDurationSeconds = null!;

    // Join notifications
    public static ConfigEntry<bool> VerboseJoinNotifications = null!;

    // Minimum level
    public static ConfigEntry<bool> MinLevelEnabled = null!;
    public static ConfigEntry<int> MinLevel = null!;
    public static ConfigEntry<bool> BanForLowLevel = null!;

    private static string _bannedWordsRaw = "";
    private static List<string> _bannedWordsCache = new();
    private static string _badNameWordsRaw = "";
    private static List<string> _badNameWordsCache = new();
    private static string _whitelistRaw = "";
    private static HashSet<string> _whitelistCache = new(StringComparer.OrdinalIgnoreCase);
    private static string _knownBotNamesRaw = "";
    private static List<string> _knownBotNamesCache = new();
    private static string _knownBotUrlsRaw = "";
    private static List<string> _knownBotUrlsCache = new();
    private static string _suspiciousColorsRaw = "";
    private static HashSet<int> _suspiciousColorsCache = new();

    public static void Initialize(ConfigFile config)
    {
        // Section master toggles
        NameFilterEnabled = config.Bind(
            "NameFilter", "Enabled", true,
            "Master toggle for name filtering (bad names, default names). Does not change individual settings."
        );
        ChatFilterEnabled = config.Bind(
            "ChatFilter", "Enabled", true,
            "Master toggle for chat word filtering. Does not change individual settings."
        );
        BotProtectionEnabled = config.Bind(
            "BotProtection", "Enabled", true,
            "Master toggle for bot protection (known bot names, cosmetic detection). Does not change individual settings."
        );

        // Chat filter
        BannedWords = config.Bind(
            "ChatFilter", "BannedWords", "start",
            "Words that get a player kicked/banned. Comma-separated, case-insensitive."
        );
        ContainsMode = config.Bind(
            "ChatFilter", "ContainsMode", false,
            "If true, triggers if message CONTAINS a banned word. If false, only exact matches."
        );
        BanForBannedWords = config.Bind(
            "ChatFilter", "BanInsteadOfKick", false,
            "If true, players saying banned words get banned (can't rejoin). If false, just kicked."
        );

        // Name filter
        BadNameWords = config.Bind(
            "NameFilter", "BadNameWords", "hitler,nigger,nigga,fucker,faggot,retard,kys,chink,spic,wetback,tranny,kike,gook,coon,nonce,pedo,rapist,rape,nazi,kkk,whore,cunt,slut",
            "If a player's name contains any of these words they get kicked on join. Comma-separated, case-insensitive."
        );
        BanForBadName = config.Bind(
            "NameFilter", "BanForBadName", false,
            "If true, players with bad names get banned (can't rejoin). If false, just kicked."
        );
        KickDefaultNames = config.Bind(
            "NameFilter", "KickDefaultNames", true,
            "If true, players with randomly generated default names (e.g. Funnybone) get kicked on join."
        );
        BanForDefaultName = config.Bind(
            "NameFilter", "BanForDefaultName", false,
            "If true, players with default names get banned (can't rejoin). If false, just kicked."
        );
        StrictDefaultNameCasing = config.Bind(
            "NameFilter", "StrictDefaultNameCasing", true,
            "If true, only matches exact default name casing (e.g. Funnybone). If false, matches any casing (e.g. FUNNYBONE, funnybone)."
        );

        // Autostart
        AutoStartEnabled = config.Bind(
            "AutoStart", "Enabled", false,
            "If true, the game will auto-start when the lobby reaches the target player count."
        );
        AutoStartPlayerCount = config.Bind(
            "AutoStart", "PlayerCount", 0,
            "Number of players needed to auto-start. 0 = use the lobby's max player setting."
        );

        // Ban list
        BanListUrl = config.Bind(
            "BanList", "BanListUrl", "",
            "Google Sheets CSV export URL. Leave empty to disable."
        );

        // Rules
        SendRulesOnLobbyStart = config.Bind(
            "Rules", "SendRulesOnLobbyStart", true,
            "If true, shows a rules reminder in your local chat when the lobby opens."
        );
        RulesMessage = config.Bind(
            "Rules", "RulesMessage", "HostGuard active. Typing 'start' or similar words will get you banned.",
            "The rules message shown to you when the lobby opens."
        );

        // Whitelist
        WhitelistedCodes = config.Bind(
            "Whitelist", "WhitelistedCodes", "",
            "Friend codes of players immune to all checks. Managed via !allow and !remove commands. Comma-separated."
        );

        // Bot protection
        KnownBotNames = config.Bind(
            "BotProtection", "KnownBotNames", "TNT,auser,Haunt Bot",
            "Known bot names to auto-kick/ban on join. Comma-separated, case-insensitive exact match."
        );
        BanKnownBots = config.Bind(
            "BotProtection", "BanKnownBots", true,
            "If true, known bots get banned (can't rejoin). If false, just kicked."
        );
        KnownBotUrls = config.Bind(
            "BotProtection", "KnownBotUrls", "tntaddict,ntadd,matchducking",
            "Keywords to detect bot URLs in chat. Bots use commas instead of dots (e.g. tntaddict,net). Comma-separated."
        );

        // Flood protection
        FloodProtectionEnabled = config.Bind(
            "FloodProtection", "Enabled", true,
            "If true, enables flood protection (rapid join detection, rapid leave detection)."
        );
        FloodJoinThreshold = config.Bind(
            "FloodProtection", "FloodJoinThreshold", 5,
            "Number of joins within the time window to trigger flood protection."
        );
        FloodJoinWindowSeconds = config.Bind(
            "FloodProtection", "FloodJoinWindowSeconds", 3,
            "Time window in seconds for join flood detection."
        );
        RapidLeaveThreshold = config.Bind(
            "FloodProtection", "RapidLeaveThreshold", 3,
            "Number of rapid join-and-leave events within 10 seconds to trigger lockdown."
        );
        MeetingSpamKick = config.Bind(
            "FloodProtection", "MeetingSpamKick", true,
            "If true, players who spam meetings get kicked."
        );
        MeetingSpamThreshold = config.Bind(
            "FloodProtection", "MeetingSpamThreshold", 2,
            "Number of meetings within the time window to trigger meeting spam kick."
        );
        MeetingSpamWindowSeconds = config.Bind(
            "FloodProtection", "MeetingSpamWindowSeconds", 15,
            "Time window in seconds for meeting spam detection."
        );

        // Anti-cheat
        AntiCheatEnabled = config.Bind(
            "AntiCheat", "Enabled", true,
            "If true, validates RPCs and blocks unauthorized game actions (kill exploits, vent abuse, etc.)."
        );
        BanOnInvalidRpc = config.Bind(
            "AntiCheat", "BanOnInvalidRpc", true,
            "If true, players sending invalid RPCs get banned. If false, just kicked."
        );
        ChatRateLimit = config.Bind(
            "AntiCheat", "ChatRateLimit", 5,
            "Maximum chat messages allowed per rate limit window."
        );
        ChatRateLimitWindowSeconds = config.Bind(
            "AntiCheat", "ChatRateLimitWindowSeconds", 3,
            "Time window in seconds for chat rate limiting."
        );

        // Cosmetic detection
        CosmeticDetectionEnabled = config.Bind(
            "BotProtection", "CosmeticDetectionEnabled", false,
            "If true, players matching known bot cosmetic profiles (e.g. Red color) get kicked. Disabled by default to avoid false positives."
        );
        SuspiciousColorIds = config.Bind(
            "BotProtection", "SuspiciousColorIds", "0",
            "Color IDs that trigger cosmetic detection when combined with a known bot name. 0=Red. Comma-separated."
        );
        BanForSuspiciousCosmetics = config.Bind(
            "BotProtection", "BanForSuspiciousCosmetics", false,
            "If true, players matching suspicious cosmetic profiles get banned. If false, just kicked."
        );

        // Lobby lock
        AutoLockOnFlood = config.Bind(
            "FloodProtection", "AutoLockOnFlood", true,
            "If true, automatically sets the lobby to private when a flood attack is detected."
        );
        AutoLockDurationSeconds = config.Bind(
            "FloodProtection", "AutoLockDurationSeconds", 30,
            "How long (in seconds) the lobby stays locked after an auto-lock. 0 = stay locked until manual !unlock."
        );

        // Join notifications
        VerboseJoinNotifications = config.Bind(
            "Notifications", "VerboseJoinNotifications", true,
            "If true, shows detailed player info in host chat when a player joins (level, color, friend code, etc.)."
        );

        // Minimum level
        MinLevelEnabled = config.Bind(
            "MinLevel", "Enabled", false,
            "If true, players below the minimum level get kicked/banned on join."
        );
        MinLevel = config.Bind(
            "MinLevel", "MinLevel", 5,
            "Minimum player level required to join. Players below this get kicked/banned."
        );
        BanForLowLevel = config.Bind(
            "MinLevel", "BanForLowLevel", false,
            "If true, low-level players get banned. If false, just kicked."
        );

    }

    public static List<string> GetBannedWordsList()
    {
        string raw = BannedWords.Value;
        if (raw != _bannedWordsRaw)
        {
            _bannedWordsRaw = raw;
            _bannedWordsCache = raw.Split(',').Select(w => w.Trim().ToLower()).Where(w => w.Length > 0).ToList();
        }
        return _bannedWordsCache;
    }

    public static HashSet<string> GetWhitelistedCodes()
    {
        string raw = WhitelistedCodes.Value;
        if (raw != _whitelistRaw)
        {
            _whitelistRaw = raw;
            _whitelistCache = new HashSet<string>(
                raw.Split(',').Select(w => w.Trim()).Where(w => w.Length > 0),
                StringComparer.OrdinalIgnoreCase
            );
        }
        return _whitelistCache;
    }

    public static void AddToWhitelist(string friendCode)
    {
        var codes = GetWhitelistedCodes();
        if (!codes.Contains(friendCode))
        {
            codes.Add(friendCode);
            WhitelistedCodes.Value = string.Join(",", codes);
        }
    }

    public static bool RemoveFromWhitelist(string friendCode)
    {
        var codes = GetWhitelistedCodes();
        if (!codes.Remove(friendCode))
            return false;
        WhitelistedCodes.Value = string.Join(",", codes);
        return true;
    }

    public static List<string> GetBadNameWordsList()
    {
        string raw = BadNameWords.Value;
        if (raw != _badNameWordsRaw)
        {
            _badNameWordsRaw = raw;
            _badNameWordsCache = raw.Split(',').Select(w => w.Trim().ToLower()).Where(w => w.Length > 0).ToList();
        }
        return _badNameWordsCache;
    }

    public static List<string> GetKnownBotNamesList()
    {
        string raw = KnownBotNames.Value;
        if (raw != _knownBotNamesRaw)
        {
            _knownBotNamesRaw = raw;
            _knownBotNamesCache = raw.Split(',').Select(w => w.Trim()).Where(w => w.Length > 0).ToList();
        }
        return _knownBotNamesCache;
    }

    public static List<string> GetKnownBotUrlsList()
    {
        string raw = KnownBotUrls.Value;
        if (raw != _knownBotUrlsRaw)
        {
            _knownBotUrlsRaw = raw;
            _knownBotUrlsCache = raw.Split(',').Select(w => w.Trim().ToLower()).Where(w => w.Length > 0).ToList();
        }
        return _knownBotUrlsCache;
    }

    public static List<(string Category, string Label, object Entry, System.Type ValueType)> GetAllEntries()
    {
        return new List<(string, string, object, System.Type)>
        {
            // Name Filter
            ("Name Filter", "Kick Default Names", KickDefaultNames, typeof(bool)),
            ("Name Filter", "Ban Default Names", BanForDefaultName, typeof(bool)),
            ("Name Filter", "Strict Casing", StrictDefaultNameCasing, typeof(bool)),
            ("Name Filter", "Ban Bad Names", BanForBadName, typeof(bool)),
            ("Name Filter", "Bad Name Words", BadNameWords, typeof(string)),

            // Chat Filter
            ("Chat Filter", "Ban for Banned Words", BanForBannedWords, typeof(bool)),
            ("Chat Filter", "Contains Mode", ContainsMode, typeof(bool)),
            ("Chat Filter", "Banned Words", BannedWords, typeof(string)),

            // Bot Protection
            ("Bot Protection", "Ban Known Bots", BanKnownBots, typeof(bool)),
            ("Bot Protection", "Cosmetic Detection", CosmeticDetectionEnabled, typeof(bool)),
            ("Bot Protection", "Ban Suspicious Cosmetics", BanForSuspiciousCosmetics, typeof(bool)),
            ("Bot Protection", "Known Bot Names", KnownBotNames, typeof(string)),
            ("Bot Protection", "Known Bot URLs", KnownBotUrls, typeof(string)),
            ("Bot Protection", "Suspicious Color IDs", SuspiciousColorIds, typeof(string)),

            // Flood Protection
            ("Flood Protection", "Enabled", FloodProtectionEnabled, typeof(bool)),
            ("Flood Protection", "Meeting Spam Kick", MeetingSpamKick, typeof(bool)),
            ("Flood Protection", "Auto-Lock on Flood", AutoLockOnFlood, typeof(bool)),
            ("Flood Protection", "Join Threshold", FloodJoinThreshold, typeof(int)),
            ("Flood Protection", "Join Window (sec)", FloodJoinWindowSeconds, typeof(int)),
            ("Flood Protection", "Rapid Leave Threshold", RapidLeaveThreshold, typeof(int)),
            ("Flood Protection", "Meeting Spam Threshold", MeetingSpamThreshold, typeof(int)),
            ("Flood Protection", "Meeting Window (sec)", MeetingSpamWindowSeconds, typeof(int)),
            ("Flood Protection", "Auto-Lock Duration (sec)", AutoLockDurationSeconds, typeof(int)),

            // Anti-Cheat
            ("Anti-Cheat", "Enabled", AntiCheatEnabled, typeof(bool)),
            ("Anti-Cheat", "Ban on Invalid RPC", BanOnInvalidRpc, typeof(bool)),
            ("Anti-Cheat", "Chat Rate Limit", ChatRateLimit, typeof(int)),
            ("Anti-Cheat", "Rate Limit Window (sec)", ChatRateLimitWindowSeconds, typeof(int)),

            // General
            ("General", "Auto-Start", AutoStartEnabled, typeof(bool)),
            ("General", "Auto-Start Players", AutoStartPlayerCount, typeof(int)),
            ("General", "Send Rules on Start", SendRulesOnLobbyStart, typeof(bool)),
            ("General", "Join Notifications", VerboseJoinNotifications, typeof(bool)),
            ("General", "Rules Message", RulesMessage, typeof(string)),
            ("General", "Ban List URL", BanListUrl, typeof(string)),
        };
    }

    public static HashSet<int> GetSuspiciousColorIds()
    {
        string raw = SuspiciousColorIds.Value;
        if (raw != _suspiciousColorsRaw)
        {
            _suspiciousColorsRaw = raw;
            _suspiciousColorsCache = new HashSet<int>(
                raw.Split(',')
                   .Select(w => w.Trim())
                   .Where(w => int.TryParse(w, out _))
                   .Select(w => int.Parse(w))
            );
        }
        return _suspiciousColorsCache;
    }
}

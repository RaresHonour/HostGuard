using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

public static class HostGuardConfig
{
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

    // Ban list
    public static ConfigEntry<string> BanListUrl = null!;

    // Rules
    public static ConfigEntry<bool> SendRulesOnLobbyStart = null!;
    public static ConfigEntry<string> RulesMessage = null!;

    // Whitelist
    public static ConfigEntry<string> WhitelistedCodes = null!;

    private static string _bannedWordsRaw = "";
    private static List<string> _bannedWordsCache = new();
    private static string _badNameWordsRaw = "";
    private static List<string> _badNameWordsCache = new();
    private static string _whitelistRaw = "";
    private static HashSet<string> _whitelistCache = new(StringComparer.OrdinalIgnoreCase);

    public static void Initialize(ConfigFile config)
    {
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

    public static void RemoveFromWhitelist(string friendCode)
    {
        var codes = GetWhitelistedCodes();
        if (codes.Remove(friendCode))
            WhitelistedCodes.Value = string.Join(",", codes);
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
}

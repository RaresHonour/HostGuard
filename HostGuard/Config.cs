using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

public static class HostGuardConfig
{
    public static ConfigEntry<string> BannedWords = null!;
    public static ConfigEntry<bool> AnnounceKick = null!;
    public static ConfigEntry<bool> BanInsteadOfKick = null!;
    public static ConfigEntry<bool> SendRulesOnLobbyStart = null!;
    public static ConfigEntry<string> RulesMessage = null!;
    public static ConfigEntry<bool> ContainsMode = null!;
    public static ConfigEntry<string> WhitelistedCodes = null!;
    public static ConfigEntry<string> BadNameWords = null!;
    public static ConfigEntry<string> BanListUrl = null!;

    public static void Initialize(ConfigFile config)
    {
        BannedWords = config.Bind(
            "AutoBan", "BannedWords", "start",
            "Words that get a player kicked/banned. Comma-separated, case-insensitive."
        );
        AnnounceKick = config.Bind(
            "AutoBan", "AnnounceKick", true,
            "If true, logs a message when someone gets kicked/banned."
        );
        BanInsteadOfKick = config.Bind(
            "AutoBan", "BanInsteadOfKick", false,
            "If true: banned (can't rejoin). If false: just kicked (can rejoin)."
        );
        ContainsMode = config.Bind(
            "AutoBan", "ContainsMode", false,
            "If true, triggers if message CONTAINS a banned word. If false, only exact matches."
        );
        SendRulesOnLobbyStart = config.Bind(
            "Rules", "SendRulesOnLobbyStart", true,
            "If true, shows a rules reminder in your local chat when the lobby opens."
        );
        RulesMessage = config.Bind(
            "Rules", "RulesMessage", "HostGuard active. Typing 'start' or similar words will get you banned.",
            "The rules message shown to you when the lobby opens."
        );
        WhitelistedCodes = config.Bind(
            "Whitelist", "WhitelistedCodes", "",
            "Friend codes of players immune to all checks. Managed via !allow and !remove commands. Comma-separated."
        );
        BadNameWords = config.Bind(
            "NameFilter", "BadNameWords", "hitler,nigger,nigga,fucker,faggot,retard,kys,chink,spic,wetback,tranny,kike,gook,coon,nonce,pedo,rapist,rape,nazi,kkk,whore,cunt,slut",
            "If a player's name contains any of these words they get kicked on join. Comma-separated, case-insensitive."
        );
        BanListUrl = config.Bind(
            "BanList", "BanListUrl", "",
            "Google Sheets CSV export URL. Leave empty to disable."
        );
    }

    public static List<string> GetBannedWordsList() =>
        BannedWords.Value.Split(',').Select(w => w.Trim().ToLower()).Where(w => w.Length > 0).ToList();

    public static List<string> GetWhitelistedCodes() =>
        WhitelistedCodes.Value.Split(',').Select(w => w.Trim()).Where(w => w.Length > 0).ToList();

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
        codes.Remove(friendCode);
        WhitelistedCodes.Value = string.Join(",", codes);
    }

    public static List<string> GetBadNameWordsList() =>
        BadNameWords.Value.Split(',').Select(w => w.Trim().ToLower()).Where(w => w.Length > 0).ToList();
}
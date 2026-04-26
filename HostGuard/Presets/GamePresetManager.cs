using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using AmongUs.GameOptions;

public static class GamePresetManager
{
    private static readonly string PresetsDir =
        Path.Combine(BepInEx.Paths.ConfigPath, "HostGuard_GamePresets");

    private static void EnsureDirectory()
    {
        if (!Directory.Exists(PresetsDir))
            Directory.CreateDirectory(PresetsDir);
    }

    public static List<string> GetPresetNames()
    {
        EnsureDirectory();
        return Directory.GetFiles(PresetsDir, "*.txt")
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .OrderBy(n => n)
            .ToList();
    }

    public static bool SavePreset(string name)
    {
        EnsureDirectory();
        var opts = GameOptionsManager.Instance?.currentNormalGameOptions;
        if (opts == null)
        {
            HostGuardPlugin.Logger.LogWarning("[HostGuard] Cannot save game preset — game options not available.");
            return false;
        }

        var lines = new List<string>();
        lines.Add($"# Game Preset: {name}");
        lines.Add($"# Saved: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        lines.Add("");

        try { lines.Add($"PlayerSpeedMod={opts.PlayerSpeedMod.ToString(CultureInfo.InvariantCulture)}"); } catch { }
        try { lines.Add($"KillCooldown={opts.KillCooldown.ToString(CultureInfo.InvariantCulture)}"); } catch { }
        try { lines.Add($"CrewLightMod={opts.CrewLightMod.ToString(CultureInfo.InvariantCulture)}"); } catch { }
        try { lines.Add($"ImpostorLightMod={opts.ImpostorLightMod.ToString(CultureInfo.InvariantCulture)}"); } catch { }
        try { lines.Add($"NumImpostors={opts.NumImpostors}"); } catch { }
        try { lines.Add($"KillDistance={opts.KillDistance}"); } catch { }
        try { lines.Add($"NumEmergencyMeetings={opts.NumEmergencyMeetings}"); } catch { }
        try { lines.Add($"EmergencyCooldown={opts.EmergencyCooldown}"); } catch { }
        try { lines.Add($"DiscussionTime={opts.DiscussionTime}"); } catch { }
        try { lines.Add($"VotingTime={opts.VotingTime}"); } catch { }
        try { lines.Add($"NumCommonTasks={opts.NumCommonTasks}"); } catch { }
        try { lines.Add($"NumShortTasks={opts.NumShortTasks}"); } catch { }
        try { lines.Add($"NumLongTasks={opts.NumLongTasks}"); } catch { }
        try { lines.Add($"ConfirmImpostor={opts.ConfirmImpostor}"); } catch { }
        try { lines.Add($"VisualTasks={opts.VisualTasks}"); } catch { }
        try { lines.Add($"AnonymousVotes={opts.AnonymousVotes}"); } catch { }
        try { lines.Add($"TaskBarMode={(int)opts.TaskBarMode}"); } catch { }
        try { lines.Add($"MapId={opts.MapId}"); } catch { }

        string path = Path.Combine(PresetsDir, SanitizeFileName(name) + ".txt");
        File.WriteAllLines(path, lines);
        HostGuardPlugin.Logger.LogInfo($"[HostGuard] Game preset saved: {name}");
        return true;
    }

    public static bool LoadPreset(string name)
    {
        string path = Path.Combine(PresetsDir, SanitizeFileName(name) + ".txt");
        if (!File.Exists(path)) return false;

        var opts = GameOptionsManager.Instance?.currentNormalGameOptions;
        if (opts == null)
        {
            HostGuardPlugin.Logger.LogWarning("[HostGuard] Cannot load game preset — game options not available.");
            return false;
        }

        try
        {
            var values = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                int eq = line.IndexOf('=');
                if (eq < 0) continue;
                values[line.Substring(0, eq).Trim()] = line.Substring(eq + 1).Trim();
            }

            string v;
            if (values.TryGetValue("PlayerSpeedMod", out v)) try { opts.PlayerSpeedMod = float.Parse(v, CultureInfo.InvariantCulture); } catch { }
            if (values.TryGetValue("KillCooldown", out v)) try { opts.KillCooldown = float.Parse(v, CultureInfo.InvariantCulture); } catch { }
            if (values.TryGetValue("CrewLightMod", out v)) try { opts.CrewLightMod = float.Parse(v, CultureInfo.InvariantCulture); } catch { }
            if (values.TryGetValue("ImpostorLightMod", out v)) try { opts.ImpostorLightMod = float.Parse(v, CultureInfo.InvariantCulture); } catch { }
            if (values.TryGetValue("NumImpostors", out v)) try { opts.NumImpostors = int.Parse(v); } catch { }
            if (values.TryGetValue("KillDistance", out v)) try { opts.KillDistance = int.Parse(v); } catch { }
            if (values.TryGetValue("NumEmergencyMeetings", out v)) try { opts.NumEmergencyMeetings = int.Parse(v); } catch { }
            if (values.TryGetValue("EmergencyCooldown", out v)) try { opts.EmergencyCooldown = int.Parse(v); } catch { }
            if (values.TryGetValue("DiscussionTime", out v)) try { opts.DiscussionTime = int.Parse(v); } catch { }
            if (values.TryGetValue("VotingTime", out v)) try { opts.VotingTime = int.Parse(v); } catch { }
            if (values.TryGetValue("NumCommonTasks", out v)) try { opts.NumCommonTasks = int.Parse(v); } catch { }
            if (values.TryGetValue("NumShortTasks", out v)) try { opts.NumShortTasks = int.Parse(v); } catch { }
            if (values.TryGetValue("NumLongTasks", out v)) try { opts.NumLongTasks = int.Parse(v); } catch { }
            if (values.TryGetValue("ConfirmImpostor", out v)) try { opts.ConfirmImpostor = bool.Parse(v); } catch { }
            if (values.TryGetValue("VisualTasks", out v)) try { opts.VisualTasks = bool.Parse(v); } catch { }
            if (values.TryGetValue("AnonymousVotes", out v)) try { opts.AnonymousVotes = bool.Parse(v); } catch { }
            if (values.TryGetValue("TaskBarMode", out v)) try { opts.TaskBarMode = (AmongUs.GameOptions.TaskBarMode)int.Parse(v); } catch { }
            if (values.TryGetValue("MapId", out v)) try { opts.MapId = byte.Parse(v); } catch { }

            HostGuardPlugin.Logger.LogInfo($"[HostGuard] Game preset loaded: {name}");
            return true;
        }
        catch (Exception ex)
        {
            HostGuardPlugin.Logger.LogError($"[HostGuard] Error loading game preset {name}: {ex.Message}");
            return false;
        }
    }

    public static bool DeletePreset(string name)
    {
        string path = Path.Combine(PresetsDir, SanitizeFileName(name) + ".txt");
        if (!File.Exists(path)) return false;
        File.Delete(path);
        HostGuardPlugin.Logger.LogInfo($"[HostGuard] Game preset deleted: {name}");
        return true;
    }

    private static string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}

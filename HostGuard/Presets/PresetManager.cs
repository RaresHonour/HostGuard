using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;

public static class PresetManager
{
    private static readonly string PresetsDir =
        Path.Combine(BepInEx.Paths.ConfigPath, "HostGuard_Presets");

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

    public static void SavePreset(string name)
    {
        EnsureDirectory();
        var lines = new List<string>();
        lines.Add($"# HostGuard Preset: {name}");
        lines.Add($"# Saved: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        lines.Add("");

        foreach (var field in typeof(HostGuardConfig).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (!field.FieldType.IsGenericType) continue;
            if (field.FieldType.GetGenericTypeDefinition() != typeof(ConfigEntry<>)) continue;

            var entry = field.GetValue(null);
            if (entry == null) continue;

            var valueProp = field.FieldType.GetProperty("Value");
            var boxedValue = valueProp?.GetValue(entry);
            lines.Add($"{field.Name}={boxedValue}");
        }

        string path = Path.Combine(PresetsDir, SanitizeFileName(name) + ".txt");
        File.WriteAllLines(path, lines);

        HostGuardPlugin.Logger.LogInfo($"[HostGuard] Preset saved: {name}");
    }

    public static bool LoadPreset(string name)
    {
        string path = Path.Combine(PresetsDir, SanitizeFileName(name) + ".txt");
        if (!File.Exists(path)) return false;

        try
        {
            var values = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                int eq = line.IndexOf('=');
                if (eq < 0) continue;
                string key = line.Substring(0, eq).Trim();
                string val = line.Substring(eq + 1).Trim();
                values[key] = val;
            }

            // Fields to skip when loading presets (managed separately)
            var skipFields = new HashSet<string> { "WhitelistedCodes" };

            foreach (var field in typeof(HostGuardConfig).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (!field.FieldType.IsGenericType) continue;
                if (field.FieldType.GetGenericTypeDefinition() != typeof(ConfigEntry<>)) continue;
                if (!values.TryGetValue(field.Name, out var strVal)) continue;
                if (skipFields.Contains(field.Name)) continue;

                var entry = field.GetValue(null);
                if (entry == null) continue;

                var valueType = field.FieldType.GetGenericArguments()[0];
                var valueProp = field.FieldType.GetProperty("Value");

                object? parsed;
                if (valueType == typeof(bool))
                    parsed = bool.Parse(strVal);
                else if (valueType == typeof(int))
                    parsed = int.Parse(strVal);
                else
                    parsed = strVal;

                valueProp?.SetValue(entry, parsed);
                HostGuardPlugin.Logger.LogInfo($"[HostGuard] Preset set {field.Name} = {parsed}");
            }

            HostGuardPlugin.Logger.LogInfo($"[HostGuard] Preset loaded: {name}");
            return true;
        }
        catch (Exception ex)
        {
            HostGuardPlugin.Logger.LogError($"[HostGuard] Error loading preset {name}: {ex.Message}");
            return false;
        }
    }

    public static bool DeletePreset(string name)
    {
        string path = Path.Combine(PresetsDir, SanitizeFileName(name) + ".txt");
        if (!File.Exists(path)) return false;
        File.Delete(path);
        HostGuardPlugin.Logger.LogInfo($"[HostGuard] Preset deleted: {name}");
        return true;
    }

    private static string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}

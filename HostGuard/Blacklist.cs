using System;
using System.Collections.Generic;
using System.IO;

public static class Blacklist
{
    private static readonly string FilePath =
        Path.Combine(BepInEx.Paths.ConfigPath, "hostguard_blacklist.txt");

    private static readonly HashSet<string> Codes =
        new(StringComparer.OrdinalIgnoreCase);

    public static void Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                File.Create(FilePath).Dispose();
                return;
            }

            var lines = File.ReadAllLines(FilePath);
            Codes.Clear();
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.Length > 0)
                    Codes.Add(trimmed);
            }

            HostGuardPlugin.Logger.LogInfo($"[Blacklist] Loaded {Codes.Count} friend code(s).");
        }
        catch (Exception ex)
        {
            HostGuardPlugin.Logger.LogError($"[Blacklist] Failed to load blacklist: {ex.Message}");
        }
    }

    public static void Save()
    {
        try
        {
            File.WriteAllLines(FilePath, Codes);
        }
        catch (Exception ex)
        {
            HostGuardPlugin.Logger.LogError($"[Blacklist] Failed to save blacklist: {ex.Message}");
        }
    }

    public static bool Add(string friendCode)
    {
        if (!Codes.Add(friendCode))
            return false;

        Save();
        return true;
    }

    public static bool Remove(string friendCode)
    {
        if (!Codes.Remove(friendCode))
            return false;

        Save();
        return true;
    }

    public static bool Contains(string friendCode)
    {
        return Codes.Contains(friendCode);
    }

    public static IReadOnlyCollection<string> GetAll()
    {
        return Codes;
    }
}

using System;
using System.IO;

public static class CrashLog
{
    private static readonly string LogPath = Path.Combine(
        BepInEx.Paths.ConfigPath, "hostguard_crash.log");
    private static readonly object _lock = new();

    public static void Write(string message)
    {
        try
        {
            lock (_lock)
            {
                using var writer = new StreamWriter(LogPath, true);
                writer.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
                writer.Flush();
            }
        }
        catch { }
    }

    public static void Clear()
    {
        try { File.Delete(LogPath); } catch { }
    }
}

public static class DiagHelper
{
    public static bool ShouldLog()
    {
        try
        {
            return AmongUsClient.Instance != null
                && AmongUsClient.Instance.AmHost
                && ShipStatus.Instance != null;
        }
        catch { return false; }
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

public static class BanListManager
{
    private static readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    private static HashSet<string> _cachedCodes = new HashSet<string>();
    private static string _cachedUrl = "";
    private static DateTime _lastFetch = DateTime.MinValue;
    private static bool _hasFetched;
    private static readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(5);

    public static async Task<HashSet<string>> FetchBannedCodesAsync()
    {
        string url = HostGuardConfig.BanListUrl.Value.Trim();
        if (string.IsNullOrEmpty(url))
            return new HashSet<string>();

        bool urlChanged = url != _cachedUrl;
        if (_hasFetched && !urlChanged && DateTime.UtcNow - _lastFetch < _cacheTtl)
            return _cachedCodes;

        try
        {
            string csv = await _client.GetStringAsync(url);
            var codes = ParseBannedCodes(csv);
            _cachedCodes = codes;
            _cachedUrl = url;
            _lastFetch = DateTime.UtcNow;
            _hasFetched = true;
            HostGuardPlugin.Logger.LogInfo($"[HostGuard] Ban list fetched: {codes.Count} codes loaded.");
            return codes;
        }
        catch (Exception ex)
        {
            HostGuardPlugin.Logger.LogWarning($"[HostGuard] Failed to fetch ban list: {ex.Message}");
            return _cachedCodes;
        }
    }

    private static HashSet<string> ParseBannedCodes(string csv)
    {
        var codes = new HashSet<string>();
        string[] lines = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            string[] columns = line.Split(',');
            if (columns.Length > 1)
            {
                string code = columns[1].Trim().Trim('"');
                if (!string.IsNullOrEmpty(code))
                    codes.Add(code);
            }
        }
        return codes;
    }
}
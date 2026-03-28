using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

public static class BanListManager
{
    private static readonly HttpClient _client = new HttpClient();

    public static async Task<HashSet<string>> FetchBannedCodesAsync()
    {
        string url = HostGuardConfig.BanListUrl.Value.Trim();
        if (string.IsNullOrEmpty(url))
            return new HashSet<string>();

        try
        {
            string csv = await _client.GetStringAsync(url);
            var codes = ParseBannedCodes(csv);
            HostGuardPlugin.Logger.LogInfo($"[HostGuard] Ban list fetched: {codes.Count} codes loaded.");
            return codes;
        }
        catch (System.Exception ex)
        {
            HostGuardPlugin.Logger.LogWarning($"[HostGuard] Failed to fetch ban list: {ex.Message}");
            return new HashSet<string>();
        }
    }

    private static HashSet<string> ParseBannedCodes(string csv)
    {
        var codes = new HashSet<string>();
        string[] lines = csv.Split('\n');
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
using System;
using HarmonyLib;

/// <summary>
/// Intercepts cosmetic loading to prevent crashes from malformed cosmetic data.
/// Logs all cosmetic data to a file that survives freezes/crashes.
/// </summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetOutfit))]
public static class SetOutfitPatch
{
    public static bool Prefix(PlayerControl __instance, NetworkedPlayerInfo.PlayerOutfit newOutfit, PlayerOutfitType type)
    {
        try
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (__instance.AmOwner) return true;

            string playerName = __instance.Data?.PlayerName ?? "unknown";
            string friendCode = __instance.Data?.FriendCode ?? "";

            // VERBOSE LOG — write to file immediately (survives freeze/crash)
            if (newOutfit != null)
            {
                CrashLog.Write($"[SetOutfit] {playerName} ({friendCode}): " +
                    $"color={newOutfit.ColorId} skin='{newOutfit.SkinId}' hat='{newOutfit.HatId}' " +
                    $"visor='{newOutfit.VisorId}' pet='{newOutfit.PetId}' nameplate='{newOutfit.NamePlateId}' " +
                    $"type={type}");
            }
            else
            {
                CrashLog.Write($"[SetOutfit] {playerName} ({friendCode}): NULL OUTFIT");
            }

            if (!HostGuardConfig.AntiCheatEnabled.Value) return true;

            if (newOutfit == null)
            {
                HostGuardPlugin.Logger.LogWarning($"[AntiCheat] Blocked null outfit from {playerName}");
                return false;
            }

            // Validate color ID
            int colorId = newOutfit.ColorId;
            if (colorId < 0 || colorId > 50)
            {
                BanForBadCosmetics(__instance, $"invalid color {colorId}");
                return false;
            }

            // Validate cosmetic string IDs
            if (!ValidId(newOutfit.SkinId, "skin") ||
                !ValidId(newOutfit.HatId, "hat") ||
                !ValidId(newOutfit.VisorId, "visor") ||
                !ValidId(newOutfit.PetId, "pet") ||
                !ValidId(newOutfit.NamePlateId, "nameplate"))
            {
                BanForBadCosmetics(__instance,
                    $"malformed cosmetics: skin='{newOutfit.SkinId}' hat='{newOutfit.HatId}'");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            CrashLog.Write($"[SetOutfit] ERROR: {ex}");
            return true;
        }
    }

    private static bool ValidId(string id, string type)
    {
        if (string.IsNullOrEmpty(id)) return true;

        if (id.Length > 80)
        {
            CrashLog.Write($"[Cosmetic] {type} ID too long: {id.Length} chars: '{id}'");
            return false;
        }

        for (int i = 0; i < id.Length; i++)
        {
            char c = id[i];
            if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
            {
                CrashLog.Write($"[Cosmetic] {type} ID invalid char '{c}' in '{id}'");
                return false;
            }
        }

        return true;
    }

    private static void BanForBadCosmetics(PlayerControl player, string reason)
    {
        try
        {
            string name = player.Data?.PlayerName ?? "unknown";
            string code = player.Data?.FriendCode ?? "";
            CrashLog.Write($"[Cosmetic] BANNING {name} ({code}) — {reason}");
            HostGuardPlugin.Logger.LogWarning($"[AntiCheat] Banning {name} ({code}) — {reason}");
            ChatHelper.SendLocalMessage($"[AntiCheat] Banned {name} — {reason}");
            FloodGuard.SessionBan(code);
            var client = AmongUsClient.Instance.GetClient(player.OwnerId);
            if (client != null)
                AmongUsClient.Instance.KickPlayer(client.Id, true);
        }
        catch { }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RawSetOutfit))]
public static class RawSetOutfitPatch
{
    static bool Prefix(PlayerControl __instance, NetworkedPlayerInfo.PlayerOutfit newOutfit, PlayerOutfitType type)
    {
        CrashLog.Write($"[RawSetOutfit] called for {__instance.Data?.PlayerName ?? "unknown"}");
        return SetOutfitPatch.Prefix(__instance, newOutfit, type);
    }
}


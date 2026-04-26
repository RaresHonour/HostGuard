using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using HarmonyLib;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class RpcValidationPatch
{
    // Chat rate limiting per player
    private static readonly Dictionary<byte, Queue<DateTime>> _chatTimes = new();

    // Game-action RPCs that should never be sent in lobby
    private static readonly HashSet<byte> _gameOnlyRpcs = new()
    {
        (byte)RpcCalls.MurderPlayer,
        (byte)RpcCalls.ReportDeadBody,
        (byte)RpcCalls.StartMeeting,
        (byte)RpcCalls.EnterVent,
        (byte)RpcCalls.ExitVent,
        (byte)RpcCalls.Shapeshift,
        (byte)RpcCalls.ProtectPlayer,
    };

    // Impostor-only RPCs
    private static readonly HashSet<byte> _impostorRpcs = new()
    {
        (byte)RpcCalls.MurderPlayer,
    };

    static bool Prefix(PlayerControl __instance, byte callId, MessageReader reader)
    {
        try
        {
        if (!HostGuardConfig.AntiCheatEnabled.Value) return true;
        if (!AmongUsClient.Instance.AmHost) return true;
        if (__instance.AmOwner) return true;

        // Skip validation for whitelisted players
        string friendCode = __instance.Data?.FriendCode ?? "";
        if (!string.IsNullOrEmpty(friendCode) && HostGuardConfig.GetWhitelistedCodes().Contains(friendCode))
            return true;

        var rpc = (RpcCalls)callId;

        // Block game-action RPCs in lobby (primary anti-crash protection)
        if (!AmongUsClient.Instance.IsGameStarted && _gameOnlyRpcs.Contains(callId))
        {
            HandleViolation(__instance, rpc, "game RPC in lobby");
            return false;
        }

        // Role-based validation during game
        if (AmongUsClient.Instance.IsGameStarted)
        {
            // Kill exploit: non-impostor sending MurderPlayer
            if (_impostorRpcs.Contains(callId) && !IsImpostor(__instance))
            {
                HandleViolation(__instance, rpc, "impostor RPC from non-impostor");
                return false;
            }

            // Vent abuse: only impostors and engineers can vent
            if (callId == (byte)RpcCalls.EnterVent || callId == (byte)RpcCalls.ExitVent)
            {
                if (!CanVent(__instance))
                {
                    HandleViolation(__instance, rpc, "vent usage without vent ability");
                    return false;
                }
            }

            // Shapeshift abuse: only shapeshifters can shapeshift
            if (callId == (byte)RpcCalls.Shapeshift)
            {
                if (!IsRole(__instance, RoleTypes.Shapeshifter))
                {
                    HandleViolation(__instance, rpc, "shapeshift without shapeshifter role");
                    return false;
                }
            }

            // Meeting spam detection
            if (callId == (byte)RpcCalls.StartMeeting || callId == (byte)RpcCalls.ReportDeadBody)
            {
                if (FloodGuard.IsMeetingSpam(__instance.PlayerId))
                {
                    HandleViolation(__instance, rpc, "meeting spam");
                    return false;
                }
            }
        }

        // Chat rate limiting (applies in both lobby and game)
        if (callId == (byte)RpcCalls.SendChat || callId == (byte)RpcCalls.SendQuickChat)
        {
            if (IsChatSpam(__instance.PlayerId))
            {
                HostGuardPlugin.Logger.LogWarning($"[AntiCheat] Blocked chat spam from {__instance.Data?.PlayerName ?? "unknown"} (PlayerId: {__instance.PlayerId})");
                return false; // silently block, don't kick for chat spam
            }
        }

        return true;
        }
        catch (System.Exception ex)
        {
            HostGuardPlugin.Logger.LogError($"[AntiCheat] Error in RPC validation: {ex.Message}");
            return true; // allow on error to avoid blocking legitimate RPCs
        }
    }

    private static bool IsImpostor(PlayerControl player)
    {
        return player.Data?.Role?.IsImpostor ?? false;
    }

    private static bool CanVent(PlayerControl player)
    {
        if (player.Data?.Role == null) return false;
        if (player.Data.Role.IsImpostor) return true;
        // Engineers can vent
        if (IsRole(player, RoleTypes.Engineer)) return true;
        return false;
    }

    private static bool IsRole(PlayerControl player, RoleTypes role)
    {
        return player.Data?.RoleType == role;
    }

    private static bool IsChatSpam(byte playerId)
    {
        var now = DateTime.UtcNow;

        if (!_chatTimes.TryGetValue(playerId, out var times))
        {
            times = new Queue<DateTime>();
            _chatTimes[playerId] = times;
        }

        times.Enqueue(now);
        int windowSeconds = HostGuardConfig.ChatRateLimitWindowSeconds.Value;
        while (times.Count > 0 && (now - times.Peek()).TotalSeconds > windowSeconds)
            times.Dequeue();

        return times.Count > HostGuardConfig.ChatRateLimit.Value;
    }

    private static void HandleViolation(PlayerControl player, RpcCalls rpc, string reason)
    {
        string playerName = player.Data?.PlayerName ?? "unknown";
        HostGuardPlugin.Logger.LogWarning($"[AntiCheat] Blocked {rpc} from {playerName}: {reason}");
        ChatHelper.SendLocalMessage($"[AntiCheat] {playerName}: {reason}");

        var client = AmongUsClient.Instance.GetClient(player.OwnerId);
        if (client != null)
            AmongUsClient.Instance.KickPlayer(client.Id, HostGuardConfig.BanOnInvalidRpc.Value);
    }

    public static void Reset()
    {
        _chatTimes.Clear();
    }
}

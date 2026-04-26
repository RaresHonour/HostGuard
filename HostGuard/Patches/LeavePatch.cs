using HarmonyLib;
using InnerNet;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
public static class LeavePatch
{
    static void Postfix(ClientData data)
    {
        if (!AmongUsClient.Instance.AmHost || data == null) return;

        if (HostGuardConfig.FloodProtectionEnabled.Value)
            FloodGuard.OnPlayerLeft(data.Id);
    }
}

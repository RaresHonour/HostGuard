using HarmonyLib;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class HudStartPatch
{
    public static void Postfix(HudManager __instance)
    {
        HostGuardUI.CreateLobbyButton(__instance.transform);
        CrashLog.Clear();
        CrashLog.Write("=== New lobby session ===");
        BanListManager.RefreshInBackground();
        HostGuardPlugin.Logger.LogInfo("[HG UI] HudManager.Start — lobby button initialized.");
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HudUpdatePatch
{
    public static void Postfix()
    {
        HostGuardUI.UpdateVisibility();
    }
}

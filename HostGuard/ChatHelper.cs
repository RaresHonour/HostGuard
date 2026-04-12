public static class ChatHelper
{
    public static void SendLocalMessage(string message)
    {
        var chat = DestroyableSingleton<HudManager>.Instance?.Chat;
        if (chat == null) return;

        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null) return;

        chat.AddChat(localPlayer, message, false);
    }
}

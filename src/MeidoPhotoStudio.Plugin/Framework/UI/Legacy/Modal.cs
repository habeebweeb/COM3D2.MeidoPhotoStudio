namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public static class Modal
{
    private static BaseWindow currentModal;
    private static GUIStyle windowStyle;

    internal static bool Visible { get; private set; }

    internal static int ID =>
        Visible ? currentModal.ID : -1;

    private static GUIStyle WindowStyle =>
        windowStyle ??= new(GUI.skin.box);

    internal static void Show(BaseWindow modalWindow)
    {
        _ = modalWindow ?? throw new ArgumentNullException(nameof(modalWindow));

        Close();
        currentModal = modalWindow;
        currentModal.Visible = true;
        Visible = true;
    }

    internal static void Close()
    {
        Visible = false;
        currentModal = null;
    }

    internal static void Update()
    {
        if (UnityEngine.Input.mouseScrollDelta.y is 0f || !Visible)
            return;

        var mousePos = new Vector2(UnityEngine.Input.mousePosition.x, Screen.height - UnityEngine.Input.mousePosition.y);

        if (currentModal.WindowRect.Contains(mousePos))
            UnityEngine.Input.ResetInputAxes();
    }

    internal static void Draw()
    {
        if (!Visible)
            return;

        currentModal.WindowRect =
            GUI.Window(currentModal.ID, currentModal.WindowRect, currentModal.GUIFunc, string.Empty, WindowStyle);

        GUI.BringWindowToFront(currentModal.ID);
    }

    internal static bool MouseOverModal()
    {
        if (!Visible)
            return false;

        var mousePosition = new Vector2(UnityEngine.Input.mousePosition.x, Screen.height - UnityEngine.Input.mousePosition.y);

        return currentModal.WindowRect.Contains(mousePosition);
    }
}

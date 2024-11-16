using MeidoPhotoStudio.Plugin.Framework.UI;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class WindowManager
{
    private static GUIStyle windowStyle;

    private readonly Dictionary<Window, BaseWindow> windows = [];

    public WindowManager() =>
        ScreenSizeChecker.ScreenSizeChanged += OnScreenSizeChanged;

    public enum Window
    {
        Main,
        Message,
        Save,
        Settings,
    }

    private static GUIStyle WindowStyle =>
        windowStyle ??= new(GUI.skin.box);

    public BaseWindow this[Window id]
    {
        get => windows[id];
        set => windows[id] = value;
    }

    public bool MouseOverAnyWindow()
    {
        foreach (var window in windows.Values.Where(static window => window.Visible))
            if (MouseOverWindow(window))
                return true;

        return false;

        static bool MouseOverWindow(BaseWindow window)
        {
            var mousePosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

            return window.WindowRect.Contains(mousePosition);
        }
    }

    internal void DrawWindows()
    {
        foreach (var window in windows.Values)
        {
            if (!window.Visible)
                continue;

            window.WindowRect = GUI.Window(window.ID, window.WindowRect, window.GUIFunc, string.Empty, WindowStyle);
        }
    }

    internal void Update()
    {
        foreach (var window in windows.Values)
            window.Update();
    }

    internal void Activate()
    {
        foreach (var window in windows.Values)
            window.Activate();
    }

    internal void Deactivate()
    {
        foreach (var window in windows.Values)
            window.Deactivate();
    }

    private void OnScreenSizeChanged(object sender, EventArgs e)
    {
        foreach (var window in windows.Values)
            window.OnScreenDimensionsChanged(new(Screen.width, Screen.height));
    }
}

using MeidoPhotoStudio.Plugin.Framework.Service;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

/// <summary>Main window.</summary>
public partial class MainWindow : BaseWindow
{
    public const int MinimumWindowWidth = 255;

    private const float ResizeHandleSize = 15f;
    private const float MinimumWindowHeight = 400f;

    private readonly LazyStyle pluginInfoStyle = new(
        10,
        static () => new(GUI.skin.label)
        {
            alignment = TextAnchor.LowerLeft,
        });

    private readonly LazyStyle buttonStyle = new(13, () => new(GUI.skin.button));
    private readonly TabSelectionController tabSelectionController;
    private readonly Dictionary<Constants.Window, BaseMainWindowPane> windowPanes;
    private readonly CustomMaidSceneService customMaidSceneService;
    private readonly InputRemapper inputRemapper;
    private readonly TabsPane tabsPane;
    private readonly Button settingsButton;

    private int windowWidth = MinimumWindowWidth;
    private Rect resizeHandleRect = new(0f, 0f, ResizeHandleSize, ResizeHandleSize);
    private bool resizing;
    private BaseMainWindowPane currentWindowPane;
    private string settingsButtonLabel;
    private string closeButtonLabel;
    private Constants.Window selectedWindow;

    public MainWindow(
        TabSelectionController tabSelectionController,
        CustomMaidSceneService customMaidSceneService,
        InputRemapper inputRemapper)
    {
        this.tabSelectionController = tabSelectionController;

        this.tabSelectionController.TabSelected += (_, e) =>
            ChangeWindow(e.Tab);

        this.customMaidSceneService = customMaidSceneService;
        this.inputRemapper = inputRemapper ? inputRemapper : throw new ArgumentNullException(nameof(inputRemapper));

        windowPanes = [];
        WindowRect = new(Screen.width, Screen.height * 0.08f, WindowWidth, Screen.height * 0.9f);

        tabsPane = new TabsPane();
        tabsPane.TabChange += (_, _) =>
            ChangeTab();

        settingsButtonLabel = Translation.Get("mainWindow", "settingsButton");
        closeButtonLabel = Translation.Get("mainWindow", "closeSettingsButton");

        settingsButton = new(settingsButtonLabel);
        settingsButton.ControlEvent += (_, _) =>
        {
            if (selectedWindow is Constants.Window.Settings)
            {
                ChangeTab();
            }
            else
            {
                settingsButton.Label = closeButtonLabel;
                SetCurrentWindow(Constants.Window.Settings);
            }
        };
    }

    public override Rect WindowRect
    {
        set
        {
            value.x = Mathf.Clamp(value.x, 0, Screen.width - value.width);
            value.y = Mathf.Clamp(value.y, -value.height + 30, Screen.height - 50);

            base.WindowRect = value;
        }
    }

    public int WindowWidth
    {
        get => windowWidth;
        set
        {
            var newWidth = value;

            if (newWidth < MinimumWindowWidth)
                newWidth = MinimumWindowWidth;

            windowWidth = newWidth;

            var clampedWidth = ClampWindowWidth(Screen.width * 0.13f);

            WindowRect = WindowRect with
            {
                xMin = WindowRect.xMax - clampedWidth,
            };
        }
    }

    public BaseMainWindowPane this[Constants.Window id]
    {
        get => windowPanes[id];
        set => AddWindow(id, value);
    }

    public override void Draw()
    {
        currentWindowPane?.Draw();

        GUI.enabled = true;

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();

        GUILayout.Space(ResizeHandleSize + 3f);

        GUILayout.Label(Plugin.PluginString, pluginInfoStyle);

        GUILayout.FlexibleSpace();

        GUI.enabled = !inputRemapper.Listening;

        settingsButton.Draw(buttonStyle, GUILayout.ExpandWidth(false));

        GUI.enabled = true;

        GUILayout.EndHorizontal();

        GUI.Box(resizeHandleRect, GUIContent.none);
    }

    public override void OnScreenDimensionsChanged(Vector2 newScreenDimensions)
    {
        base.OnScreenDimensionsChanged(newScreenDimensions);

        var newWindowRect = new Rect(
            Screen.width,
            Screen.height * 0.08f,
            ClampWindowWidth(Screen.width * 0.13f),
            Screen.height * 0.9f);

        if (customMaidSceneService.EditScene)
            newWindowRect.height *= 0.85f;

        WindowRect = newWindowRect;
    }

    public override void GUIFunc(int id)
    {
        HandleResize();

        Draw();

        if (!resizing)
            GUI.DragWindow();

        void HandleResize()
        {
            resizeHandleRect = resizeHandleRect with
            {
                x = 0f,
                y = WindowRect.height - ResizeHandleSize,
            };

            if (resizing && !Input.GetMouseButton(0))
                resizing = false;
            else if (!resizing && Input.GetMouseButtonDown(0) && resizeHandleRect.Contains(Event.current.mousePosition))
                resizing = true;

            if (resizing)
            {
                var minimumWindowWidth = Mathf.Max(WindowWidth, Utility.GetPix(WindowWidth));
                var xMin = Mathf.Max(0f, Mathf.Min(WindowRect.xMax - minimumWindowWidth, Input.mousePosition.x - ResizeHandleSize / 2f));
                var height = Mathf.Max(MinimumWindowHeight, Event.current.mousePosition.y + ResizeHandleSize / 2f);

                WindowRect = WindowRect with
                {
                    xMin = Mathf.RoundToInt(xMin),
                    height = height,
                };
            }
        }
    }

    public override void Activate()
    {
        foreach (var pane in windowPanes.Values)
            pane.Activate();

        tabsPane.SelectedTab = Constants.Window.Call;
        Visible = true;

        var newWindowRect = new Rect(
            Screen.width,
            Screen.height * 0.08f,
            ClampWindowWidth(Screen.width * 0.13f),
            Screen.height * 0.9f);

        if (customMaidSceneService.EditScene)
            newWindowRect.height *= 0.85f;

        WindowRect = newWindowRect;
    }

    protected override void ReloadTranslation()
    {
        settingsButtonLabel = Translation.Get("mainWindow", "settingsButton");
        closeButtonLabel = Translation.Get("mainWindow", "closeSettingsButton");
        settingsButton.Label = selectedWindow == Constants.Window.Settings ? closeButtonLabel : settingsButtonLabel;
    }

    private void AddWindow(Constants.Window id, BaseMainWindowPane window)
    {
        if (windowPanes.ContainsKey(id))
            return;

        windowPanes[id] = window;
        windowPanes[id].SetTabsPane(tabsPane);
        windowPanes[id].SetParent(this);
    }

    private float ClampWindowWidth(float width) =>
        Mathf.Min(Screen.width - 20f, Mathf.Max(WindowWidth, Mathf.Min(Utility.GetPix(WindowWidth), width)));

    private void ChangeTab()
    {
        settingsButton.Label = settingsButtonLabel;
        SetCurrentWindow(tabsPane.SelectedTab);
    }

    private void SetCurrentWindow(Constants.Window window)
    {
        selectedWindow = window;
        currentWindowPane = windowPanes[selectedWindow];
    }

    private void ChangeWindow(Constants.Window window)
    {
        if (window == selectedWindow)
            return;

        tabsPane.SelectedTab = window;

        Visible = true;
    }
}

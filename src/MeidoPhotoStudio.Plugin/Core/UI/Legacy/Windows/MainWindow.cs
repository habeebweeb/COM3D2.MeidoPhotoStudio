using MeidoPhotoStudio.Plugin.Framework.Service;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

/// <summary>Main window.</summary>
public partial class MainWindow : BaseWindow
{
    public const int MinimumWindowWidth = 255;

    private const float ResizeHandleSize = 15f;
    private const float MinimumWindowHeight = 400f;
    private readonly TabSelectionController tabSelectionController;
    private readonly CustomMaidSceneService customMaidSceneService;
    private readonly SettingsWindow settingsWindow;
    private readonly InputRemapper inputRemapper;
    private readonly Dictionary<Constants.Window, BaseMainWindowPane> windowPanes = [];
    private readonly TabsPane tabsPane;
    private readonly Button settingsButton;
    private readonly LazyStyle pluginInfoStyle = new(
        10,
        static () => new(GUI.skin.label)
        {
            alignment = TextAnchor.LowerLeft,
        });

    private readonly LazyStyle buttonStyle = new(13, static () => new(GUI.skin.button));

    private int windowWidth = MinimumWindowWidth;
    private Rect resizeHandleRect = new(0f, 0f, ResizeHandleSize, ResizeHandleSize);
    private bool resizing;
    private BaseMainWindowPane currentWindowPane;
    private Constants.Window selectedWindow;

    public MainWindow(
        TabSelectionController tabSelectionController,
        CustomMaidSceneService customMaidSceneService,
        InputRemapper inputRemapper,
        SettingsWindow settingsWindow)
    {
        this.tabSelectionController = tabSelectionController ?? throw new ArgumentNullException(nameof(tabSelectionController));
        this.customMaidSceneService = customMaidSceneService ?? throw new ArgumentNullException(nameof(customMaidSceneService));
        this.inputRemapper = inputRemapper ? inputRemapper : throw new ArgumentNullException(nameof(inputRemapper));
        this.settingsWindow = settingsWindow ?? throw new ArgumentNullException(nameof(settingsWindow));

        this.tabSelectionController.TabSelected += OnTabSelected;

        tabsPane = new TabsPane();
        tabsPane.TabChange += OnTabChanged;

        settingsButton = new(Translation.Get("mainWindow", "settingsButton"));
        settingsButton.ControlEvent += OnSettingsButtonPushed;

        WindowRect = new(Screen.width, Screen.height * 0.08f, WindowWidth, Screen.height * 0.9f);
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

    protected override void ReloadTranslation() =>
        settingsButton.Label = Translation.Get("mainWindow", "settingsButton");

    private void OnTabSelected(object sender, TabSelectionEventArgs e) =>
        ChangeWindow(e.Tab);

    private void OnTabChanged(object sender, EventArgs e) =>
        SetCurrentWindow(tabsPane.SelectedTab);

    private void OnSettingsButtonPushed(object sender, EventArgs e) =>
        settingsWindow.Visible = !settingsWindow.Visible;

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

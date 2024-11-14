using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class SettingsWindow : BaseWindow
{
    private const int CategoryListWidth = 200;
    private const float ResizeHandleSize = 15f;

    private readonly List<SettingType> settingCategories = [];
    private readonly Dictionary<SettingType, BasePane> settingPanes = [];
    private readonly SelectionGrid settingCategorySelectionGrid;
    private readonly Button closeButton;
    private readonly Header currentSettingHeader;
    private readonly LazyStyle settingCategoryStyle = new(
        15,
        static () => new(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
        });

    private readonly LazyStyle headerStyle = new(
        15,
        static () => new(GUI.skin.label)
        {
            padding = new(0, 0, 2, 0),
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white },
            wordWrap = true,
        });

    private bool resizing;
    private Rect resizeHandlRect = new(0f, 0f, ResizeHandleSize, ResizeHandleSize);
    private Vector2 settingsCategoryScrollPosition;
    private Vector2 settingsScrollPosition;

    public SettingsWindow()
    {
        closeButton = new("X");
        closeButton.ControlEvent += OnCloseButtonPushed;

        currentSettingHeader = new(string.Empty);

        settingCategorySelectionGrid = new([])
        {
            Vertical = true,
        };

        settingCategorySelectionGrid.ControlEvent += OnSettingCategoryChanged;

        var minimumWidth = Utility.GetPix(CategoryListWidth * 2.75f + 38);
        var minimumHeight = Utility.GetPix(400);

        WindowRect = new(
            Screen.width * 0.5f - minimumWidth / 2f,
            Screen.height * 0.5f - Screen.height * 0.6f / 2f,
            minimumWidth,
            Screen.height * 0.6f);
    }

    public enum SettingType
    {
        Controls,
        DragHandle,
        AutoSave,
        Translation,
        UI,
    }

    public BasePane this[SettingType settingType]
    {
        get => settingPanes[settingType];
        set => AddPane(settingType, value);
    }

    public override void Draw()
    {
        GUILayout.BeginArea(new(10f, 10f, WindowRect.width - 20f, WindowRect.height - 20f));

        GUILayout.BeginHorizontal();

        DrawSettingsCategories();

        DrawSettingsPane();

        GUILayout.EndHorizontal();

        GUILayout.EndArea();

        void DrawSettingsCategories()
        {
            var categoryWidth = GUILayout.Width(Utility.GetPix(CategoryListWidth));

            GUILayout.BeginVertical(categoryWidth);

            settingsCategoryScrollPosition = GUILayout.BeginScrollView(settingsCategoryScrollPosition);

            settingCategorySelectionGrid.Draw(settingCategoryStyle);

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        void DrawSettingsPane()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            currentSettingHeader.Draw(headerStyle);

            closeButton.Draw(GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();

            MpsGui.WhiteLine();

            settingsScrollPosition = GUILayout.BeginScrollView(settingsScrollPosition);

            settingPanes[settingCategories[settingCategorySelectionGrid.SelectedItemIndex]].Draw();

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
    }

    public override void GUIFunc(int id)
    {
        HandleResize();
        Draw();

        GUI.Box(resizeHandlRect, GUIContent.none);

        if (!resizing)
            GUI.DragWindow();

        void HandleResize()
        {
            resizeHandlRect = resizeHandlRect with
            {
                x = WindowRect.width - ResizeHandleSize,
                y = WindowRect.height - ResizeHandleSize,
            };

            if (resizing && !Input.GetMouseButton(0))
                resizing = false;
            else if (!resizing && Input.GetMouseButtonDown(0) && resizeHandlRect.Contains(Event.current.mousePosition))
                resizing = true;

            if (resizing)
            {
                var mousePosition = Event.current.mousePosition;

                var (windowWidth, windowHeight) = mousePosition;
                var minimumWidth = Utility.GetPix(CategoryListWidth * 2.75f + 38);
                var minimumHeight = Utility.GetPix(400);

                WindowRect = WindowRect with
                {
                    width = Mathf.Max(minimumWidth, windowWidth + ResizeHandleSize / 2f),
                    height = Mathf.Max(minimumHeight, windowHeight + ResizeHandleSize / 2f),
                };
            }
        }
    }

    public override void OnScreenDimensionsChanged(Vector2 newScreenDimensions)
    {
        base.OnScreenDimensionsChanged(newScreenDimensions);

        var minimumWidth = Utility.GetPix(CategoryListWidth * 2.75f + 38);
        var minimumHeight = Utility.GetPix(400);

        WindowRect = WindowRect with
        {
            width = Mathf.Clamp(WindowRect.width, minimumWidth, Screen.width),
            height = Mathf.Clamp(WindowRect.height, minimumHeight, Screen.height),
        };
    }

    protected override void ReloadTranslation()
    {
        settingCategorySelectionGrid.SetItemsWithoutNotify(Translation.GetArray("settingTypes", settingCategories.Select(static category => category.ToLower())));
        currentSettingHeader.Text = Translation.Get("settingTypes", settingCategories[settingCategorySelectionGrid.SelectedItemIndex].ToLower());
    }

    private void AddPane(SettingType settingType, BasePane pane)
    {
        _ = pane ?? throw new ArgumentNullException(nameof(pane));

        if (settingPanes.ContainsKey(settingType))
            return;

        settingCategories.Add(settingType);

        settingPanes.Add(settingType, pane);
        pane.SetParent(this);

        settingCategorySelectionGrid.SetItems(Translation.GetArray("settingTypes", settingCategories.Select(static setting => setting.ToLower())), 0);
    }

    private void OnCloseButtonPushed(object sender, EventArgs e) =>
        Visible = !Visible;

    private void OnSettingCategoryChanged(object sender, EventArgs e) =>
        currentSettingHeader.Text = Translation.Get("settingTypes", settingCategories[settingCategorySelectionGrid.SelectedItemIndex].ToLower());
}

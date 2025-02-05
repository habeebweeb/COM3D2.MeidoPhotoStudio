namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public abstract class BasePane : IEnumerable<BasePane>
{
    private static readonly GUILayoutOption[] ComboBoxLayoutOptions = new GUILayoutOption[2];
    private static readonly GUILayoutOption[] ComboBoxButtonLayoutOptions = new GUILayoutOption[2];
    private static readonly GUILayoutOption[] DropdownArrowLayoutOptions = new GUILayoutOption[2];
    private static readonly GUILayoutOption[] TextFieldLayoutOptions = new GUILayoutOption[2];

    private static int textFieldHeight;

    private List<BasePane> panes;

    static BasePane()
    {
        ScreenSizeChecker.ScreenSizeChanged += OnScreenSizeChanged;

        RefreshLayoutOptions();

        static void OnScreenSizeChanged(object sender, EventArgs e) =>
            RefreshLayoutOptions();

        static void RefreshLayoutOptions()
        {
            textFieldHeight = Mathf.Max(21, UIUtility.Scaled(StyleSheet.TextSize) + 10);

            DropdownArrowLayoutOptions[0] = GUILayout.Width(UIUtility.Scaled(23));
            DropdownArrowLayoutOptions[1] = GUILayout.Height(UIUtility.Scaled(StyleSheet.TextSize) + 12);

            ComboBoxLayoutOptions[1] = GUILayout.Height(textFieldHeight);

            ComboBoxButtonLayoutOptions[0] = GUILayout.Width(UIUtility.Scaled(23));
            ComboBoxButtonLayoutOptions[1] = GUILayout.Height(textFieldHeight);

            TextFieldLayoutOptions[1] = GUILayout.Height(textFieldHeight);
        }
    }

    protected int PaneCount =>
        panes?.Count ?? 0;

    protected BaseWindow Parent { get; private set; }

    protected IEnumerable<BasePane> Panes =>
        panes ?? [];

    protected BasePane this[int index] =>
        panes[index];

    public abstract void Draw();

    public virtual void Add<T>(T pane)
        where T : BasePane
    {
        _ = pane ?? throw new ArgumentNullException(nameof(pane));

        panes ??= [];

        panes.Add(pane);
    }

    public virtual void SetParent(BaseWindow window)
    {
        _ = window ?? throw new ArgumentNullException(nameof(window));

        Parent = window;

        foreach (var pane in Panes)
            pane.SetParent(window);
    }

    public IEnumerator<BasePane> GetEnumerator() =>
        Panes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public virtual void UpdatePane()
    {
        foreach (var pane in Panes)
            pane.UpdatePane();
    }

    public virtual void OnScreenDimensionsChanged(Vector2 newScreenDimensions)
    {
        foreach (var pane in Panes)
            pane.OnScreenDimensionsChanged(newScreenDimensions);
    }

    public virtual void Activate()
    {
        foreach (var pane in Panes)
            pane.Activate();
    }

    public virtual void Deactivate()
    {
        foreach (var pane in Panes)
            pane.Deactivate();
    }

    protected void DrawDropdown<T>(Dropdown<T> dropdown)
    {
        GUILayout.BeginHorizontal();

        var buttonAndScrollbarSize = UIUtility.Scaled(23) * 2 + 27 + 15;
        var dropdownButtonWidth = Parent.WindowRect.width - buttonAndScrollbarSize;

        dropdown.Draw(GUILayout.Width(dropdownButtonWidth));

        if (GUILayout.Button(Symbols.LeftChevron, Symbols.IconButtonStyle, DropdownArrowLayoutOptions))
            dropdown.CyclePrevious();

        if (GUILayout.Button(Symbols.RightChevron, Symbols.IconButtonStyle, DropdownArrowLayoutOptions))
            dropdown.CycleNext();

        GUILayout.EndHorizontal();
    }

    protected void DrawComboBox(ComboBox comboBox)
    {
        var buttonAndScrollbarSize = UIUtility.Scaled(23) + 22 + 15;

        ComboBoxLayoutOptions[0] = GUILayout.Width(Parent.WindowRect.width - buttonAndScrollbarSize);

        comboBox.Draw(TextField.Style, Symbols.IconButtonStyle, ComboBoxLayoutOptions, ComboBoxButtonLayoutOptions);
    }

    protected void DrawTextFieldMaxWidth(BaseControl textField)
    {
        TextFieldLayoutOptions[0] = GUILayout.Width(Parent.WindowRect.width - 8f);

        textField.Draw(TextFieldLayoutOptions);
    }

    protected void DrawTextFieldWithScrollBarOffset(BaseControl textField)
    {
        TextFieldLayoutOptions[0] = GUILayout.Width(Parent.WindowRect.width - 32f);

        textField.Draw(TextFieldLayoutOptions);
    }
}

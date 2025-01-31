namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public abstract class BasePane : IEnumerable<BasePane>
{
    private List<BasePane> panes;

    protected int PaneCount =>
        panes?.Count ?? 0;

    protected BaseWindow Parent { get; private set; }

    protected IEnumerable<BasePane> Panes =>
        panes ?? [];

    private int TextFieldHeight { get; } = Mathf.CeilToInt(StyleSheet.TextSize * 1.85f);

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

        var buttonAndScrollbarSize = 33 * 2 + 15;
        var dropdownButtonWidth = Parent.WindowRect.width - buttonAndScrollbarSize;

        dropdown.Draw(GUILayout.Width(dropdownButtonWidth));

        var arrowLayoutOptions = GUILayout.MaxWidth(20);

        if (GUILayout.Button("<", arrowLayoutOptions))
            dropdown.CyclePrevious();

        if (GUILayout.Button(">", arrowLayoutOptions))
            dropdown.CycleNext();

        GUILayout.EndHorizontal();
    }

    protected void DrawComboBox(ComboBox comboBox) =>
        comboBox.Draw(
            GUILayout.Width(Parent.WindowRect.width - 56f),
            GUILayout.Height(Mathf.Max(23f, UIUtility.Scaled(TextFieldHeight))));

    protected void DrawTextFieldMaxWidth(BaseControl textField) =>
        textField.Draw(
            GUILayout.Width(Parent.WindowRect.width - 10f),
            GUILayout.Height(Mathf.Max(23f, UIUtility.Scaled(TextFieldHeight))));

    protected void DrawTextFieldWithScrollBarOffset(BaseControl textField) =>
        textField.Draw(
            GUILayout.Width(Parent.WindowRect.width - 35f),
            GUILayout.Height(Mathf.Max(23f, UIUtility.Scaled(TextFieldHeight))));
}

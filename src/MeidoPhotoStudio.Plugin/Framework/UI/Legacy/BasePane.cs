namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public abstract class BasePane : IEnumerable<BasePane>
{
    private List<BasePane> panes;

    protected BasePane() =>
        Translation.ReloadTranslationEvent += OnReloadTranslation;

    protected BaseWindow Parent { get; private set; }

    protected IEnumerable<BasePane> Panes =>
        panes ?? [];

    public abstract void Draw();

    public void Add<T>(T pane)
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

    protected virtual void ReloadTranslation()
    {
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
            GUILayout.Height(Utility.GetPix(22f)));

    protected void DrawTextFieldMaxWidth(BaseControl textField) =>
        textField.Draw(
            GUILayout.Width(Parent.WindowRect.width - 10f),
            GUILayout.Height(Utility.GetPix(22f)));

    protected void DrawTextFieldWithScrollBarOffset(BaseControl textField) =>
        textField.Draw(
            GUILayout.Width(Parent.WindowRect.width - 35f),
            GUILayout.Height(Utility.GetPix(22f)));

    private void OnReloadTranslation(object sender, EventArgs args) =>
        ReloadTranslation();
}

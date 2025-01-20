namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class Toggle(GUIContent content, bool state = false) : BaseControl
{
    private bool value = state;
    private GUIContent content = content ?? new();

    public Toggle(string label, bool state = false)
        : this(new GUIContent(label ?? string.Empty), state)
    {
    }

    public Toggle(Texture icon, bool state = false)
        : this(new GUIContent(icon), state)
    {
    }

    public static LazyStyle Style { get; } = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.toggle)
        {
            wordWrap = true,
        });

    public string Label
    {
        get => Content.text;
        set => Content.text = value ?? string.Empty;
    }

    public Texture Icon
    {
        get => Content.image;
        set => Content.image = value;
    }

    public GUIContent Content
    {
        get => content;
        set => content = value ?? new();
    }

    public bool Value
    {
        get => value;
        set => SetEnabled(value);
    }

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(Style, layoutOptions);

    public void Draw(GUIStyle toggleStyle, params GUILayoutOption[] layoutOptions)
    {
        var value = GUILayout.Toggle(Value, content, toggleStyle, layoutOptions);

        if (value != Value)
            Value = value;
    }

    public void SetEnabledWithoutNotify(bool enabled) =>
        SetEnabled(enabled, false);

    private void SetEnabled(bool enabled, bool notify = true)
    {
        value = enabled;

        if (notify)
            OnControlEvent(EventArgs.Empty);
    }
}

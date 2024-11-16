namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class Header(string text) : BaseControl
{
    private string text = text;
    private GUIContent content = new(text);

    public static LazyStyle Style { get; } = new(
        13,
        static () => new(GUI.skin.label)
        {
            padding = new(7, 0, 0, -5),
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white },
            wordWrap = true,
        });

    public string Text
    {
        get => text;
        set
        {
            text = value ?? string.Empty;
            content = new(value);
        }
    }

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(Style, layoutOptions);

    public void Draw(GUIStyle style, params GUILayoutOption[] layoutOptions) =>
        GUILayout.Label(content, style, layoutOptions);
}

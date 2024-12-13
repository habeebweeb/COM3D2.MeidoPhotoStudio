namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class Label(string text) : BaseControl
{
    private string text = text;
    private GUIContent content = new(text);

    public static LazyStyle Style { get; } = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.label)
        {
            wordWrap = true,
        });

    public string Text
    {
        get => text;
        set
        {
            text = value ?? string.Empty;
            content = new GUIContent(value);
        }
    }

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(Style, layoutOptions);

    public virtual void Draw(GUIStyle labelStyle, params GUILayoutOption[] layoutOptions) =>
        GUILayout.Label(content, labelStyle, layoutOptions);
}

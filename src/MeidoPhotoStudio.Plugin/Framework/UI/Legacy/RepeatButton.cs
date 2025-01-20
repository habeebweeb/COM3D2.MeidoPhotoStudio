namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class RepeatButton : BaseControl
{
    private GUIContent content;
    private float startClickTime;
    private float holdTime;
    private bool clicked;
    private float interval;

    public RepeatButton(GUIContent content, float interval)
    {
        this.content = content ?? new();
        Interval = interval;
    }

    public RepeatButton(string label, float interval = 0f)
        : this(new GUIContent(label ?? string.Empty), interval)
    {
    }

    public RepeatButton(Texture icon, float interval = 0f)
        : this(new GUIContent(icon), interval)
    {
    }

    public static LazyStyle Style { get; } = new(StyleSheet.TextSize, static () => new(GUI.skin.button));

    public string Label
    {
        get => content.text;
        set => content.text = value ?? string.Empty;
    }

    public Texture Icon
    {
        get => content.image;
        set => content.image = value;
    }

    public GUIContent Content
    {
        get => content;
        set => content = value ?? new();
    }

    public float Interval
    {
        get => interval;
        set
        {
            if (interval < 0f)
                interval = 0f;

            interval = value;
        }
    }

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(Style, layoutOptions);

    public void Draw(GUIStyle buttonStyle, params GUILayoutOption[] layoutOptions)
    {
        if (GUILayout.RepeatButton(content, buttonStyle, layoutOptions))
        {
            if (!clicked)
            {
                clicked = true;
                startClickTime = Time.time;
                holdTime = Time.time;
                OnControlEvent(EventArgs.Empty);
            }
            else
            {
                if (Time.time - startClickTime >= 1f)
                {
                    if (Time.time - holdTime >= interval * 0.01f)
                    {
                        holdTime = Time.time;
                        OnControlEvent(EventArgs.Empty);
                    }
                }
            }
        }

        if (clicked && !UnityEngine.Input.GetMouseButton(0) && Event.current.type is EventType.Repaint)
            clicked = false;
    }
}

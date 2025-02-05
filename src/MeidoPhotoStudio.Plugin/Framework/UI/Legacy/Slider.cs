namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class Slider : BaseControl
{
    private const string SliderBackgroundBase64 =
        """
        iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAZElEQVQ4y2NkQAX/GYgDjOiM/wwM
        DAziDGpE6X7JcAuunwnGIlYzulpGBgaG/7YqfgzkgMN3NjEwMVAIWBgYGBgURDQHzgWjBgwGA+B5
        IdainCSNi090ouYFqAApmlGzJbnZGQCkbRQt6+fbKAAAAABJRU5ErkJggg==
        """;

    private bool hasContent;
    private float value;
    private float left;
    private float right;
    private float defaultValue;
    private bool hasTextField;
    private float temporaryValue;
    private NumericalTextField textField;
    private GUIContent content;

    public Slider(float left, float right, float value = 0, float defaultValue = 0)
    {
        this.left = left;
        this.right = right;
        SetValue(value, false);
        DefaultValue = defaultValue;
    }

    public Slider(string label, float left, float right, float value = 0, float defaultValue = 0)
        : this(new GUIContent(label ?? string.Empty), left, right, value, defaultValue)
    {
    }

    public Slider(GUIContent content, float left, float right, float value = 0, float defaultValue = 0)
        : this(left, right, value, defaultValue)
    {
        Content = content ?? new();
        hasContent = true;
    }

    public event EventHandler StartedInteraction;

    public event EventHandler EndedInteraction;

    public event EventHandler PushingResetButton;

    public static LazyStyle LabelStyle { get; } = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.label)
        {
            alignment = TextAnchor.LowerLeft,
            normal = { textColor = Color.white },
        });

    public static LazyStyle SliderStyle { get; } = new(
        0,
        static () => new(GUI.skin.horizontalSlider)
        {
            normal = { background = UIUtility.LoadTextureFromBase64(16, 16, SliderBackgroundBase64) },
        });

    public static LazyStyle NoLabelSliderStyle { get; } = new(
        0,
        static () => new(GUI.skin.horizontalSlider)
        {
            margin = { top = 10 },
            normal = { background = UIUtility.LoadTextureFromBase64(16, 16, SliderBackgroundBase64) },
        });

    public static LazyStyle SliderThumbStyle { get; } = new(0, static () => new(GUI.skin.horizontalSliderThumb));

    public static LazyStyle ResetButtonStyle { get; } = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleRight,
        });

    public bool HasReset { get; set; }

    public string Label
    {
        get => content?.text;
        set
        {
            if (value is null)
            {
                Content = null;

                return;
            }

            Content ??= new();
            Content.text = value;
        }
    }

    public GUIContent Content
    {
        get => content;
        set
        {
            content = value;
            hasContent = content is not null;
        }
    }

    public float Value
    {
        get => value;
        set => SetValue(value);
    }

    public float Left
    {
        get => left;
        set => SetBounds(left: value);
    }

    public float Right
    {
        get => right;
        set => SetBounds(right: value);
    }

    public float DefaultValue
    {
        get => defaultValue;
        set => defaultValue = Bound(value, Left, Right);
    }

    public bool HasTextField
    {
        get => hasTextField;
        set
        {
            hasTextField = value;

            if (hasTextField)
            {
                textField = new(Value);
                textField.ControlEvent += TextFieldInputChangedHandler;
            }
            else
            {
                if (textField is not null)
                    textField.ControlEvent -= TextFieldInputChangedHandler;

                textField = null;
            }
        }
    }

    public bool Dragging { get; private set; }

    public override void Draw(params GUILayoutOption[] layoutOptions)
    {
        var hasUpper = hasContent || HasTextField || HasReset;

        if (hasUpper)
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
            GUILayout.BeginHorizontal();

            if (hasContent)
            {
                GUILayout.Label(content, LabelStyle, GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
            }

            if (HasTextField)
                textField.Draw(GUILayout.Width(60f));

            if (HasReset && GUILayout.Button("|", ResetButtonStyle, GUILayout.ExpandWidth(false)))
                OnResetButtonPushed();

            GUILayout.EndHorizontal();
        }

        var sliderStyle = hasUpper ? SliderStyle : NoLabelSliderStyle;

        temporaryValue =
            GUILayout.HorizontalSlider(temporaryValue, Left, Right, sliderStyle, SliderThumbStyle, layoutOptions);

        var @event = Event.current;

        if (!Dragging
            && UnityEngine.Input.GetMouseButtonDown(0)
            && @event.type is EventType.Repaint
            && GUILayoutUtility.GetLastRect().Contains(@event.mousePosition))
        {
            Dragging = true;

            StartedInteraction?.Invoke(this, EventArgs.Empty);
        }

        if (hasUpper)
            GUILayout.EndVertical();

        if (@event.type is EventType.Repaint && !Mathf.Approximately(Value, temporaryValue))
            Value = temporaryValue;

        if (Dragging && UnityEngine.Input.GetMouseButtonUp(0))
        {
            Dragging = false;

            EndedInteraction?.Invoke(this, EventArgs.Empty);
        }

        void OnResetButtonPushed()
        {
            PushingResetButton?.Invoke(this, EventArgs.Empty);

            ResetValue();
        }
    }

    public void SetValueWithoutNotify(float value) =>
        SetValue(value, false);

    public void SetBounds(float left, float right) =>
        SetBounds(left, right, true);

    public void SetBoundsWithoutNotify(float left, float right) =>
        SetBounds(left, right, false);

    public void SetLeftBoundWithoutNotify(float left) =>
        SetBounds(left: left, notify: false);

    public void SetRightBoundWithoutNotify(float right) =>
        SetBounds(right: right, notify: false);

    public void ResetValue() =>
        Value = DefaultValue;

    private static float Bound(float value, float left, float right) =>
        left > (double)right ? Mathf.Clamp(value, right, left) : Mathf.Clamp(value, left, right);

    private void SetValue(float value, bool notify = true, bool updateTextField = true)
    {
        var newValue = Bound(value, Left, Right);

        if (this.value == newValue)
            return;

        this.value = temporaryValue = newValue;

        if (updateTextField)
            textField?.SetValueWithoutNotify(this.value);

        if (notify)
            OnControlEvent(EventArgs.Empty);
    }

    private void SetBounds(float? left = null, float? right = null, bool notify = true)
    {
        if (left.HasValue)
            this.left = (float)left;

        if (right.HasValue)
            this.right = (float)right;

        SetValue(value, notify);
    }

    private void TextFieldInputChangedHandler(object sender, EventArgs e) =>
        SetValue(textField.Value, updateTextField: false);
}

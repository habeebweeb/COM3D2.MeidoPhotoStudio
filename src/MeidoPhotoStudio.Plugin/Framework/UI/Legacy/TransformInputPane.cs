using MeidoPhotoStudio.Plugin.Framework.Service;

using TransformType = MeidoPhotoStudio.Plugin.Framework.Service.TransformClipboard.TransformType;

namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class TransformInputPane : BasePane
{
    private readonly TransformControl positionControl;
    private readonly TransformControl rotationControl;
    private readonly TransformControl scaleControl;

    private bool internalChange;
    private IObservableTransform target;

    public TransformInputPane(TransformClipboard transformClipboard)
    {
        _ = transformClipboard ?? throw new ArgumentNullException(nameof(transformClipboard));

        positionControl = new(transformClipboard, TransformType.Position);
        positionControl.ControlEvent += OnPositionChanged;

        rotationControl = new(transformClipboard, TransformType.Rotation);
        rotationControl.ControlEvent += OnRotationChanged;

        scaleControl = new(transformClipboard, TransformType.Scale);
        scaleControl.ControlEvent += OnScaleChanged;
    }

    public Space Space { get; set; } = Space.World;

    public bool EnablePosition { get; set; } = true;

    public bool EnableRotation { get; set; } = true;

    public bool EnableScale { get; set; } = true;

    public bool LinkScale
    {
        get => scaleControl.LinkFields;
        set => scaleControl.LinkFields = value;
    }

    public IObservableTransform Target
    {
        get => target;
        set
        {
            if (target is not null)
                target.ChangedTransform -= OnTransformChanged;

            target = value;

            if (target is not null)
                target.ChangedTransform += OnTransformChanged;

            positionControl.SetValueWithoutNotify(Position);
            rotationControl.SetValueWithoutNotify(Rotation);
            scaleControl.SetValueWithoutNotify(Scale);

            var initialTransform = target?.InitialTransform ?? new(Space, Vector3.zero, Quaternion.identity, Vector3.one);

            positionControl.DefaultValue = initialTransform.Position;
            rotationControl.DefaultValue = initialTransform.Rotation.eulerAngles;
            scaleControl.DefaultValue = initialTransform.LocalScale;
        }
    }

    public Vector3 Position =>
        !Transform ? Vector3.zero :
        Space is Space.Self ? Transform.localPosition :
        Transform.position;

    public Vector3 Rotation =>
        !Transform ? Vector3.zero :
        Space is Space.Self ? Transform.localEulerAngles :
        Transform.eulerAngles;

    public Vector3 Scale =>
        !Transform ? Vector3.one : Transform.localScale;

    private Transform Transform =>
        Target.Transform;

    public override void Draw()
    {
        GUI.enabled = Parent.Enabled && Target is not null;

        var fieldWidth = GUILayout.Width((Parent.WindowRect.width - 23f * 3 - 18f) / 3f);

        if (EnablePosition)
            positionControl.Draw(fieldWidth);

        if (EnableRotation)
            rotationControl.Draw(fieldWidth);

        if (EnableScale)
            scaleControl.Draw(fieldWidth);
    }

    protected override void ReloadTranslation()
    {
        positionControl.ReloadTranslation();
        rotationControl.ReloadTranslation();
        scaleControl.ReloadTranslation();
    }

    private void OnTransformChanged(object sender, EventArgs e)
    {
        if (internalChange)
        {
            internalChange = false;

            return;
        }

        positionControl.SetValueWithoutNotify(Position);
        rotationControl.SetValueWithoutNotify(Rotation);
        scaleControl.SetValueWithoutNotify(Scale);
    }

    private void OnPositionChanged(object sender, EventArgs e)
    {
        if (!Transform)
            return;

        internalChange = true;

        if (Space is Space.Self)
            Transform.localPosition = positionControl.Value;
        else
            Transform.position = positionControl.Value;
    }

    private void OnRotationChanged(object sender, EventArgs e)
    {
        if (!Transform)
            return;

        internalChange = true;

        if (Space is Space.Self)
            Transform.localEulerAngles = rotationControl.Value;
        else
            Transform.eulerAngles = rotationControl.Value;
    }

    private void OnScaleChanged(object sender, EventArgs e)
    {
        if (!Transform)
            return;

        var value = scaleControl.Value;

        if (value.x < 0f || value.y < 0f || value.z < 0f)
            return;

        internalChange = true;

        Transform.localScale = scaleControl.Value;
    }

    private class TransformControl : BaseControl
    {
        private static readonly LazyStyle HeaderStyle = new(
            14,
            static () => new(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
            });

        private static readonly LazyStyle LabelStyle = new(13, static () => new(GUI.skin.label));
        private static readonly GUIContent XLabelContent = new("X");
        private static readonly GUIContent YLabelContent = new("Y");
        private static readonly GUIContent ZLabelContent = new("Z");

        private readonly TransformClipboard clipboard;
        private readonly TransformType transformType;
        private readonly Button copyButton;
        private readonly Button pasteButton;
        private readonly Button resetButton;
        private readonly NumericalTextField xTextField;
        private readonly NumericalTextField yTextField;
        private readonly NumericalTextField zTextField;
        private readonly Label header;

        public TransformControl(TransformClipboard clipboard, TransformType transformType)
        {
            this.clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));
            this.transformType = transformType;

            header = new(Header(transformType));

            copyButton = new(Translation.Get("transformInputPane", "copyButton"));
            copyButton.ControlEvent += OnCopyButtonPushed;

            pasteButton = new(Translation.Get("transformInputPane", "pasteButton"));
            pasteButton.ControlEvent += OnPasteButtonPushed;

            resetButton = new(Translation.Get("transformInputPane", "resetButton"));
            resetButton.ControlEvent += OnResetButtonPushed;

            xTextField = new(DefaultValue.x);
            yTextField = new(DefaultValue.y);
            zTextField = new(DefaultValue.z);

            xTextField.ControlEvent += OnXTextFieldChanged;
            yTextField.ControlEvent += OnYTextFieldChanged;
            zTextField.ControlEvent += OnZTextFieldChanged;
        }

        public Vector3 DefaultValue { get; set; }

        public Vector3 Value
        {
            get => new(xTextField.Value, yTextField.Value, zTextField.Value);
            set => SetValue(value);
        }

        public bool LinkFields { get; set; }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            var noExpandWidth = GUILayout.ExpandWidth(false);

            GUILayout.BeginHorizontal();

            header.Draw(HeaderStyle);

            GUILayout.FlexibleSpace();

            if (clipboard is not null)
            {
                copyButton.Draw(noExpandWidth);
                pasteButton.Draw(noExpandWidth);
            }

            resetButton.Draw(noExpandWidth);

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Label(XLabelContent, LabelStyle);
            xTextField.Draw(layoutOptions);

            GUILayout.Label(YLabelContent, LabelStyle);
            yTextField.Draw(layoutOptions);

            GUILayout.Label(ZLabelContent, LabelStyle);
            zTextField.Draw(layoutOptions);

            GUILayout.EndHorizontal();
        }

        public void ReloadTranslation()
        {
            header.Text = Header(transformType);
            copyButton.Label = Translation.Get("transformInputPane", "copyButton");
            pasteButton.Label = Translation.Get("transformInputPane", "pasteButton");
            resetButton.Label = Translation.Get("transformInputPane", "resetButton");
        }

        public void SetValueWithoutNotify(Vector3 value) =>
            SetValue(value, false);

        private static string Header(TransformType transformType) =>
            transformType switch
            {
                TransformType.Position => Translation.Get("transformInputPane", "positionHeader"),
                TransformType.Rotation => Translation.Get("transformInputPane", "rotationHeader"),
                TransformType.Scale => Translation.Get("transformInputPane", "scaleHeader"),
                _ => transformType.ToString(),
            };

        private void OnCopyButtonPushed(object sender, EventArgs e) =>
            clipboard[transformType] = Value;

        private void OnPasteButtonPushed(object sender, EventArgs e)
        {
            if (clipboard[transformType] is not Vector3 value)
                return;

            if (LinkFields)
            {
                var average = (value.x + value.y + value.z) / 3f;

                value = Vector3.one * average;
            }

            SetValue(value);
        }

        private void OnResetButtonPushed(object sender, EventArgs e) =>
            SetValue(DefaultValue);

        private void OnXTextFieldChanged(object sender, EventArgs e) =>
            SetValue(LinkFields ? Vector3.one * xTextField.Value : Value);

        private void OnYTextFieldChanged(object sender, EventArgs e) =>
            SetValue(LinkFields ? Vector3.one * yTextField.Value : Value);

        private void OnZTextFieldChanged(object sender, EventArgs e) =>
            SetValue(LinkFields ? Vector3.one * zTextField.Value : Value);

        private void SetValue(Vector3 value, bool notify = true)
        {
            xTextField.SetValueWithoutNotify(value.x);
            yTextField.SetValueWithoutNotify(value.y);
            zTextField.SetValueWithoutNotify(value.z);

            if (!notify)
                return;

            OnControlEvent(EventArgs.Empty);
        }
    }
}

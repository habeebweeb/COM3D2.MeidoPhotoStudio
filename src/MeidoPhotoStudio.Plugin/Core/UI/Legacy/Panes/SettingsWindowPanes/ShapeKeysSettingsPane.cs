using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class ShapeKeysSettingsPane : BasePane
{
    private readonly FaceShapeKeySettingsPane faceShapeKeysSettingsPane;
    private readonly BodyShapeKeySettingsPane bodyShapeKeysSettingsPane;
    private readonly Toggle.Group tabGroup;
    private readonly Toggle faceTab;
    private readonly Toggle bodyTab;
    private readonly LazyStyle tabStyle = new(StyleSheet.TextSize, static () => new(GUI.skin.button));

    public ShapeKeysSettingsPane(
        Translation translation,
        FaceShapeKeyConfiguration faceShapeKeyConfiguration,
        ShapeKeyRangeConfiguration faceShapeKeyRangeConfiguration,
        BodyShapeKeyConfiguration bodyShapeKeyConfiguration,
        ShapeKeyRangeConfiguration bodyShapeKeyRangeConfiguration)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        _ = faceShapeKeyConfiguration ?? throw new ArgumentNullException(nameof(faceShapeKeyConfiguration));
        _ = faceShapeKeyRangeConfiguration ?? throw new ArgumentNullException(nameof(faceShapeKeyRangeConfiguration));
        _ = bodyShapeKeyConfiguration ?? throw new ArgumentNullException(nameof(bodyShapeKeyConfiguration));
        _ = bodyShapeKeyRangeConfiguration ?? throw new ArgumentNullException(nameof(bodyShapeKeyRangeConfiguration));

        faceTab = new(new LocalizableGUIContent(translation, "shapeKeySettingsPane", "faceTab"), true);
        bodyTab = new(new LocalizableGUIContent(translation, "shapeKeySettingsPane", "bodyTab"), false);

        tabGroup = [faceTab, bodyTab];

        faceShapeKeysSettingsPane = new(translation, faceShapeKeyConfiguration, faceShapeKeyRangeConfiguration);
        Add(faceShapeKeysSettingsPane);

        bodyShapeKeysSettingsPane = new(translation, bodyShapeKeyConfiguration, bodyShapeKeyRangeConfiguration);
        Add(bodyShapeKeysSettingsPane);
    }

    public override void Draw()
    {
        GUILayout.BeginHorizontal();

        foreach (var tab in tabGroup)
            tab.Draw(tabStyle);

        GUILayout.EndHorizontal();

        UIUtility.DrawBlackLine();

        if (faceTab.Value)
            faceShapeKeysSettingsPane.Draw();
        else if (bodyTab.Value)
            bodyShapeKeysSettingsPane.Draw();
    }

    private abstract class ShapeKeySettingsPane : BasePane, IVirtualListHandler
    {
        protected static readonly LazyStyle LabelStyle = new(
            StyleSheet.TextSize,
            static () => new(GUI.skin.label)
            {
                wordWrap = false,
            });

        protected static readonly LazyStyle NoShapeKeysStyle = new(
            StyleSheet.TextSize,
            static () => new(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
            });

        protected readonly LazyStyle ButtonStyle = new(StyleSheet.TextSize, static () => new(GUI.skin.button));
        protected readonly LazyStyle NoShapeKeysLabelStyle = new(
            StyleSheet.TextSize,
            static () => new(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
            });

        private static float? buttonContentsWidth;

        private readonly List<string> shapeKeys;
        private readonly GUIContent resetRangeContent;
        private readonly TextField searchBar;
        private readonly Header shapeKeyHeaderLabel;
        private readonly Header lowerHeaderLabel;
        private readonly Header upperHeaderLabel;
        private readonly Button reloadConfigurationButton;
        private readonly Button resetChangesButton;
        private readonly Button saveChangesButton;
        private readonly Label noShapeKeysLabel;
        private readonly VirtualList virtualList;

        private Dictionary<string, ShapeKeyEditor> shapeKeyRangeFields;
        private float rangeEditorHeight;
        private Vector2 scrollPosition;
        private bool pendingChanges;

        public ShapeKeySettingsPane(
            Translation translation, ShapeKeyConfiguration shapeKeyConfiguration, ShapeKeyRangeConfiguration shapeKeyRangeConfiguration)
        {
            Translation = translation ?? throw new ArgumentNullException(nameof(translation));
            ShapeKeyConfiguration = shapeKeyConfiguration ?? throw new ArgumentNullException(nameof(shapeKeyConfiguration));
            ShapeKeyRangeConfiguration = shapeKeyRangeConfiguration ?? throw new ArgumentNullException(nameof(shapeKeyRangeConfiguration));

            ShapeKeyConfiguration.RemovedShapeKey += OnShapeKeyRemoved;
            ShapeKeyConfiguration.AddedShapeKey += OnShapeKeyAdded;

            ShapeKeyRangeConfiguration.ChangedRange += OnRangeChanged;
            ShapeKeyRangeConfiguration.Refreshed += OnRangeConfigurationRefreshed;

            Translation.Initialized += OnTranslationInitialized;

            searchBar = new(new LocalizableGUIContent(Translation, "shapeKeySettingsPane", "searchBarPlaceholder"));
            searchBar.ChangedValue += OnSearchQueryChanged;

            shapeKeyHeaderLabel = new(new LocalizableGUIContent(Translation, "shapeKeySettingsPane", "shapeKeyHeaderLabel"));
            lowerHeaderLabel = new(new LocalizableGUIContent(Translation, "shapeKeySettingsPane", "lowerHeaderLabel"));
            upperHeaderLabel = new(new LocalizableGUIContent(Translation, "shapeKeySettingsPane", "upperHeaderLabel"));
            resetRangeContent = new LocalizableGUIContent(Translation, "shapeKeySettingsPane", "resetRangeButton");

            reloadConfigurationButton = new(new LocalizableGUIContent(Translation, "shapeKeySettingsPane", "reloadConfigurationButton"));
            reloadConfigurationButton.ControlEvent += OnReloadConfigurationButtonPushed;

            resetChangesButton = new(new LocalizableGUIContent(Translation, "shapeKeySettingsPane", "resetChangesButton"));
            resetChangesButton.ControlEvent += OnResetChangesButtonPushed;

            saveChangesButton = new(new LocalizableGUIContent(Translation, "shapeKeySettingsPane", "saveChangesButton"));
            saveChangesButton.ControlEvent += OnSaveChangesButtonPushed;

            noShapeKeysLabel = new(new LocalizableGUIContent(Translation, "shapeKeySettingsPane", "noShapeKeysLabel"));

            shapeKeys = [.. ShapeKeys];

            virtualList = new()
            {
                Handler = this,
                BucketSize = 10,
                Spacing = new(0f, 5f),
            };

            rangeEditorHeight = UIUtility.Scaled(StyleSheet.TextSize) + 15;

            ScreenSizeChecker.ScreenSizeChanged += OnScreenSizeChanged;

            void OnScreenSizeChanged(object sender, EventArgs e)
            {
                rangeEditorHeight = UIUtility.Scaled(StyleSheet.TextSize) + 15;
                buttonContentsWidth = null;
            }

            void OnTranslationInitialized(object sender, EventArgs e) =>
                buttonContentsWidth = null;
        }

        public int Count =>
            shapeKeys.Count;

        protected Translation Translation { get; }

        protected Dictionary<string, ShapeKeyEditor> ShapeKeyRangeEditors
        {
            get
            {
                if (shapeKeyRangeFields is not null)
                    return shapeKeyRangeFields;

                shapeKeyRangeFields = new(StringComparer.Ordinal);

                foreach (var shapeKey in ShapeKeys)
                {
                    if (shapeKeyRangeFields.ContainsKey(shapeKey))
                        continue;

                    shapeKeyRangeFields.Add(shapeKey, CreateEditor(shapeKey));
                }

                return shapeKeyRangeFields;
            }
        }

        protected ShapeKeyConfiguration ShapeKeyConfiguration { get; }

        protected ShapeKeyRangeConfiguration ShapeKeyRangeConfiguration { get; }

        protected virtual IEnumerable<string> ShapeKeys =>
            ShapeKeyConfiguration.ShapeKeys;

        protected virtual int ShapeKeyCount =>
            ShapeKeyConfiguration.ShapeKeyCount;

        public Vector2 ItemDimensions(int index) =>
            new(0f, rangeEditorHeight);

        public override void Draw()
        {
            if (ShapeKeyCount is 0)
            {
                noShapeKeysLabel.Draw(NoShapeKeysLabelStyle);

                return;
            }

            buttonContentsWidth ??= ButtonStyle.Style.CalcSize(resetRangeContent).x;

            var windowRect = Parent.WindowRect;

            searchBar.Draw(GUILayout.Width(windowRect.width - UIUtility.Scaled(200) - 30));

            DrawHeader();

            DrawEditors();

            DrawFooter();

            void DrawHeader()
            {
                var textFieldWidth = GUILayout.Width(UIUtility.Scaled(65));

                GUILayout.BeginHorizontal();

                shapeKeyHeaderLabel.Draw();

                lowerHeaderLabel.Draw(textFieldWidth);

                upperHeaderLabel.Draw(textFieldWidth);

                GUILayout.Space(buttonContentsWidth.Value + 25);

                GUILayout.EndHorizontal();
            }

            void DrawEditors()
            {
                var scrollRect = GUILayoutUtility.GetRect(0f, windowRect.width, 0f, windowRect.height);

                scrollPosition = virtualList.BeginScrollView(scrollRect, scrollPosition);

                var textFieldWidth = UIUtility.Scaled(65);
                var textFieldHeight = rangeEditorHeight;
                var windowWidth = windowRect.width - UIUtility.Scaled(200) - 35;
                var labelWidth = windowWidth - textFieldWidth * 2 - buttonContentsWidth.Value - 20;

                foreach (var (i, offset) in virtualList)
                {
                    GUI.enabled = Parent.Enabled;

                    var shapeKey = shapeKeys[i];
                    var editor = ShapeKeyRangeEditors[shapeKey];
                    var (name, lower, upper) = editor;
                    var y = scrollRect.y + offset.y;

                    GUI.Label(new(5f, y, labelWidth, rangeEditorHeight), name, LabelStyle);

                    lower.Draw(new(labelWidth + 5, y, textFieldWidth, textFieldHeight));
                    upper.Draw(new(labelWidth + textFieldWidth + 10, y, textFieldWidth, textFieldHeight));

                    GUI.enabled = Parent.Enabled && editor.Changed;

                    if (GUI.Button(new(windowWidth - buttonContentsWidth.Value - 5, y, buttonContentsWidth.Value, rangeEditorHeight), resetRangeContent, ButtonStyle))
                        editor.Reset();
                }

                GUI.enabled = Parent.Enabled;

                GUI.EndScrollView();
            }

            void DrawFooter()
            {
                GUILayout.BeginHorizontal();

                reloadConfigurationButton.Draw();

                GUILayout.FlexibleSpace();

                GUI.enabled = Parent.Enabled && pendingChanges;

                resetChangesButton.Draw();
                saveChangesButton.Draw();

                GUI.enabled = Parent.Enabled;

                GUILayout.EndHorizontal();
            }
        }

        protected virtual GUIContent GetShapeKeyName(string shapeKey) =>
            new(shapeKey);

        private void OnShapeKeyRemoved(object sender, ShapeKeyConfigurationEventArgs e)
        {
            ShapeKeyRangeEditors.Remove(e.ChangedShapeKey);

            pendingChanges = CheckForPendingChanges();

            UpdateShapeKeyList();
        }

        private void OnShapeKeyAdded(object sender, ShapeKeyConfigurationEventArgs e)
        {
            ShapeKeyRangeConfiguration.AddRange(e.ChangedShapeKey, new(0f, 1f));
            ShapeKeyRangeEditors[e.ChangedShapeKey] = CreateEditor(e.ChangedShapeKey);

            pendingChanges = CheckForPendingChanges();

            UpdateShapeKeyList();
        }

        private void OnRangeConfigurationRefreshed(object sender, EventArgs e)
        {
            foreach (var shapeKey in ShapeKeys)
            {
                if (!ShapeKeyRangeEditors.TryGetValue(shapeKey, out var editor))
                    editor = CreateEditor(shapeKey);

                if (!ShapeKeyRangeConfiguration.TryGetRange(shapeKey, out var range))
                {
                    range = new(0f, 1f);

                    ShapeKeyRangeConfiguration.AddRange(shapeKey, range);
                }

                editor.SetRangeWithoutNotify(range);
            }

            pendingChanges = CheckForPendingChanges();
        }

        private void OnRangeChanged(object sender, ShapeKeyRangeConfigurationEventArgs e)
        {
            if (!ShapeKeyRangeEditors.TryGetValue(e.ChangedShapeKey, out var editor))
                return;

            editor.SetRangeWithoutNotify(e.Range);
        }

        private void OnSearchQueryChanged(object sender, EventArgs e) =>
            UpdateShapeKeyList();

        private void UpdateShapeKeyList()
        {
            shapeKeys.Clear();

            shapeKeys.AddRange(string.IsNullOrEmpty(searchBar.Value)
                ? ShapeKeys
                : ShapeKeys
                    .Where(ShapeKeyRangeEditors.ContainsKey)
                    .Where(shapeKey => shapeKey.Contains(searchBar.Value, StringComparison.OrdinalIgnoreCase)
                        || ShapeKeyRangeEditors[shapeKey].Name.text.Contains(searchBar.Value, StringComparison.OrdinalIgnoreCase)));
        }

        private void OnReloadConfigurationButtonPushed(object sender, EventArgs e) =>
            ShapeKeyRangeConfiguration.Refresh();

        private void OnResetChangesButtonPushed(object sender, EventArgs e)
        {
            foreach (var shapeKey in ShapeKeys)
            {
                var editor = ShapeKeyRangeEditors[shapeKey];

                editor.Reset();
            }

            pendingChanges = false;
        }

        private void OnSaveChangesButtonPushed(object sender, EventArgs e)
        {
            foreach (var (shapeKey, editor) in ShapeKeyRangeEditors)
                ShapeKeyRangeConfiguration.SetRange(shapeKey, editor.Range);

            ShapeKeyRangeConfiguration.Save();

            pendingChanges = false;
        }

        private ShapeKeyEditor CreateEditor(string shapeKey)
        {
            if (!ShapeKeyRangeConfiguration.TryGetRange(shapeKey, out var range))
            {
                range = new(0f, 1f);

                ShapeKeyRangeConfiguration.AddRange(shapeKey, range);
            }

            var editor = new ShapeKeyEditor(shapeKey, ShapeKeyRangeConfiguration, GetShapeKeyName(shapeKey), range);

            editor.ChangedRange += OnEditorChanged;

            return editor;

            void OnEditorChanged(object sender, EventArgs e)
            {
                var editor = (ShapeKeyEditor)sender;

                pendingChanges = CheckForPendingChanges();
            }
        }

        private bool CheckForPendingChanges() =>
            ShapeKeys
                .Where(ShapeKeyRangeConfiguration.ContainsKey)
                .Select(shapeKey => (EditorRange: ShapeKeyRangeEditors[shapeKey].Range, Range: ShapeKeyRangeConfiguration[shapeKey]))
                .Any(rangePair => rangePair.EditorRange != rangePair.Range);
    }

    private class FaceShapeKeySettingsPane(
        Translation translation,
        FaceShapeKeyConfiguration shapeKeyConfiguration,
        ShapeKeyRangeConfiguration shapeKeyRangeConfiguration)
        : ShapeKeySettingsPane(translation, shapeKeyConfiguration, shapeKeyRangeConfiguration)
    {
        private static readonly string[] BaseGameFaceHashes =
        [
            "eyeclose", "eyeclose2", "eyeclose3", "eyebig", "eyeclose6", "eyeclose5", "eyeclose8", "eyeclose7",
            "hitomih", "hitomis", "mayuha", "mayuw", "mayuup", "mayuv", "mayuvhalf", "moutha", "mouths", "mouthc",
            "mouthi", "mouthup", "mouthdw", "mouthhe", "mouthuphalf", "tangout", "tangup", "tangopen",
        ];

        protected override IEnumerable<string> ShapeKeys =>
            BaseGameFaceHashes.Concat(ShapeKeyConfiguration.ShapeKeys);

        protected override int ShapeKeyCount =>
            base.ShapeKeyCount + BaseGameFaceHashes.Length;

        protected override GUIContent GetShapeKeyName(string shapeKey) =>
            Translation.ContainsTranslation("faceBlendValues", shapeKey)
                ? new LocalizableGUIContent(Translation, "faceBlendValues", shapeKey, text => $"{text} ({shapeKey})")
                : base.GetShapeKeyName(shapeKey);
    }

    private class BodyShapeKeySettingsPane(
        Translation translation,
        BodyShapeKeyConfiguration shapeKeyConfiguration,
        ShapeKeyRangeConfiguration shapeKeyRangeConfiguration)
        : ShapeKeySettingsPane(translation, shapeKeyConfiguration, shapeKeyRangeConfiguration), IVirtualListHandler;

    private class ShapeKeyEditor
    {
        private readonly string shapeKey;
        private readonly ShapeKeyRangeConfiguration configuration;

        public ShapeKeyEditor(string shapeKey, ShapeKeyRangeConfiguration configuration, GUIContent name, ShapeKeyRange range)
        {
            if (string.IsNullOrEmpty(shapeKey))
                throw new ArgumentException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey));

            this.shapeKey = shapeKey;
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            LowerField = new(range.Lower);
            UpperField = new(range.Upper);

            LowerField.ChangedValue += OnRangeChanged;
            UpperField.ChangedValue += OnRangeChanged;
        }

        public event EventHandler ChangedRange;

        public GUIContent Name { get; }

        public SimpleTextField LowerField { get; }

        public SimpleTextField UpperField { get; }

        public bool Changed =>
            Range != configuration[shapeKey];

        public float Lower
        {
            get => LowerField.Value;
            set => LowerField.Value = value;
        }

        public float Upper
        {
            get => UpperField.Value;
            set => UpperField.Value = value;
        }

        public ShapeKeyRange Range
        {
            get => new(Lower, Upper);
            set => SetRange(value);
        }

        public void Reset() =>
            Range = configuration[shapeKey];

        public void SetRangeWithoutNotify(ShapeKeyRange range) =>
            SetRange(range, notify: false);

        public void Deconstruct(out GUIContent name, out SimpleTextField lowerField, out SimpleTextField upperField) =>
            (name, lowerField, upperField) = (Name, LowerField, UpperField);

        private void OnRangeChanged(object sender, EventArgs e) =>
            ChangedRange?.Invoke(this, EventArgs.Empty);

        private void SetRange(ShapeKeyRange range, bool notify = true)
        {
            LowerField.SetValueWithoutNotify(range.Lower);
            UpperField.SetValueWithoutNotify(range.Upper);

            if (notify)
                ChangedRange?.Invoke(this, EventArgs.Empty);
        }
    }

    private class SimpleTextField(float initialValue)
    {
        private float value = initialValue;
        private string textFieldValue = FormatValue(initialValue);

        public event EventHandler ChangedValue;

        public float Value
        {
            get => value;
            set => SetValue(value);
        }

        public void Draw(Rect rect) =>
            Draw(rect, NumericalTextField.Style);

        public void Draw(Rect rect, GUIStyle style)
        {
            var newText = GUI.TextField(rect, textFieldValue, style);

            if (string.Equals(newText, textFieldValue))
                return;

            textFieldValue = newText;

            if (!float.TryParse(textFieldValue, out var newValue))
                newValue = Value;

            if (!Mathf.Approximately(Value, newValue))
                SetValue(newValue, updateTextField: false);
        }

        public void SetValueWithoutNotify(float value) =>
            SetValue(value, notify: false);

        private static string FormatValue(float value) =>
            value.ToString("0.####");

        private void SetValue(float value, bool notify = true, bool updateTextField = true)
        {
            if (this.value == value)
                return;

            this.value = value;

            if (updateTextField)
                textFieldValue = FormatValue(this.value);

            if (notify)
                ChangedValue?.Invoke(this, EventArgs.Empty);
        }
    }
}

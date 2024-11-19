using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class ExpressionPane : BasePane
{
    private static readonly string[] EyeHashes =
    [
        "eyeclose", "eyeclose2", "eyeclose3", "eyebig", "eyeclose6", "eyeclose5", "eyeclose8", "eyeclose7", "hitomih",
        "hitomis", "mayuha", "mayuw", "mayuup", "mayuv", "mayuvhalf",
    ];

    private static readonly string[] MouthHashes =
    [
        "moutha", "mouths", "mouthc", "mouthi", "mouthup", "mouthdw", "mouthhe", "mouthuphalf", "tangout", "tangup",
        "tangopen",
    ];

    private static readonly string[] FaceHashes =
    [
        "hoho2", "shock", "nosefook", "namida", "yodare", "toothoff", "tear1", "tear2", "tear3", "hohos", "hoho",
        "hohol",
    ];

    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly ShapeKeyRangeConfiguration shapeKeyRangeConfiguration;
    private readonly Dictionary<string, BaseControl> controls = new(StringComparer.Ordinal);
    private readonly Toggle blinkToggle;
    private readonly Button refreshRangeButton;
    private readonly PaneHeader paneHeader;
    private readonly PaneHeader baseGameShapeKeyHeader;
    private readonly PaneHeader customShapeKeyHeader;
    private readonly ShapeKeysPane shapeKeysPane;

    public ExpressionPane(
        SelectionController<CharacterController> characterSelectionController,
        FaceShapeKeyConfiguration faceShapeKeyConfiguration,
        ShapeKeyRangeConfiguration shapeKeyRangeConfiguration)
    {
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));
        _ = faceShapeKeyConfiguration ?? throw new ArgumentNullException(nameof(faceShapeKeyConfiguration));
        this.shapeKeyRangeConfiguration = shapeKeyRangeConfiguration ?? throw new ArgumentNullException(nameof(shapeKeyRangeConfiguration));

        this.shapeKeyRangeConfiguration.Refreshed += OnFaceShapeKeyRangeConfigurationRefreshed;
        this.characterSelectionController.Selecting += OnCharacterSelectionChanging;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        paneHeader = new(Translation.Get("expressionPane", "header"), true);

        blinkToggle = new(Translation.Get("expressionPane", "blinkToggle"), true);
        blinkToggle.ControlEvent += OnBlinkToggleChanged;

        refreshRangeButton = new(Translation.Get("expressionPane", "refreshShapeKeyRangeButton"));
        refreshRangeButton.ControlEvent += OnRefreshRangeButtonPushed;

        baseGameShapeKeyHeader = new(Translation.Get("expressionPane", "baseGameExpressionKeys"));

        foreach (var hashKey in EyeHashes.Concat(MouthHashes))
        {
            if (!shapeKeyRangeConfiguration.TryGetRange(hashKey, out var range))
                range = new(0f, 1f);

            var slider = new Slider(Translation.Get("faceBlendValues", hashKey, false), range.Lower, range.Upper);

            slider.ControlEvent += OnControlChanged(hashKey);

            controls.Add(hashKey, slider);
        }

        customShapeKeyHeader = new(Translation.Get("expressionPane", "customExpressionKeys"));

        foreach (var hashKey in FaceHashes)
        {
            var toggle = new Toggle(Translation.Get("faceBlendValues", hashKey));

            toggle.ControlEvent += OnControlChanged(hashKey);

            controls.Add(hashKey, toggle);
        }

        shapeKeysPane = new(faceShapeKeyConfiguration, shapeKeyRangeConfiguration);
        Add(shapeKeysPane);

        EventHandler OnControlChanged(string hashKey) =>
            (object sender, EventArgs e) =>
            {
                if (CurrentFace is null)
                    return;

                var value = sender switch
                {
                    Slider slider => slider.Value,
                    Toggle toggle => Convert.ToSingle(toggle.Value),
                    _ => throw new NotSupportedException($"'{sender.GetType()} is not supported'"),
                };

                CurrentFace[hashKey] = value;
            };
    }

    private FaceController CurrentFace =>
        characterSelectionController.Current?.Face;

    public override void Draw()
    {
        var guiEnabled = Parent.Enabled && CurrentFace is not null;

        GUI.enabled = guiEnabled;

        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        GUILayout.BeginHorizontal();

        blinkToggle.Draw();

        refreshRangeButton.Draw(GUILayout.ExpandWidth(false));

        GUILayout.EndHorizontal();

        if (CurrentFace is null)
            return;

        UIUtility.DrawBlackLine();

        baseGameShapeKeyHeader.Draw();

        if (baseGameShapeKeyHeader.Enabled)
            DrawBuiltinTab();

        customShapeKeyHeader.Draw();

        if (customShapeKeyHeader.Enabled)
            shapeKeysPane.Draw();

        void DrawBuiltinTab()
        {
            const int SliderColumnCount = 2;
            const int ToggleColumnCount = 3;

            var maxWidth = GUILayout.MaxWidth(Parent.WindowRect.width - 10f);
            var sliderWidth = GUILayout.MaxWidth(Parent.WindowRect.width / SliderColumnCount - 10f);

            foreach (var chunk in EyeHashes
                .Where(CurrentFace.ContainsShapeKey)
                .Select(hash => controls[hash])
                .Chunk(SliderColumnCount))
            {
                GUILayout.BeginHorizontal(maxWidth);

                foreach (var slider in chunk)
                    slider.Draw(sliderWidth);

                GUILayout.EndHorizontal();
            }

            foreach (var chunk in MouthHashes
                .Where(CurrentFace.ContainsShapeKey)
                .Select(hash => controls[hash])
                .Chunk(SliderColumnCount))
            {
                GUILayout.BeginHorizontal(maxWidth);

                foreach (var slider in chunk)
                    slider.Draw(sliderWidth);

                GUILayout.EndHorizontal();
            }

            UIUtility.DrawBlackLine();

            foreach (var chunk in FaceHashes
                .Where(CurrentFace.ContainsShapeKey)
                .Select(hash => controls[hash])
                .Chunk(ToggleColumnCount))
            {
                GUILayout.BeginHorizontal(maxWidth);

                foreach (var toggle in chunk)
                    toggle.Draw();

                GUILayout.EndHorizontal();
            }
        }
    }

    protected override void ReloadTranslation()
    {
        foreach (var (hashKey, control) in EyeHashes.Concat(MouthHashes).Concat(FaceHashes).Select(hashKey => (hashKey, controls[hashKey])))
        {
            var translation = Translation.Get("faceBlendValues", hashKey);

            if (control is Slider slider)
                slider.Label = translation;
            else if (control is Toggle toggle)
                toggle.Label = translation;
        }

        blinkToggle.Label = Translation.Get("expressionPane", "blinkToggle");
        paneHeader.Label = Translation.Get("expressionPane", "header");
        refreshRangeButton.Label = Translation.Get("expressionPane", "refreshShapeKeyRangeButton");
        baseGameShapeKeyHeader.Label = Translation.Get("expressionPane", "baseGameExpressionKeys");
        customShapeKeyHeader.Label = Translation.Get("expressionPane", "customExpressionKeys");
    }

    private void OnRefreshRangeButtonPushed(object sender, EventArgs e) =>
        shapeKeyRangeConfiguration.Refresh();

    private void OnBlinkToggleChanged(object sender, EventArgs e)
    {
        if (CurrentFace is null)
            return;

        CurrentFace.Blink = blinkToggle.Value;
    }

    private void OnFaceShapeKeyRangeConfigurationRefreshed(object sender, EventArgs e)
    {
        foreach (var (key, slider) in controls.Where(static kvp => kvp.Value is Slider).Select(static kvp => (kvp.Key, (Slider)kvp.Value)))
        {
            if (!shapeKeyRangeConfiguration.TryGetRange(key, out var range))
                range = new(0f, 1f);

            slider.Left = range.Lower;
            slider.Right = range.Upper;
        }
    }

    private void OnCharacterSelectionChanging(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        var face = e.Selected.Face;

        face.PropertyChanged -= OnFacePropertyChanged;
        face.ChangedShapeKey -= OnFaceBlendValueChanged;
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        var face = e.Selected?.Face;

        if (face is not null)
        {
            face.PropertyChanged += OnFacePropertyChanged;
            face.ChangedShapeKey += OnFaceBlendValueChanged;

            UpdateControls();
        }

        shapeKeysPane.ShapeKeyController = face;
    }

    private void OnFaceBlendValueChanged(object sender, KeyedPropertyChangeEventArgs<string> e)
    {
        if (!controls.TryGetValue(e.Key, out var control))
            return;

        var face = (FaceController)sender;

        if (control is Slider slider)
            slider.SetValueWithoutNotify(face[e.Key]);
        else if (control is Toggle toggle)
            toggle.SetEnabledWithoutNotify(Convert.ToBoolean(face[e.Key]));
    }

    private void OnFacePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var face = (FaceController)sender;

        if (e.PropertyName is nameof(FaceController.Blink))
            blinkToggle.SetEnabledWithoutNotify(face.Blink);
        else if (e.PropertyName is nameof(FaceController.BlendSet))
            UpdateControls();
    }

    private void UpdateControls()
    {
        if (CurrentFace is null)
            return;

        blinkToggle.SetEnabledWithoutNotify(CurrentFace.Blink);

        var hashKeyAndSliders = EyeHashes
            .Concat(MouthHashes)
            .Where(CurrentFace.ContainsShapeKey)
            .Select(hashKey => (hashKey, (Slider)controls[hashKey]));

        foreach (var (hashKey, slider) in hashKeyAndSliders)
            slider.SetValueWithoutNotify(CurrentFace[hashKey]);

        var hashKeyAndToggles = FaceHashes
            .Where(CurrentFace.ContainsShapeKey)
            .Select(hashKey => (hashKey, (Toggle)controls[hashKey]));

        foreach (var (hashKey, toggle) in hashKeyAndToggles)
            toggle.SetEnabledWithoutNotify(Convert.ToBoolean(CurrentFace[hashKey]));
    }
}

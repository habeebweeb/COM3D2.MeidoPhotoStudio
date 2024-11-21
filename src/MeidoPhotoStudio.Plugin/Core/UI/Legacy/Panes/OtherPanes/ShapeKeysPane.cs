using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class ShapeKeysPane : BasePane
{
    private readonly ShapeKeyConfiguration shapeKeyConfiguration;
    private readonly ShapeKeyRangeConfiguration shapeKeyRangeConfiguration;
    private readonly Dictionary<string, EventHandler> sliderEventHandlers = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Slider> sliders = new(StringComparer.Ordinal);
    private readonly HashSet<string> shapeKeySet = new(StringComparer.Ordinal);
    private readonly Toggle modifyShapeKeysToggle;
    private readonly Button refreshRangeButton;
    private readonly Toggle deleteShapeKeysToggle;
    private readonly Framework.UI.Legacy.ComboBox addShapeKeyComboBox;
    private readonly Button addShapeKeyButton;
    private readonly Label noShapeKeysLabel;
    private readonly LazyStyle deleteShapeKeyButtonStyle = new(13, static () => new(GUI.skin.button));
    private readonly LazyStyle shapeKeyLabelStyle = new(13, static () => new(GUI.skin.label));
    private readonly LazyStyle noShapeKeysLabelStyle = new(
        13,
        static () => new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
        });

    private IShapeKeyController shapeKeyController;
    private bool validShapeKey;
    private string[] shapeKeys;
    private bool hasShapeKeys;

    public ShapeKeysPane(ShapeKeyConfiguration shapeKeyConfiguration, ShapeKeyRangeConfiguration shapeKeyRangeConfiguration)
    {
        this.shapeKeyConfiguration = shapeKeyConfiguration ?? throw new ArgumentNullException(nameof(shapeKeyConfiguration));
        this.shapeKeyRangeConfiguration = shapeKeyRangeConfiguration ?? throw new ArgumentNullException(nameof(shapeKeyRangeConfiguration));

        this.shapeKeyRangeConfiguration.Refreshed += OnShapeKeyRangeConfigurationRefreshed;

        this.shapeKeyConfiguration.AddedShapeKey += OnShapeKeyAdded;
        this.shapeKeyConfiguration.RemovedShapeKey += OnShapeKeyRemoved;

        shapeKeys = [.. this.shapeKeyConfiguration.ShapeKeys];

        deleteShapeKeysToggle = new(Translation.Get("shapeKeysPane", "deleteShapeKeysToggle"));

        modifyShapeKeysToggle = new(Translation.Get("shapeKeysPane", "modifyShapeKeysToggle"));
        modifyShapeKeysToggle.ControlEvent += OnModifyShapeKeysToggleChanged;

        addShapeKeyComboBox = new(shapeKeys)
        {
            Placeholder = Translation.Get("shapeKeysPane", "searchShapeKeyPlaceholder"),
        };

        addShapeKeyComboBox.ChangedValue += OnAddShapeKeyComboBoxValueChanged;

        addShapeKeyButton = new(Translation.Get("shapeKeysPane", "addShapeKeyButton"));
        addShapeKeyButton.ControlEvent += OnAddShapeKeyButtonPushed;

        refreshRangeButton = new(Translation.Get("shapeKeysPane", "refreshShapeKeyRangeButton"));
        refreshRangeButton.ControlEvent += OnRefreshRangeButtonPushed;

        noShapeKeysLabel = new(Translation.Get("shapeKeysPane", "noShapeKeysLabel"));

        foreach (var hashKey in shapeKeys)
            _ = AddSlider(hashKey);
    }

    public bool DrawRefreshRangeButton { get; set; }

    public IShapeKeyController ShapeKeyController
    {
        get => shapeKeyController;
        set
        {
            if (value == shapeKeyController)
                return;

            if (shapeKeyController is not null)
            {
                shapeKeyController.ChangedShapeKey -= OnShapeKeyChanged;
                shapeKeyController.ChangedShapeKeySet -= OnShapeKeySetChanged;
            }

            shapeKeyController = value;

            if (shapeKeyController is not null)
            {
                shapeKeyController.ChangedShapeKey += OnShapeKeyChanged;
                shapeKeyController.ChangedShapeKeySet += OnShapeKeySetChanged;
            }

            UpdateShapeKeyList();
            UpdateControls();
        }
    }

    public override void Draw()
    {
        if (ShapeKeyController is null)
            return;

        if (!hasShapeKeys)
        {
            noShapeKeysLabel.Draw(noShapeKeysLabelStyle);

            return;
        }

        var guiEnabled = Parent.Enabled;

        if (DrawRefreshRangeButton)
        {
            GUILayout.BeginHorizontal();

            modifyShapeKeysToggle.Draw();

            refreshRangeButton.Draw(GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();
        }
        else
        {
            modifyShapeKeysToggle.Draw();
        }

        if (modifyShapeKeysToggle.Value)
        {
            UIUtility.DrawBlackLine();

            GUI.enabled = guiEnabled && hasShapeKeys && !deleteShapeKeysToggle.Value;

            DrawComboBox(addShapeKeyComboBox);

            GUILayout.BeginHorizontal();

            GUI.enabled = guiEnabled;

            deleteShapeKeysToggle.Draw();

            GUI.enabled = guiEnabled && hasShapeKeys && !deleteShapeKeysToggle.Value && validShapeKey;

            addShapeKeyButton.Draw(GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();
        }

        UIUtility.DrawBlackLine();

        if (deleteShapeKeysToggle.Value)
        {
            GUI.enabled = guiEnabled;

            var noExpandWidth = GUILayout.ExpandWidth(false);
            var maxWidth = GUILayout.MaxWidth(Parent.WindowRect.width);

            foreach (var shapeKey in shapeKeys.Where(ShapeKeyController.ContainsShapeKey))
            {
                GUILayout.BeginHorizontal(maxWidth);

                GUILayout.Label(shapeKey, shapeKeyLabelStyle);

                if (GUILayout.Button("X", deleteShapeKeyButtonStyle, noExpandWidth))
                    shapeKeyConfiguration.RemoveShapeKey(shapeKey);

                GUILayout.EndHorizontal();
            }
        }
        else
        {
            GUI.enabled = guiEnabled;

            const int SliderColumnCount = 2;

            var maxWidth = GUILayout.MaxWidth(Parent.WindowRect.width - 10f);
            var sliderWidth = GUILayout.MaxWidth(Parent.WindowRect.width / SliderColumnCount - 10f);

            foreach (var chunk in shapeKeys.Where(ShapeKeyController.ContainsShapeKey).Select(hash => sliders[hash]).Chunk(2))
            {
                GUILayout.BeginHorizontal(maxWidth);

                foreach (var slider in chunk)
                    slider.Draw(sliderWidth);

                GUILayout.EndHorizontal();
            }
        }
    }

    protected override void ReloadTranslation()
    {
        modifyShapeKeysToggle.Label = Translation.Get("shapeKeysPane", "modifyShapeKeysToggle");
        deleteShapeKeysToggle.Label = Translation.Get("shapeKeysPane", "deleteShapeKeysToggle");
        refreshRangeButton.Label = Translation.Get("shapeKeysPane", "refreshShapeKeyRangeButton");
        addShapeKeyComboBox.Placeholder = Translation.Get("shapeKeysPane", "searchShapeKeyPlaceholder");
        addShapeKeyButton.Label = Translation.Get("shapeKeysPane", "addShapeKeyButton");
        noShapeKeysLabel.Text = Translation.Get("shapeKeysPane", "noShapeKeysLabel");
    }

    private void OnRefreshRangeButtonPushed(object sender, EventArgs e) =>
        shapeKeyRangeConfiguration.Refresh();

    private void OnShapeKeyAdded(object sender, ShapeKeyConfigurationEventArgs e)
    {
        var slider = AddSlider(e.ChangedShapeKey);

        if (ShapeKeyController is var controller && controller.ContainsShapeKey(e.ChangedShapeKey))
            slider.SetValueWithoutNotify(controller[e.ChangedShapeKey]);

        UpdateShapeKeyList();

        shapeKeys = [.. shapeKeyConfiguration.ShapeKeys];
    }

    private void OnShapeKeyRemoved(object sender, ShapeKeyConfigurationEventArgs e)
    {
        RemoveSlider(e.ChangedShapeKey);

        if (ShapeKeyController is var controller && controller.ContainsShapeKey(e.ChangedShapeKey))
            controller[e.ChangedShapeKey] = 0f;

        UpdateShapeKeyList();

        shapeKeys = [.. shapeKeyConfiguration.ShapeKeys];
    }

    private void OnShapeKeyRangeConfigurationRefreshed(object sender, EventArgs e)
    {
        foreach (var (key, slider) in sliders)
        {
            if (!shapeKeyRangeConfiguration.TryGetRange(key, out var range))
                range = new(0f, 1f);

            slider.Left = range.Lower;
            slider.Right = range.Upper;
        }
    }

    private void OnShapeKeySetChanged(object sender, EventArgs e) =>
        UpdateShapeKeyList();

    private void OnShapeKeyChanged(object sender, KeyedPropertyChangeEventArgs<string> e)
    {
        if (!sliders.TryGetValue(e.Key, out var slider))
            return;

        var controller = (IShapeKeyController)sender;

        slider.SetValueWithoutNotify(controller[e.Key]);
    }

    private void OnAddShapeKeyComboBoxValueChanged(object sender, EventArgs e)
    {
        if (ShapeKeyController is null)
            return;

        validShapeKey = shapeKeySet.Contains(addShapeKeyComboBox.Value);
    }

    private void OnAddShapeKeyButtonPushed(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(addShapeKeyComboBox.Value))
            return;

        if (!shapeKeySet.Contains(addShapeKeyComboBox.Value))
            return;

        shapeKeyConfiguration.AddShapeKey(addShapeKeyComboBox.Value);

        addShapeKeyComboBox.Value = string.Empty;
    }

    private void OnModifyShapeKeysToggleChanged(object sender, EventArgs e) =>
        deleteShapeKeysToggle.Value = false;

    private Slider AddSlider(string hashKey)
    {
        if (sliders.TryGetValue(hashKey, out var slider))
            return slider;

        if (!shapeKeyRangeConfiguration.TryGetRange(hashKey, out var range))
            range = new(0f, 1f);

        slider = new Slider(hashKey, range.Lower, range.Upper);

        sliderEventHandlers[hashKey] = OnControlChanged(hashKey);
        slider.ControlEvent += sliderEventHandlers[hashKey];

        sliders[hashKey] = slider;

        return slider;

        EventHandler OnControlChanged(string hashKey) =>
            (object sender, EventArgs e) =>
            {
                if (ShapeKeyController is null)
                    return;

                var value = ((Slider)sender).Value;

                ShapeKeyController[hashKey] = value;
            };
    }

    private void RemoveSlider(string hashKey)
    {
        if (!sliders.ContainsKey(hashKey))
            return;

        sliders[hashKey].ControlEvent -= sliderEventHandlers[hashKey];
        sliderEventHandlers.Remove(hashKey);
        sliders.Remove(hashKey);
    }

    private void UpdateShapeKeyList()
    {
        if (ShapeKeyController is null)
            return;

        var shapeKeyList = ShapeKeyController.ShapeKeys
            .Except(shapeKeyConfiguration.BlockedShapeKeys)
            .Except(shapeKeyConfiguration.ShapeKeys)
            .ToArray();

        hasShapeKeys = shapeKeyList.Length is not 0;

        shapeKeySet.Clear();
        shapeKeySet.UnionWith(shapeKeyList);

        validShapeKey = shapeKeySet.Contains(addShapeKeyComboBox.Value);

        addShapeKeyComboBox.SetItems(shapeKeyList);
    }

    private void UpdateControls()
    {
        if (ShapeKeyController is not IShapeKeyController controller)
            return;

        var hashKeyAndSliders = shapeKeys
            .Where(controller.ContainsShapeKey)
            .Select(hashKey => (hashKey, sliders[hashKey]));

        foreach (var (hashKey, slider) in hashKeyAndSliders)
            slider.SetValueWithoutNotify(controller[hashKey]);
    }
}

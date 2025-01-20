using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Core.Scenes;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class AutoSaveSettingsPane : BasePane
{
    private readonly AutoSaveConfiguration configuration;
    private readonly AutoSaveService autoSaveService;
    private readonly Toggle enabledToggle;
    private readonly Header saveFrequencyHeader;
    private readonly Label saveFrequencyHintLabel;
    private readonly NumericalTextField frequencyField;
    private readonly Button saveFrequencyButton;
    private readonly Button resetFrequencyButton;
    private readonly Header slotsHeader;
    private readonly Label slotsHintLabel;
    private readonly NumericalTextField slotsField;
    private readonly Button saveSlotsButton;
    private readonly Button resetSlotsButton;

    private bool validFrequencyValue = true;
    private bool validSlotsValue = true;

    public AutoSaveSettingsPane(
        Translation translation, AutoSaveConfiguration configuration, AutoSaveService autoSaveService)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.autoSaveService = autoSaveService ?? throw new ArgumentNullException(nameof(autoSaveService));

        enabledToggle = new(
            new LocalizableGUIContent(translation, "autoSaveSettingsPane", "enabledToggle"),
            this.configuration.Enabled.Value);

        enabledToggle.ControlEvent += OnEnabledToggleChanged;

        saveFrequencyHeader = new(
            new LocalizableGUIContent(translation, "autoSaveSettingsPane", "saveFrequencyHeader"));

        saveFrequencyHintLabel = new(
            new LocalizableGUIContent(
                translation,
                "autoSaveSettingsPane",
                "saveFrequencyHint",
                translation => string.Format(translation, this.configuration.Frequency.MinimumValue())));

        frequencyField = new(this.configuration.Frequency.Value);
        frequencyField.ControlEvent += OnFrequencyFieldChanged;

        saveFrequencyButton = new(new LocalizableGUIContent(translation, "autoSaveSettingsPane", "saveButton"));
        saveFrequencyButton.ControlEvent += OnSaveFrequencyButtonPushed;

        resetFrequencyButton = new(new LocalizableGUIContent(translation, "autoSaveSettingsPane", "resetButton"));
        resetFrequencyButton.ControlEvent += OnResetFrequencyButtonPushed;

        slotsHeader = new(new LocalizableGUIContent(translation, "autoSaveSettingsPane", "slotsHeader"));

        slotsHintLabel = new(new LocalizableGUIContent(
            translation,
            "autoSaveSettingsPane",
            "slotsHint",
            translation => string.Format(translation, this.configuration.Slots.MinimumValue())));

        slotsField = new(this.configuration.Slots.Value);
        slotsField.ControlEvent += OnSlotsFieldChanged;

        saveSlotsButton = new(new LocalizableGUIContent(translation, "autoSaveSettingsPane", "saveButton"));
        saveSlotsButton.ControlEvent += OnSaveSlotsButtonPushed;

        resetSlotsButton = new(new LocalizableGUIContent(translation, "autoSaveSettingsPane", "resetButton"));
        resetSlotsButton.ControlEvent += OnResetSlotsButtonPushed;

        this.configuration.Enabled.SettingChanged += OnEnabledSettingChanged;
        this.configuration.Frequency.SettingChanged += OnFrequencySettingChanged;
        this.configuration.Slots.SettingChanged += OnSlotsSettingChanged;
    }

    public override void Draw()
    {
        enabledToggle.Draw();

        if (!enabledToggle.Value)
            return;

        UIUtility.DrawBlackLine();

        var guiEnabled = Parent.Enabled && enabledToggle.Value;

        GUI.enabled = guiEnabled;

        saveFrequencyHeader.Draw();
        saveFrequencyHintLabel.Draw();

        var noExpandWidth = GUILayout.ExpandWidth(false);

        GUILayout.BeginHorizontal();

        frequencyField.Draw();

        GUI.enabled = guiEnabled && validFrequencyValue && frequencyField.Value != configuration.Frequency.Value;

        saveFrequencyButton.Draw(noExpandWidth);

        GUI.enabled = guiEnabled && configuration.Frequency.Value != (int)configuration.Frequency.DefaultValue;

        resetFrequencyButton.Draw(noExpandWidth);

        GUILayout.EndHorizontal();

        UIUtility.DrawBlackLine();

        GUI.enabled = guiEnabled;

        slotsHeader.Draw();
        slotsHintLabel.Draw();

        GUILayout.BeginHorizontal();

        slotsField.Draw();

        GUI.enabled = guiEnabled && validSlotsValue && slotsField.Value != configuration.Slots.Value;

        saveSlotsButton.Draw(noExpandWidth);

        GUI.enabled = guiEnabled && configuration.Slots.Value != (int)configuration.Slots.DefaultValue;

        resetSlotsButton.Draw(noExpandWidth);

        GUILayout.EndHorizontal();
    }

    private void OnEnabledSettingChanged(object sender, EventArgs e)
    {
        enabledToggle.SetEnabledWithoutNotify(configuration.Enabled.Value);
        autoSaveService.Enabled = configuration.Enabled.Value;
    }

    private void OnFrequencySettingChanged(object sender, EventArgs e)
    {
        frequencyField.SetValueWithoutNotify(configuration.Frequency.Value);
        autoSaveService.AutoSaveInterval = configuration.Frequency.Value;
    }

    private void OnSlotsSettingChanged(object sender, EventArgs e)
    {
        slotsField.SetValueWithoutNotify(configuration.Slots.Value);
        autoSaveService.Slots = configuration.Slots.Value;
    }

    private void OnEnabledToggleChanged(object sender, EventArgs e) =>
        configuration.Enabled.Value = enabledToggle.Value;

    private void OnFrequencyFieldChanged(object sender, EventArgs e) =>
        validFrequencyValue = configuration.Frequency.IsValid((int)frequencyField.Value);

    private void OnSaveFrequencyButtonPushed(object sender, EventArgs e) =>
        configuration.Frequency.Value =
            (int)configuration.Frequency.Description.AcceptableValues.Clamp((int)frequencyField.Value);

    private void OnResetFrequencyButtonPushed(object sender, EventArgs e) =>
        configuration.Frequency.Value = (int)configuration.Frequency.DefaultValue;

    private void OnSlotsFieldChanged(object sender, EventArgs e) =>
        validSlotsValue = configuration.Slots.IsValid((int)slotsField.Value);

    private void OnSaveSlotsButtonPushed(object sender, EventArgs e) =>
        configuration.Slots.Value =
            (int)configuration.Slots.Description.AcceptableValues.Clamp((int)slotsField.Value);

    private void OnResetSlotsButtonPushed(object sender, EventArgs e) =>
        configuration.Slots.Value = (int)configuration.Slots.DefaultValue;
}

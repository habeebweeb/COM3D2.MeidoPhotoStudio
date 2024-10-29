using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Scenes;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class AutoSaveSettingsPane : BasePane
{
    private readonly AutoSaveConfiguration configuration;
    private readonly AutoSaveService autoSaveService;
    private readonly PaneHeader paneHeader;
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

    public AutoSaveSettingsPane(AutoSaveConfiguration configuration, AutoSaveService autoSaveService)
    {
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.autoSaveService = autoSaveService ?? throw new ArgumentNullException(nameof(autoSaveService));

        paneHeader = new(Translation.Get("autoSaveSettingsPane", "header"));

        enabledToggle = new(Translation.Get("autoSaveSettingsPane", "enabledToggle"), this.configuration.Enabled.Value);
        enabledToggle.ControlEvent += OnEnabledToggleChanged;

        saveFrequencyHeader = new(Translation.Get("autoSaveSettingsPane", "saveFrequencyHeader"));
        saveFrequencyHintLabel = new(string.Format(
            Translation.Get("autoSaveSettingsPane", "saveFrequencyHint"),
            this.configuration.Frequency.MinimumValue()));

        frequencyField = new(this.configuration.Frequency.Value);
        frequencyField.ControlEvent += OnFrequencyFieldChanged;

        saveFrequencyButton = new(Translation.Get("autoSaveSettingsPane", "saveButton"));
        saveFrequencyButton.ControlEvent += OnSaveFrequencyButtonPushed;

        resetFrequencyButton = new(Translation.Get("autoSaveSettingsPane", "resetButton"));
        resetFrequencyButton.ControlEvent += OnResetFrequencyButtonPushed;

        slotsHeader = new(Translation.Get("autoSaveSettingsPane", "slotsHeader"));
        slotsHintLabel = new(string.Format(
            Translation.Get("autoSaveSettingsPane", "slotsHint"),
            this.configuration.Slots.MinimumValue()));

        slotsField = new(this.configuration.Slots.Value);
        slotsField.ControlEvent += OnSlotsFieldChanged;

        saveSlotsButton = new(Translation.Get("autoSaveSettingsPane", "saveButton"));
        saveSlotsButton.ControlEvent += OnSaveSlotsButtonPushed;

        resetSlotsButton = new(Translation.Get("autoSaveSettingsPane", "resetButton"));
        resetSlotsButton.ControlEvent += OnResetSlotsButtonPushed;

        this.configuration.Enabled.SettingChanged += OnEnabledSettingChanged;
        this.configuration.Frequency.SettingChanged += OnFrequencySettingChanged;
        this.configuration.Slots.SettingChanged += OnSlotsSettingChanged;
    }

    public override void Draw()
    {
        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        enabledToggle.Draw();

        if (!enabledToggle.Value)
            return;

        MpsGui.BlackLine();

        var guiEnabled = enabledToggle.Value;

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

        MpsGui.BlackLine();

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

        GUI.enabled = true;
    }

    protected override void ReloadTranslation()
    {
        paneHeader.Label = Translation.Get("autoSaveSettingsPane", "header");
        enabledToggle.Label = Translation.Get("autoSaveSettingsPane", "enabledToggle");
        saveFrequencyHeader.Text = Translation.Get("autoSaveSettingsPane", "saveFrequencyHeader");
        saveFrequencyHintLabel.Text = string.Format(
            Translation.Get("autoSaveSettingsPane", "saveFrequencyHint"),
            configuration.Frequency.MinimumValue());

        saveFrequencyButton.Label = Translation.Get("autoSaveSettingsPane", "saveButton");
        resetFrequencyButton.Label = Translation.Get("autoSaveSettingsPane", "resetButton");

        slotsHeader.Text = Translation.Get("autoSaveSettingsPane", "slotsHeader");
        slotsHintLabel.Text = string.Format(
            Translation.Get("autoSaveSettingsPane", "slotsHint"),
            configuration.Slots.MinimumValue());

        saveSlotsButton.Label = Translation.Get("autoSaveSettingsPane", "saveButton");
        resetSlotsButton.Label = Translation.Get("autoSaveSettingsPane", "resetButton");
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

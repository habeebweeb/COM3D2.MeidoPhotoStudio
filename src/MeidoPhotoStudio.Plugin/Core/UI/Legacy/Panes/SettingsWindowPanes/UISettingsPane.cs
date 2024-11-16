using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class UISettingsPane : BasePane
{
    private readonly UIConfiguration configuration;
    private readonly MainWindow mainWindow;
    private readonly Header mainWindowWidthHeader;
    private readonly Label mainWindowWidthHintLabel;
    private readonly NumericalTextField mainWindowWidthField;
    private readonly Button saveMainWindowWidthButton;
    private readonly Button resetMainWindowWidthButton;

    private bool validMainWindowWidth;

    public UISettingsPane(UIConfiguration configuration, MainWindow mainWindow)
    {
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));

        mainWindowWidthHeader = new(Translation.Get("uiSettingsPane", "mainWindowWidthHeader"));
        mainWindowWidthHintLabel = new(
            string.Format(
                Translation.Get("uiSettingsPane", "mainWindowWidthHint"),
                MainWindow.MinimumWindowWidth));

        mainWindowWidthField = new(configuration.WindowWidth.Value);
        mainWindowWidthField.ControlEvent += OnMainWindowWidthFieldChanged;

        saveMainWindowWidthButton = new(Translation.Get("uiSettingsPane", "saveButton"));
        saveMainWindowWidthButton.ControlEvent += OnSaveMainWindowWidthButtonPushed;

        resetMainWindowWidthButton = new(Translation.Get("uiSettingsPane", "resetButton"));
        resetMainWindowWidthButton.ControlEvent += OnResetMainWindowWidthButtonButtonPushed;

        this.configuration.WindowWidth.SettingChanged += OnMinimumWindowWidthSettingChanged;
    }

    public override void Draw()
    {
        var noExpandWidth = GUILayout.ExpandWidth(false);

        mainWindowWidthHeader.Draw();
        mainWindowWidthHintLabel.Draw();

        GUILayout.BeginHorizontal();

        mainWindowWidthField.Draw();

        var guiEnabled = Parent.Enabled;

        GUI.enabled = guiEnabled && validMainWindowWidth && mainWindowWidthField.Value != configuration.WindowWidth.Value;

        saveMainWindowWidthButton.Draw(noExpandWidth);

        GUI.enabled = guiEnabled && configuration.WindowWidth.Value != (int)configuration.WindowWidth.DefaultValue;

        resetMainWindowWidthButton.Draw(noExpandWidth);

        GUILayout.EndHorizontal();
    }

    protected override void ReloadTranslation()
    {
        mainWindowWidthHeader.Text = Translation.Get("uiSettingsPane", "mainWindowWidthHeader");
        mainWindowWidthHintLabel.Text = string.Format(
            Translation.Get("uiSettingsPane", "mainWindowWidthHint"),
            MainWindow.MinimumWindowWidth);

        saveMainWindowWidthButton.Label = Translation.Get("uiSettingsPane", "saveButton");
        resetMainWindowWidthButton.Label = Translation.Get("uiSettingsPane", "resetButton");
    }

    private void OnMinimumWindowWidthSettingChanged(object sender, EventArgs e)
    {
        mainWindowWidthField.SetValueWithoutNotify(configuration.WindowWidth.Value);
        mainWindow.WindowWidth = configuration.WindowWidth.Value;
    }

    private void OnMainWindowWidthFieldChanged(object sender, EventArgs e) =>
        validMainWindowWidth = configuration.WindowWidth.IsValid((int)mainWindowWidthField.Value);

    private void OnSaveMainWindowWidthButtonPushed(object sender, EventArgs e) =>
        configuration.WindowWidth.Value = configuration.WindowWidth.Clamp((int)mainWindowWidthField.Value);

    private void OnResetMainWindowWidthButtonButtonPushed(object sender, EventArgs e) =>
        configuration.WindowWidth.Value = (int)configuration.WindowWidth.DefaultValue;
}

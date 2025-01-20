using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Localization;
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

    public UISettingsPane(Translation translation, UIConfiguration configuration, MainWindow mainWindow)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));

        mainWindowWidthHeader = new(new LocalizableGUIContent(translation, "uiSettingsPane", "mainWindowWidthHeader"));
        mainWindowWidthHintLabel = new(
            new LocalizableGUIContent(
                translation,
                "uiSettingsPane",
                "mainWindowWidthHint",
                static translation =>
                    string.Format(translation, MainWindow.MinimumWindowWidth)));

        mainWindowWidthField = new(configuration.WindowWidth.Value);
        mainWindowWidthField.ControlEvent += OnMainWindowWidthFieldChanged;

        saveMainWindowWidthButton = new(new LocalizableGUIContent(translation, "uiSettingsPane", "saveButton"));
        saveMainWindowWidthButton.ControlEvent += OnSaveMainWindowWidthButtonPushed;

        resetMainWindowWidthButton = new(new LocalizableGUIContent(translation, "uiSettingsPane", "resetButton"));
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

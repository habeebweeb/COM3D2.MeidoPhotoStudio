using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class TranslationSettingsPane : BasePane
{
    private readonly Button reloadTranslationButton;
    private readonly Toggle logMissingTranslationsToggle;
    private readonly TranslationConfiguration translationConfiguration;
    private readonly Translation translation;
    private readonly Label selectLanguageLabel;
    private readonly Dropdown<string> languagesDropdown;
    private readonly Button refreshAvailableTranslationsButton;

    public TranslationSettingsPane(TranslationConfiguration translationConfiguration, Translation translation)
    {
        this.translationConfiguration = translationConfiguration ?? throw new ArgumentNullException(nameof(translationConfiguration));
        this.translation = translation ?? throw new ArgumentNullException(nameof(translation));
        this.translation.RefreshedAvailableLanguages += OnAvailableLanguagesRefreshed;

        reloadTranslationButton = new(
            new LocalizableGUIContent(this.translation, "translationSettingsPane", "reloadTranslationButton"));

        reloadTranslationButton.ControlEvent += OnReloadTranslationButtonPushed;

        logMissingTranslationsToggle = new(
            new LocalizableGUIContent(this.translation, "translationSettingsPane", "logMissingTranslationsToggle"),
            translationConfiguration.LogMissingTranslations.Value);

        logMissingTranslationsToggle.ControlEvent += OnLogMissingTranslationsToggleChanged;

        selectLanguageLabel = new(
            new LocalizableGUIContent(this.translation, "translationSettingsPane", "selectedLanguageLabel"));

        var currentLanguageIndex = this.translation.AvailableLanguages.IndexOf(this.translation.CurrentLanguage);

        if (currentLanguageIndex is -1)
            currentLanguageIndex = 0;

        languagesDropdown = new(this.translation.AvailableLanguages, currentLanguageIndex);
        languagesDropdown.SelectionChanged += OnLanguageSelectionChanged;

        refreshAvailableTranslationsButton = new(
            new LocalizableGUIContent(this.translation, "translationSettingsPane", "refreshAvailableLanguagesButton"));

        refreshAvailableTranslationsButton.ControlEvent += OnRefreshAvailableTranslationsButtonPushed;
    }

    public override void Draw()
    {
        GUILayout.BeginHorizontal();

        selectLanguageLabel.Draw();

        GUI.enabled = Parent.Enabled && translation.AvailableLanguages.Count is not 0;

        languagesDropdown.Draw(GUILayout.Width(UIUtility.Scaled(135)));

        GUI.enabled = Parent.Enabled;

        refreshAvailableTranslationsButton.Draw(
            GUILayout.MinHeight(UIUtility.Scaled(StyleSheet.TextSize) + 12), GUILayout.ExpandWidth(false));

        GUILayout.EndHorizontal();

        logMissingTranslationsToggle.Draw();

        UIUtility.DrawBlackLine();

        reloadTranslationButton.Draw();
    }

    private void OnAvailableLanguagesRefreshed(object sender, EventArgs e)
    {
        var currentLanguageIndex = translation.AvailableLanguages
            .IndexOf(translationConfiguration.CurrentLanguage.Value, StringComparer.Ordinal);

        if (currentLanguageIndex is -1)
            currentLanguageIndex = 0;

        languagesDropdown.SetItemsWithoutNotify(translation.AvailableLanguages, currentLanguageIndex);
    }

    private void OnLanguageSelectionChanged(object sender, DropdownEventArgs<string> e)
    {
        translationConfiguration.CurrentLanguage.Value = languagesDropdown.SelectedItem;
        translation.SetLanguage(translationConfiguration.CurrentLanguage.Value);
    }

    private void OnReloadTranslationButtonPushed(object sender, EventArgs e) =>
        translation.RefreshCurrentTranslations();

    private void OnLogMissingTranslationsToggleChanged(object sender, EventArgs e) =>
        translation.LogMissingTranslations = translationConfiguration.LogMissingTranslations.Value = logMissingTranslationsToggle.Value;

    private void OnRefreshAvailableTranslationsButtonPushed(object sender, EventArgs e) =>
        translation.RefreshAvailableLanguages();
}

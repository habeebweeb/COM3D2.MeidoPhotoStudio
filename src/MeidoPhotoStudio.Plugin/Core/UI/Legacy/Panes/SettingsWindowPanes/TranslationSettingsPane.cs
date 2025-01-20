using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class TranslationSettingsPane : BasePane
{
    private readonly Button reloadTranslationButton;
    private readonly TranslationConfiguration translationConfiguration;
    private readonly Translation translation;

    public TranslationSettingsPane(TranslationConfiguration translationConfiguration, Translation translation)
    {
        this.translationConfiguration = translationConfiguration ?? throw new ArgumentNullException(nameof(translationConfiguration));
        this.translation = translation ?? throw new ArgumentNullException(nameof(translation));

        reloadTranslationButton = new(new LocalizableGUIContent(this.translation, "translationSettingsPane", "reloadTranslationButton"));
        reloadTranslationButton.ControlEvent += OnReloadTranslationButtonPushed;
    }

    public override void Draw() =>
        reloadTranslationButton.Draw();

    private void OnReloadTranslationButtonPushed(object sender, EventArgs e) =>
        translation.Refresh();
}

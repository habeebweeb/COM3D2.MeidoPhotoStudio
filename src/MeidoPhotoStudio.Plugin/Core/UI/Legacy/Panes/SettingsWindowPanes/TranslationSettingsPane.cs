using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class TranslationSettingsPane : BasePane
{
    private readonly Button reloadTranslationButton;

    public TranslationSettingsPane()
    {
        reloadTranslationButton = new(Translation.Get("translationSettingsPane", "reloadTranslationButton"));
        reloadTranslationButton.ControlEvent += OnReloadTranslationButtonPushed;
    }

    public override void Draw() =>
        reloadTranslationButton.Draw();

    protected override void ReloadTranslation() =>
        reloadTranslationButton.Label = Translation.Get("translationSettingsPane", "reloadTranslationButton");

    private void OnReloadTranslationButtonPushed(object sender, EventArgs e) =>
        Translation.ReinitializeTranslation();
}

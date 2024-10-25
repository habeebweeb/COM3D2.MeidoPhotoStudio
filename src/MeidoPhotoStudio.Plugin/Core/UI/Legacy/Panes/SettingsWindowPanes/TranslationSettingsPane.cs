using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class TranslationSettingsPane : BasePane
{
    private readonly PaneHeader paneHeader;
    private readonly Button reloadTranslationButton;

    public TranslationSettingsPane()
    {
        paneHeader = new(Translation.Get("translationSettingsPane", "header"));

        reloadTranslationButton = new(Translation.Get("translationSettingsPane", "reloadTranslationButton"));
        reloadTranslationButton.ControlEvent += OnReloadTranslationButtonPushed;
    }

    public override void Draw()
    {
        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        reloadTranslationButton.Draw();
    }

    protected override void ReloadTranslation()
    {
        paneHeader.Label = Translation.Get("translationSettingsPane", "header");
        reloadTranslationButton.Label = Translation.Get("translationSettingsPane", "reloadTranslationButton");
    }

    private void OnReloadTranslationButtonPushed(object sender, EventArgs e) =>
        Translation.ReinitializeTranslation();
}

using MeidoPhotoStudio.Plugin.Core.Effects;
using MeidoPhotoStudio.Plugin.Core.Localization;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class SepiaTonePane(Translation translation, SepiaToneController effectController)
    : EffectPane<SepiaToneController>(translation, effectController)
{
    public override void Draw() =>
        effectActiveToggle.Draw();
}

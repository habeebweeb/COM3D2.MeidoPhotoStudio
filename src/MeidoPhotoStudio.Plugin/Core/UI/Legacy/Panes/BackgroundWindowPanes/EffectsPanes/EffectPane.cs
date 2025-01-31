using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Effects;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class EffectPane<T> : BasePane
    where T : EffectControllerBase
{
    protected readonly Toggle effectActiveToggle;
    protected readonly Button resetEffectButton;

    public EffectPane(Translation translation, T effectController)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        Effect = effectController ?? throw new ArgumentNullException(nameof(effectController));

        effectActiveToggle = new(new LocalizableGUIContent(translation, "effectsPane", "onToggle"));
        effectActiveToggle.ControlEvent += OnEffectActiveToggleChanged;

        resetEffectButton = new(new LocalizableGUIContent(translation, "effectsPane", "reset"));
        resetEffectButton.ControlEvent += OnResetEffectButtonPushed;

        Effect.PropertyChanged += OnEffectPropertyChanged;
    }

    protected T Effect { get; }

    public override void Draw()
    {
        var guiEnabled = Parent.Enabled;

        GUILayout.BeginHorizontal();

        effectActiveToggle.Draw();

        GUI.enabled = guiEnabled && Effect.Active;

        resetEffectButton.Draw(GUILayout.ExpandWidth(false));

        GUILayout.EndHorizontal();
    }

    protected virtual void OnEffectPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(EffectControllerBase.Active))
            effectActiveToggle.SetEnabledWithoutNotify(((EffectControllerBase)sender).Active);
    }

    private void OnEffectActiveToggleChanged(object sender, EventArgs e) =>
        Effect.Active = ((Toggle)sender).Value;

    private void OnResetEffectButtonPushed(object sender, EventArgs e) =>
        Effect.Reset();
}

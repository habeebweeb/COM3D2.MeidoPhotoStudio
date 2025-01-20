using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Effects;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class BlurPane : EffectPane<BlurController>
{
    private readonly Slider blurSizeSlider;
    private readonly Slider blurIterationsSlider;
    private readonly Slider downsampleSlider;

    public BlurPane(Translation translation, BlurController effectController)
        : base(translation, effectController)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        blurSizeSlider = new(new LocalizableGUIContent(translation, "effectBlur", "blurSize"), 0f, 20f, Effect.BlurSize)
        {
            HasTextField = true,
        };

        blurSizeSlider.ControlEvent += OnBlurSizeChanged;

        blurIterationsSlider = new(
            new LocalizableGUIContent(translation, "effectBlur", "blurIterations"), 0f, 20f, Effect.BlurIterations)
        {
            HasTextField = true,
        };

        blurIterationsSlider.ControlEvent += OnBlurIterationsChanged;

        downsampleSlider = new(
            new LocalizableGUIContent(translation, "effectBlur", "downsample"), 0f, 10f, Effect.Downsample)
        {
            HasTextField = true,
        };

        downsampleSlider.ControlEvent += OnDownsampleChanged;
    }

    public override void Draw()
    {
        base.Draw();

        blurSizeSlider.Draw();
        blurIterationsSlider.Draw();
        downsampleSlider.Draw();
    }

    protected override void OnEffectPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        base.OnEffectPropertyChanged(sender, e);

        var blur = (BlurController)sender;

        if (e.PropertyName is nameof(BlurController.BlurSize))
            blurSizeSlider.SetValueWithoutNotify(blur.BlurSize);
        else if (e.PropertyName is nameof(BlurController.BlurIterations))
            blurIterationsSlider.SetValueWithoutNotify(blur.BlurIterations);
        else if (e.PropertyName is nameof(BlurController.Downsample))
            downsampleSlider.SetValueWithoutNotify(blur.Downsample);
    }

    private void OnBlurSizeChanged(object sender, EventArgs e) =>
        Effect.BlurSize = ((Slider)sender).Value;

    private void OnBlurIterationsChanged(object sender, EventArgs e) =>
        Effect.BlurIterations = (int)((Slider)sender).Value;

    private void OnDownsampleChanged(object sender, EventArgs e) =>
        Effect.Downsample = (int)((Slider)sender).Value;
}

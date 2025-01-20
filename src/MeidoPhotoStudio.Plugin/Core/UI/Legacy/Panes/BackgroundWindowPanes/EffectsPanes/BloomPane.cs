using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Effects;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class BloomPane : EffectPane<BloomController>
{
    private readonly Slider intensitySlider;
    private readonly Slider blurSlider;
    private readonly Slider redSlider;
    private readonly Slider greenSlider;
    private readonly Slider blueSlider;
    private readonly Toggle hdrToggle;

    public BloomPane(Translation translation, BloomController effectController)
        : base(translation, effectController)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        intensitySlider = new(
            new LocalizableGUIContent(translation, "effectBloom", "intensity"),
            0f,
            100f,
            Effect.BloomValue,
            Effect.BloomValue)
        {
            HasTextField = true,
            HasReset = true,
        };

        intensitySlider.ControlEvent += OnItensitySliderChanged;

        blurSlider = new(
            new LocalizableGUIContent(translation, "effectBloom", "blur"),
            0f,
            15f,
            Effect.BlurIterations,
            Effect.BlurIterations)
        {
            HasTextField = true,
            HasReset = true,
        };

        blurSlider.ControlEvent += OnBlurSliderChanged;

        var bloomThresholdColour = Effect.BloomThresholdColour;

        redSlider = new(
            new LocalizableGUIContent(translation, "effectBloom", "red"),
            1f,
            0f,
            bloomThresholdColour.r,
            bloomThresholdColour.r)
        {
            HasTextField = true,
            HasReset = true,
        };

        redSlider.ControlEvent += OnColourSliderChanged;

        greenSlider = new(
            new LocalizableGUIContent(translation, "effectBloom", "green"),
            1f,
            0f,
            bloomThresholdColour.g,
            bloomThresholdColour.g)
        {
            HasTextField = true,
            HasReset = true,
        };

        greenSlider.ControlEvent += OnColourSliderChanged;

        blueSlider = new(
            new LocalizableGUIContent(translation, "effectBloom", "blue"),
            1f,
            0f,
            bloomThresholdColour.b,
            bloomThresholdColour.b)
        {
            HasTextField = true,
            HasReset = true,
        };

        blueSlider.ControlEvent += OnColourSliderChanged;

        hdrToggle = new(new LocalizableGUIContent(translation, "effectBloom", "hdrToggle"), Effect.HDR);
        hdrToggle.ControlEvent += OnHDRToggleChanged;
    }

    public override void Draw()
    {
        base.Draw();

        intensitySlider.Draw();
        blurSlider.Draw();
        redSlider.Draw();
        greenSlider.Draw();
        blueSlider.Draw();
        hdrToggle.Draw();
    }

    protected override void OnEffectPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        base.OnEffectPropertyChanged(sender, e);

        var bloom = (BloomController)sender;

        if (e.PropertyName is nameof(BloomController.BloomValue))
        {
            intensitySlider.SetValueWithoutNotify(bloom.BloomValue);
        }
        else if (e.PropertyName is nameof(BloomController.BlurIterations))
        {
            blurSlider.SetValueWithoutNotify(bloom.BlurIterations);
        }
        else if (e.PropertyName is nameof(BloomController.BloomThresholdColour))
        {
            redSlider.SetValueWithoutNotify(bloom.BloomThresholdColour.r);
            greenSlider.SetValueWithoutNotify(bloom.BloomThresholdColour.g);
            blueSlider.SetValueWithoutNotify(bloom.BloomThresholdColour.b);
        }
        else if (e.PropertyName is nameof(BloomController.HDR))
        {
            hdrToggle.SetEnabledWithoutNotify(bloom.HDR);
        }
    }

    private void OnItensitySliderChanged(object sender, EventArgs e) =>
        Effect.BloomValue = (int)((Slider)sender).Value;

    private void OnBlurSliderChanged(object sender, EventArgs e) =>
        Effect.BlurIterations = (int)((Slider)sender).Value;

    private void OnColourSliderChanged(object sender, EventArgs e) =>
        Effect.BloomThresholdColour = new(redSlider.Value, blueSlider.Value, greenSlider.Value);

    private void OnHDRToggleChanged(object sender, EventArgs e) =>
        Effect.HDR = ((Toggle)sender).Value;
}

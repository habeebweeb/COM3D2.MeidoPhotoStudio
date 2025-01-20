using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Effects;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class VignettePane : EffectPane<VignetteController>
{
    private readonly Slider intensitySlider;
    private readonly Slider blurSlider;
    private readonly Slider blurSpreadSlider;
    private readonly Slider chromaticAberrationSlider;

    public VignettePane(Translation translation, VignetteController effectController)
        : base(translation, effectController)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        intensitySlider = new(
            new LocalizableGUIContent(translation, "effectVignette", "intensity"),
            -40f,
            70f,
            Effect.Intensity,
            Effect.Intensity)
        {
            HasTextField = true,
            HasReset = true,
        };

        intensitySlider.ControlEvent += OnItensitySliderChanged;

        blurSlider = new(
            new LocalizableGUIContent(translation, "effectVignette", "blur"), 0f, 5f, Effect.Blur, Effect.Blur)
        {
            HasTextField = true,
            HasReset = true,
        };

        blurSlider.ControlEvent += OnBlurSliderChanged;

        blurSpreadSlider = new(
            new LocalizableGUIContent(translation, "effectVignette", "blurSpread"),
            0,
            40f,
            Effect.BlurSpread,
            Effect.BlurSpread)
        {
            HasTextField = true,
            HasReset = true,
        };

        blurSpreadSlider.ControlEvent += OnBlurSpreadSliderChanged;

        chromaticAberrationSlider = new(
            new LocalizableGUIContent(translation, "effectVignette", "chromaticAberration"),
            -50f,
            50f,
            Effect.ChromaticAberration,
            Effect.ChromaticAberration)
        {
            HasTextField = true,
            HasReset = true,
        };

        chromaticAberrationSlider.ControlEvent += OnAberrationSliderChanged;
    }

    public override void Draw()
    {
        base.Draw();

        intensitySlider.Draw();
        blurSlider.Draw();
        blurSpreadSlider.Draw();
        chromaticAberrationSlider.Draw();
    }

    protected override void OnEffectPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        base.OnEffectPropertyChanged(sender, e);

        var vignette = (VignetteController)sender;

        if (e.PropertyName is nameof(VignetteController.Intensity))
            intensitySlider.SetValueWithoutNotify(vignette.Intensity);
        else if (e.PropertyName is nameof(VignetteController.Blur))
            blurSlider.SetValueWithoutNotify(vignette.Blur);
        else if (e.PropertyName is nameof(VignetteController.BlurSpread))
            blurSpreadSlider.SetValueWithoutNotify(vignette.BlurSpread);
        else if (e.PropertyName is nameof(VignetteController.Intensity))
            chromaticAberrationSlider.SetValueWithoutNotify(vignette.Intensity);
    }

    private void OnItensitySliderChanged(object sender, EventArgs e) =>
        Effect.Intensity = ((Slider)sender).Value;

    private void OnBlurSliderChanged(object sender, EventArgs e) =>
        Effect.Blur = ((Slider)sender).Value;

    private void OnBlurSpreadSliderChanged(object sender, EventArgs e) =>
        Effect.BlurSpread = ((Slider)sender).Value;

    private void OnAberrationSliderChanged(object sender, EventArgs e) =>
        Effect.ChromaticAberration = ((Slider)sender).Value;
}

using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class DepthOfFieldPane : EffectPane<DepthOfFieldEffectManager>
    {
        protected override DepthOfFieldEffectManager EffectManager { get; set; }
        private readonly Slider focalLengthSlider;
        private readonly Slider focalSizeSlider;
        private readonly Slider apertureSlider;
        private readonly Slider blurSlider;
        private readonly Toggle thicknessToggle;

        public DepthOfFieldPane(EffectManager effectManager) : base(effectManager.Get<DepthOfFieldEffectManager>())
        {
            focalLengthSlider = new Slider(Translation.Get("effectDof", "focalLength"), 0f, 10f);
            focalSizeSlider = new Slider(Translation.Get("effectDof", "focalArea"), 0f, 2f);
            apertureSlider = new Slider(Translation.Get("effectDof", "aperture"), 0f, 60f);
            blurSlider = new Slider(Translation.Get("effectDof", "blur"), 0f, 10f);
            thicknessToggle = new Toggle(Translation.Get("effectDof", "thicknessToggle"));
            focalLengthSlider.ControlEvent += (s, a) => EffectManager.FocalLength = focalLengthSlider.Value;
            focalSizeSlider.ControlEvent += (s, a) => EffectManager.FocalSize = focalSizeSlider.Value;
            apertureSlider.ControlEvent += (s, a) => EffectManager.Aperture = apertureSlider.Value;
            blurSlider.ControlEvent += (s, a) => EffectManager.MaxBlurSize = blurSlider.Value;
            thicknessToggle.ControlEvent += (s, a) => EffectManager.VisualizeFocus = thicknessToggle.Value;
        }

        protected override void TranslatePane()
        {
            focalLengthSlider.Label = Translation.Get("effectDof", "focalLength");
            focalSizeSlider.Label = Translation.Get("effectDof", "focalArea");
            apertureSlider.Label = Translation.Get("effectDof", "aperture");
            blurSlider.Label = Translation.Get("effectDof", "blur");
            thicknessToggle.Label = Translation.Get("effectDof", "thicknessToggle");
        }

        protected override void UpdateControls()
        {
            focalLengthSlider.Value = EffectManager.FocalLength;
            focalSizeSlider.Value = EffectManager.FocalSize;
            apertureSlider.Value = EffectManager.Aperture;
            blurSlider.Value = EffectManager.MaxBlurSize;
            thicknessToggle.Value = EffectManager.VisualizeFocus;
        }

        protected override void DrawPane()
        {
            focalLengthSlider.Draw();

            GUILayoutOption sliderWidth = MiscGUI.HalfSlider;

            GUILayout.BeginHorizontal();
            focalSizeSlider.Draw(sliderWidth);
            apertureSlider.Draw(sliderWidth);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            blurSlider.Draw(sliderWidth);
            GUILayout.FlexibleSpace();
            thicknessToggle.Draw();
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }
    }
}

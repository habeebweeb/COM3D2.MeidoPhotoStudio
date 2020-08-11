using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using static Meido;
    internal class MaidFaceSliderPane : BasePane
    {
        // TODO: Consider placing in external file to be user editable
        private static readonly Dictionary<string, float[]> SliderRange = new Dictionary<string, float[]>()
        {
            // Eye Shut
            ["eyeclose"] = new[] { 0f, 1f },
            // Eye Smile
            ["eyeclose2"] = new[] { 0f, 1f },
            // Glare
            ["eyeclose3"] = new[] { 0f, 1f },
            // Wide Eyes
            ["eyebig"] = new[] { 0f, 1f },
            // Wink 1
            ["eyeclose6"] = new[] { 0f, 1f },
            // Wink 2
            ["eyeclose5"] = new[] { 0f, 1f },
            // Highlight
            ["hitomih"] = new[] { 0f, 2f },
            // Pupil Size
            ["hitomis"] = new[] { 0f, 3f },
            // Brow 1
            ["mayuha"] = new[] { 0f, 1f },
            // Brow 2
            ["mayuw"] = new[] { 0f, 1f },
            // Brow Up
            ["mayuup"] = new[] { 0f, 0.8f },
            // Brow Down 1
            ["mayuv"] = new[] { 0f, 0.8f },
            // Brow Down 2
            ["mayuvhalf"] = new[] { 0f, 0.9f },
            // Mouth Open 1
            ["moutha"] = new[] { 0f, 1f },
            // Mouth Open 2
            ["mouths"] = new[] { 0f, 0.9f },
            // Mouth Narrow
            ["mouthc"] = new[] { 0f, 1f },
            // Mouth Widen
            ["mouthi"] = new[] { 0f, 1f },
            // Smile
            ["mouthup"] = new[] { 0f, 1.4f },
            // Frown
            ["mouthdw"] = new[] { 0f, 1f },
            // Mouth Pucker
            ["mouthhe"] = new[] { 0f, 1f },
            // Grin
            ["mouthuphalf"] = new[] { 0f, 2f },
            // Tongue Out
            ["tangout"] = new[] { 0f, 1f },
            // Tongue Up
            ["tangup"] = new[] { 0f, 0.7f },
            // Tongue Base
            ["tangopen"] = new[] { 0f, 1f }
        };
        private MeidoManager meidoManager;
        private Dictionary<string, BaseControl> faceControls;

        public MaidFaceSliderPane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;
            this.faceControls = new Dictionary<string, BaseControl>();

            foreach (string key in faceKeys)
            {
                string uiName = Translation.Get("faceBlendValues", key);
                Slider slider = new Slider(uiName, SliderRange[key][0], SliderRange[key][1]);
                string myKey = key;
                slider.ControlEvent += (s, a) => this.SetFaceValue(myKey, slider.Value);
                faceControls[key] = slider;
            }

            foreach (string key in faceToggleKeys)
            {
                string uiName = Translation.Get("faceBlendValues", key);
                Toggle toggle = new Toggle(uiName);
                string myKey = key;
                toggle.ControlEvent += (s, a) => this.SetFaceValue(myKey, toggle.Value);
                faceControls[key] = toggle;
            }
        }

        protected override void ReloadTranslation()
        {
            for (int i = 0; i < faceKeys.Length; i++)
            {
                Slider slider = (Slider)faceControls[faceKeys[i]];
                slider.Label = Translation.Get("faceBlendValues", faceKeys[i]);
            }

            for (int i = 0; i < faceToggleKeys.Length; i++)
            {
                Toggle toggle = (Toggle)faceControls[faceToggleKeys[i]];
                toggle.Label = Translation.Get("faceBlendValues", faceToggleKeys[i]);
            }
        }

        public override void UpdatePane()
        {
            this.updating = true;
            TMorph morph = this.meidoManager.ActiveMeido.Maid.body0.Face.morph;
            bool gp01FBFace = morph.bodyskin.PartsVersion >= 120;
            float[] blendValues = this.meidoManager.ActiveMeido.BlendValues;
            float[] blendValuesBackup = this.meidoManager.ActiveMeido.BlendValuesBackup;
            for (int i = 0; i < faceKeys.Length; i++)
            {
                string hash = faceKeys[i];
                Slider slider = (Slider)faceControls[hash];
                try
                {
                    hash = Utility.GP01FbFaceHash(morph, hash);
                    if (hash.StartsWith("eyeclose") && !(gp01FBFace && (hash == "eyeclose3")))
                        slider.Value = blendValuesBackup[(int)morph.hash[hash]];
                    else
                        slider.Value = blendValues[(int)morph.hash[hash]];
                }
                catch { }
            }

            for (int i = 0; i < faceToggleKeys.Length; i++)
            {
                string hash = faceToggleKeys[i];
                Toggle toggle = (Toggle)faceControls[hash];
                if (hash == "nosefook") toggle.Value = morph.boNoseFook;
                else toggle.Value = blendValues[(int)morph.hash[hash]] > 0f;
                if (hash == "toothoff") toggle.Value = !toggle.Value;
            }
            this.updating = false;
        }

        public override void Draw()
        {
            GUI.enabled = this.meidoManager.HasActiveMeido;
            DrawSliders("eyeclose", "eyeclose2");
            DrawSliders("eyeclose3", "eyebig");
            DrawSliders("eyeclose6", "eyeclose5");
            DrawSliders("hitomih", "hitomis");
            DrawSliders("mayuha", "mayuw");
            DrawSliders("mayuup", "mayuv");
            DrawSliders("mayuvhalf");
            DrawSliders("moutha", "mouths");
            DrawSliders("mouthc", "mouthi");
            DrawSliders("mouthup", "mouthdw");
            DrawSliders("mouthhe", "mouthuphalf");
            DrawSliders("tangout", "tangup");
            DrawSliders("tangopen");
            MiscGUI.WhiteLine();
            DrawToggles("hoho2", "shock", "nosefook");
            DrawToggles("namida", "yodare", "toothoff");
            DrawToggles("tear1", "tear2", "tear3");
            DrawToggles("hohos", "hoho", "hohol");
            GUI.enabled = true;
        }

        private void DrawSliders(params string[] keys)
        {
            GUILayout.BeginHorizontal();
            foreach (string key in keys)
            {
                ((Slider)faceControls[key]).Draw(MiscGUI.HalfSlider);
            }
            GUILayout.EndHorizontal();
        }

        private void DrawToggles(params string[] keys)
        {
            GUILayout.BeginHorizontal();
            foreach (string key in keys)
            {
                ((Toggle)faceControls[key]).Draw();
            }
            GUILayout.EndHorizontal();
        }

        private void SetFaceValue(string key, float value)
        {
            if (updating) return;
            this.meidoManager.ActiveMeido.SetFaceBlendValue(key, value);
        }

        private void SetFaceValue(string key, bool value)
        {
            float max = (key == "hoho" || key == "hoho2") ? 0.5f : 1f;
            if (key == "toothoff") value = !value;
            SetFaceValue(key, value ? max : 0f);
        }
    }
}

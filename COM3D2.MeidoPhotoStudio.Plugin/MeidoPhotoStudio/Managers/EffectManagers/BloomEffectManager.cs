using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class BloomEffectManager : IEffectManager
    {
        public const string header = "EFFECT_BLOOM";
        private Bloom Bloom { get; set; }
        private float initialIntensity;
        private int initialBlurIterations;
        private Color initialThresholdColour;
        private Bloom.HDRBloomMode initialHDRBloomMode;
        public bool Ready { get; private set; }
        public bool Active { get; private set; }
        public float Intensity { get; set; }
        private int blurIterations;
        public int BlurIterations
        {
            get => blurIterations;
            set => blurIterations = Bloom.bloomBlurIterations = value;
        }
        public float BloomThresholdColorRed
        {
            get => BloomThresholdColour.r;
            set
            {
                Color colour = Bloom.bloomThreshholdColor;
                BloomThresholdColour = new Color(value, colour.g, colour.b);
            }
        }
        public float BloomThresholdColorGreen
        {
            get => BloomThresholdColour.g;
            set
            {
                Color colour = Bloom.bloomThreshholdColor;
                BloomThresholdColour = new Color(colour.r, value, colour.b);
            }
        }
        public float BloomThresholdColorBlue
        {
            get => BloomThresholdColour.b;
            set
            {
                Color colour = Bloom.bloomThreshholdColor;
                BloomThresholdColour = new Color(colour.r, colour.g, value);
            }
        }
        private Color bloomThresholdColour;
        public Color BloomThresholdColour
        {
            get => bloomThresholdColour;
            set => bloomThresholdColour = Bloom.bloomThreshholdColor = value;
        }
        private bool HDRBloomMode;
        public bool BloomHDR
        {
            get => HDRBloomMode;
            set
            {
                Bloom.hdr = value ? Bloom.HDRBloomMode.On : Bloom.HDRBloomMode.Auto;
                HDRBloomMode = value;
            }
        }

        public void Serialize(System.IO.BinaryWriter binaryWriter)
        {
            binaryWriter.Write(header);
            binaryWriter.Write(Intensity);
            binaryWriter.Write(BlurIterations);
            binaryWriter.WriteColour(BloomThresholdColour);
            binaryWriter.Write(BloomHDR);
            binaryWriter.Write(Active);
        }

        public void Deserialize(System.IO.BinaryReader binaryReader)
        {
            Intensity = binaryReader.ReadSingle();
            BlurIterations = binaryReader.ReadInt32();
            BloomThresholdColour = binaryReader.ReadColour();
            BloomHDR = binaryReader.ReadBoolean();
            SetEffectActive(binaryReader.ReadBoolean());
        }

        public void Activate()
        {
            if (Bloom == null)
            {
                Ready = true;
                Bloom = Utility.GetFieldValue<CameraMain, Bloom>(GameMain.Instance.MainCamera, "m_gcBloom");
                initialIntensity = Intensity = Bloom.bloomIntensity;
                initialBlurIterations = BlurIterations = Bloom.bloomBlurIterations;
                initialThresholdColour = BloomThresholdColour = Bloom.bloomThreshholdColor;
                initialHDRBloomMode = Bloom.hdr;
                BloomHDR = initialHDRBloomMode == Bloom.HDRBloomMode.On;
            }
        }

        public void Deactivate()
        {
            Intensity = initialIntensity;
            BlurIterations = initialBlurIterations;
            BloomThresholdColour = initialThresholdColour;
            BloomHDR = initialHDRBloomMode == Bloom.HDRBloomMode.On;
            BloomHDR = false;
            Bloom.enabled = true;
            Active = false;
        }

        public void Reset()
        {
            Bloom.bloomIntensity = initialIntensity;
            Bloom.bloomBlurIterations = initialBlurIterations;
            Bloom.bloomThreshholdColor = initialThresholdColour;
            Bloom.hdr = initialHDRBloomMode;
        }

        public void SetEffectActive(bool active)
        {
            Bloom.enabled = active;
            if (Active = active)
            {
                Bloom.bloomIntensity = Intensity;
                Bloom.bloomBlurIterations = BlurIterations;
                Bloom.bloomThreshholdColor = BloomThresholdColour;
                Bloom.hdr = BloomHDR ? Bloom.HDRBloomMode.On : Bloom.HDRBloomMode.Auto;
            }
            else Reset();
        }

        public void Update()
        {
            if (Active)
            {
                // Fuck this stupid shit
                // 2020/08/15 this stupid shit doesn't even work anymore
                // TODO: Fix this stupid shit
                Bloom.enabled = true;
                Bloom.bloomIntensity = Intensity;
            }
        }
    }
}

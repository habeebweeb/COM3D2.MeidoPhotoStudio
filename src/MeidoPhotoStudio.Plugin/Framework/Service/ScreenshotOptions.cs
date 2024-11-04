namespace MeidoPhotoStudio.Plugin.Framework.Service;

public readonly record struct ScreenshotOptions(bool CaptureMessageBox, bool CaptureUI, bool PlaySound = true)
{
    public ScreenshotOptions()
        : this(true, false, true)
    {
    }
}

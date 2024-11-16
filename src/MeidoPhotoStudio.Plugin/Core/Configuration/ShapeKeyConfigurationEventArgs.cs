namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class ShapeKeyConfigurationEventArgs(string changedShapeKey) : EventArgs
{
    public string ChangedShapeKey { get; } = changedShapeKey ?? throw new ArgumentNullException(nameof(changedShapeKey));
}

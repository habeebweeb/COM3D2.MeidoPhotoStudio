namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class ShapeKeyRangeConfigurationEventArgs(string changedShapeKey, ShapeKeyRange range) : EventArgs
{
    public string ChangedShapeKey { get; } = changedShapeKey ?? throw new ArgumentNullException(nameof(changedShapeKey));

    public ShapeKeyRange Range { get; } = range;
}

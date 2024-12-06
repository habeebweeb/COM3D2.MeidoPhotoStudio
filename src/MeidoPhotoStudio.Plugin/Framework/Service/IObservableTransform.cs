namespace MeidoPhotoStudio.Plugin.Framework.Service;

public interface IObservableTransform
{
    event EventHandler<TransformChangeEventArgs> ChangedTransform;

    TransformBackup InitialTransform { get; }

    Transform Transform { get; }
}

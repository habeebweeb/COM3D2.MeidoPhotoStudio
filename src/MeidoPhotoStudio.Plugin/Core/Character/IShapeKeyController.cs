using MeidoPhotoStudio.Plugin.Framework;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public interface IShapeKeyController
{
    event EventHandler<KeyedPropertyChangeEventArgs<string>> ChangedShapeKey;

    public IEnumerable<string> ShapeKeys { get; }

    float this[string shapeKey] { get; set; }

    bool ContainsShapeKey(string key);
}

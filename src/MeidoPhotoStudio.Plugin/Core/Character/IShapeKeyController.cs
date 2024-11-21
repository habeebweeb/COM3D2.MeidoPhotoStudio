using MeidoPhotoStudio.Plugin.Framework;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public interface IShapeKeyController
{
    event EventHandler<KeyedPropertyChangeEventArgs<string>> ChangedShapeKey;

    event EventHandler ChangedShapeKeySet;

    public IEnumerable<string> ShapeKeys { get; }

    float this[string shapeKey] { get; set; }

    bool ContainsShapeKey(string key);
}

using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Core.Schema.Props;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class PropShapeKeySchemaBuilder : ISchemaBuilder<PropShapeKeySchema, ShapeKeyController>
{
    public PropShapeKeySchema Build(ShapeKeyController value) =>
        new()
        {
            BlendValues = value is null ? [] : value.ToDictionary(static kvp => kvp.HashKey, static kvp => kvp.BlendValue),
        };
}

namespace MeidoPhotoStudio.Plugin.Core.Schema.Character;

public class BodySchema(short version = BodySchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public Dictionary<string, float> BodyShapeKeySet { get; init; }
}

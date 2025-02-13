namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class ShapeKeyRangeConfiguration(IShapeKeyRangeSerializer shapeKeyRangeSerializer)
{
    private readonly IShapeKeyRangeSerializer shapeKeyRangeSerializer = shapeKeyRangeSerializer
        ?? throw new ArgumentException(nameof(shapeKeyRangeSerializer));

    private Dictionary<string, ShapeKeyRange> ranges;

    public event EventHandler Refreshed;

    public event EventHandler<ShapeKeyRangeConfigurationEventArgs> ChangedRange;

    public event EventHandler<ShapeKeyRangeConfigurationEventArgs> AddedRange;

    public event EventHandler<ShapeKeyRangeConfigurationEventArgs> RemovedRange;

    private Dictionary<string, ShapeKeyRange> Ranges =>
        ranges ??= Initialize(shapeKeyRangeSerializer);

    public ShapeKeyRange this[string shapeKey] =>
        string.IsNullOrEmpty(shapeKey)
            ? throw new ArgumentException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey))
            : Ranges[shapeKey];

    public bool TryGetRange(string shapeKey, out ShapeKeyRange range) =>
        string.IsNullOrEmpty(shapeKey)
            ? throw new ArgumentException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey))
            : Ranges.TryGetValue(shapeKey, out range);

    public bool ContainsKey(string shapeKey) =>
        string.IsNullOrEmpty(shapeKey)
            ? throw new ArgumentException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey))
            : Ranges.ContainsKey(shapeKey);

    public bool AddRange(string shapeKey, ShapeKeyRange range)
    {
        if (string.IsNullOrEmpty(shapeKey))
            throw new ArgumentException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey));

        if (Ranges.ContainsKey(shapeKey))
            return false;

        Ranges[shapeKey] = range;

        AddedRange?.Invoke(this, new(shapeKey, range));

        return true;
    }

    public bool RemoveRange(string shapeKey)
    {
        if (string.IsNullOrEmpty(shapeKey))
            throw new ArgumentException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey));

        if (!Ranges.ContainsKey(shapeKey))
            return false;

        Ranges.Remove(shapeKey);

        RemovedRange?.Invoke(this, new(shapeKey, new(0f, 1f)));

        return true;
    }

    public void SetRange(string shapeKey, ShapeKeyRange range)
    {
        if (string.IsNullOrEmpty(shapeKey))
            throw new ArgumentException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey));

        if (!Ranges.ContainsKey(shapeKey))
            return;

        range = FixRange(range);

        Ranges[shapeKey] = range;

        ChangedRange?.Invoke(this, new(shapeKey, range));
    }

    public void Save() =>
        shapeKeyRangeSerializer.Serialize(Ranges);

    public void Refresh()
    {
        ranges = Initialize(shapeKeyRangeSerializer);

        Refreshed?.Invoke(this, EventArgs.Empty);
    }

    private static Dictionary<string, ShapeKeyRange> Initialize(IShapeKeyRangeSerializer shapeKeyRangeSerializer) =>
        shapeKeyRangeSerializer
            .Deserialize()
            .ToDictionary(static kvp => kvp.Key, static kvp => FixRange(kvp.Value), StringComparer.Ordinal);

    private static ShapeKeyRange FixRange(ShapeKeyRange range) =>
        range.Lower == range.Upper ? new(0f, 1f) :
        range.Lower > range.Upper ? new(range.Upper, range.Lower) :
        range;
}

using BepInEx.Configuration;

namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class ShapeKeyConfiguration
{
    private readonly ConfigEntry<ShapeKeyCollection> shapeKeysConfigEntry;
    private readonly ConfigEntry<ShapeKeyCollection> blockedShapeKeysConfigEntry;

    public ShapeKeyConfiguration(
        ConfigFile configFile,
        string category,
        string type,
        params string[] defaultBlockList)
    {
        _ = configFile ?? throw new ArgumentNullException(nameof(configFile));

        if (string.IsNullOrEmpty(category))
            throw new ArgumentException($"'{nameof(category)}' cannot be null or empty.", nameof(category));

        if (string.IsNullOrEmpty(type))
            throw new ArgumentException($"'{nameof(type)}' cannot be null or empty.", nameof(type));

        shapeKeysConfigEntry = configFile.Bind(category, $"{type} Shape Keys", new ShapeKeyCollection());
        blockedShapeKeysConfigEntry = configFile.Bind(category, $"{type} Shape Key Block List", new ShapeKeyCollection(defaultBlockList));
    }

    public event EventHandler<ShapeKeyConfigurationEventArgs> AddedShapeKey;

    public event EventHandler<ShapeKeyConfigurationEventArgs> RemovedShapeKey;

    public event EventHandler<ShapeKeyConfigurationEventArgs> BlockedShapeKey;

    public event EventHandler<ShapeKeyConfigurationEventArgs> UnblockedShapeKey;

    public IEnumerable<string> ShapeKeys =>
        shapeKeysConfigEntry.Value;

    public IEnumerable<string> BlockedShapeKeys =>
        blockedShapeKeysConfigEntry.Value;

    public bool AddShapeKey(string shapeKey)
    {
        if (string.IsNullOrEmpty(shapeKey))
            throw new ArgumentException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey));

        if (!shapeKeysConfigEntry.Value.Add(shapeKey))
            return false;

        AddedShapeKey?.Invoke(this, new(shapeKey));

        return true;
    }

    public void RemoveShapeKey(string shapeKey)
    {
        if (string.IsNullOrEmpty(shapeKey))
            throw new ArgumentException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey));

        if (!shapeKeysConfigEntry.Value.Remove(shapeKey))
            return;

        RemovedShapeKey?.Invoke(this, new(shapeKey));
    }

    public bool BlockShapeKey(string shapeKey)
    {
        if (string.IsNullOrEmpty(shapeKey))
            throw new ArgumentException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey));

        if (!blockedShapeKeysConfigEntry.Value.Add(shapeKey))
            return false;

        BlockedShapeKey?.Invoke(this, new(shapeKey));

        return true;
    }

    public void UnblockShapeKey(string shapeKey)
    {
        if (string.IsNullOrEmpty(shapeKey))
            throw new ArgumentException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey));

        if (!blockedShapeKeysConfigEntry.Value.Remove(shapeKey))
            return;

        UnblockedShapeKey?.Invoke(this, new(shapeKey));
    }

    private class ShapeKeyCollection : IEnumerable<string>
    {
        private readonly List<string> shapeKeys;

        static ShapeKeyCollection() =>
            TomlTypeConverter.AddConverter(
                typeof(ShapeKeyCollection),
                new()
                {
                    ConvertToString = static (shapeKeyCollection, _) => ((ShapeKeyCollection)shapeKeyCollection).Serialize(),
                    ConvertToObject = static (data, _) => Deserialize(data),
                });

        public ShapeKeyCollection() =>
            shapeKeys = [];

        public ShapeKeyCollection(IEnumerable<string> values)
        {
            shapeKeys = [.. values ?? throw new ArgumentNullException(nameof(values))];

            shapeKeys.Sort(StringComparer.Ordinal);
        }

        public bool Add(string shapeKey)
        {
            if (string.IsNullOrEmpty(shapeKey))
                throw new ArgumentException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey));

            if (shapeKeys.Count is 0)
            {
                shapeKeys.Add(shapeKey);

                return true;
            }
            else if (string.CompareOrdinal(shapeKeys[shapeKeys.Count - 1], shapeKey) < 0)
            {
                shapeKeys.Add(shapeKey);

                return true;
            }
            else if (string.CompareOrdinal(shapeKeys[0], shapeKey) > 0)
            {
                shapeKeys.Insert(0, shapeKey);

                return true;
            }
            else
            {
                var index = shapeKeys.BinarySearch(shapeKey);

                if (index >= 0)
                    return false;

                shapeKeys.Insert(~index, shapeKey);

                return true;
            }
        }

        public bool Remove(string shapeKey) =>
            string.IsNullOrEmpty(shapeKey)
                ? throw new ArgumentException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey))
                : shapeKeys.Remove(shapeKey);

        public IEnumerator<string> GetEnumerator() =>
            ((IEnumerable<string>)shapeKeys).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        private static ShapeKeyCollection Deserialize(string data) =>
            new(data.Split([','], StringSplitOptions.RemoveEmptyEntries));

        private string Serialize() =>
            string.Join(",", [.. shapeKeys]);
    }
}

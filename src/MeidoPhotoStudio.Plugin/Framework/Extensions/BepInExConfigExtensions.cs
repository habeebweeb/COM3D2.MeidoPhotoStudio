using BepInEx.Configuration;

namespace MeidoPhotoStudio.Plugin.Framework.Extensions;

public static class BepInExConfigExtensions
{
    public static ConfigEntry<T> Bind<T>(
        this ConfigFile configFile,
        string section,
        string name,
        T defaultValue,
        string description = "",
        AcceptableValueBase acceptableValues = null) =>
        configFile.Bind(
            new ConfigDefinition(section, name),
            defaultValue,
            new ConfigDescription(description, acceptableValues));

    public static T Clamp<T>(this ConfigEntry<T> entry, T value)
        where T : IComparable
    {
        _ = entry ?? throw new ArgumentNullException(nameof(entry));

        return entry.Description?.AcceptableValues is AcceptableValueRange<T> range
            ? (T)range.Clamp(value)
            : value;
    }

    public static T MinimumValue<T>(this ConfigEntry<T> entry)
        where T : IComparable
    {
        _ = entry ?? throw new ArgumentNullException(nameof(entry));

        return entry.Description?.AcceptableValues is AcceptableValueRange<T> range
            ? range.MinValue
            : default;
    }

    public static T MaximumValue<T>(this ConfigEntry<T> entry)
        where T : IComparable
    {
        _ = entry ?? throw new ArgumentNullException(nameof(entry));

        return entry.Description?.AcceptableValues is AcceptableValueRange<T> range
            ? range.MaxValue
            : default;
    }

    public static bool IsValid<T>(this ConfigEntry<T> entry, T value)
    {
        _ = entry ?? throw new ArgumentNullException(nameof(entry));

        return entry.Description?.AcceptableValues is not AcceptableValueBase acceptableValues
            || acceptableValues.IsValid(value);
    }
}

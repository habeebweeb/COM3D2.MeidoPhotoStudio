using BepInEx.Configuration;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class AutoSaveConfiguration
{
    private const string Section = "Auto Save";

    public AutoSaveConfiguration(ConfigFile configFile)
    {
        _ = configFile ?? throw new ArgumentNullException(nameof(configFile));

        Enabled = configFile.Bind(Section, "Enabled", true, "Automatically save scenes");

        Frequency = configFile.Bind(
            Section,
            "Frequency",
            30,
            "Frequency of saves in seconds",
            new AcceptableValueRange<int>(10, int.MaxValue));

        Slots = configFile.Bind(
            Section,
            "Slots",
            25,
            "Maximum number of save slots",
            new AcceptableValueRange<int>(1, int.MaxValue));
    }

    public ConfigEntry<bool> Enabled { get; }

    public ConfigEntry<int> Frequency { get; }

    public ConfigEntry<int> Slots { get; }
}

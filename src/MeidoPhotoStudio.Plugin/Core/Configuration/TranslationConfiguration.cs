using BepInEx.Configuration;

namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class TranslationConfiguration
{
    public TranslationConfiguration(ConfigFile configFile)
    {
        _ = configFile ?? throw new ArgumentNullException(nameof(configFile));

        SuppressWarnings = configFile.Bind(
            "Translation",
            "SuppressWarnings",
            true,
            "Suppress translation warnings from showing up in the console");

        CurrentLanguage = configFile.Bind(
            "Translation",
            "Language",
            "en",
            "Directory to pull translations from\nTranslations are found in the 'Translations' folder");
    }

    public ConfigEntry<string> CurrentLanguage { get; }

    public ConfigEntry<bool> SuppressWarnings { get; }
}

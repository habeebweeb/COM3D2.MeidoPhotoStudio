using BepInEx.Configuration;

namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class TranslationConfiguration
{
    public TranslationConfiguration(ConfigFile configFile)
    {
        _ = configFile ?? throw new ArgumentNullException(nameof(configFile));

        LogMissingTranslations = configFile.Bind(
            "Translation",
            "Log Missing Translations",
            false,
            "Log translations that are missing");

        CurrentLanguage = configFile.Bind(
            "Translation",
            "Language",
            "en",
            "Directory to pull translations from\nTranslations are found in the 'Translations' folder");
    }

    public ConfigEntry<string> CurrentLanguage { get; }

    public ConfigEntry<bool> LogMissingTranslations { get; }
}

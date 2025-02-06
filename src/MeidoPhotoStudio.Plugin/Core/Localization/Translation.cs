using System.Collections.ObjectModel;

using MeidoPhotoStudio.Plugin.Framework.Extensions;
using Newtonsoft.Json;

namespace MeidoPhotoStudio.Plugin.Core.Localization;

public class Translation
{
    private static Dictionary<string, Dictionary<string, string>> minimalTranslations;
    private Dictionary<string, Dictionary<string, string>> translations;

    public Translation(string translationsRootDirectory, string language)
    {
        if (string.IsNullOrEmpty(translationsRootDirectory))
            throw new ArgumentException($"'{nameof(translationsRootDirectory)}' cannot be null or empty.", nameof(translationsRootDirectory));

        if (string.IsNullOrEmpty(language))
            throw new ArgumentException($"'{nameof(language)}' cannot be null or empty.", nameof(language));

        TranslationsRootDirectory = translationsRootDirectory;

        RefreshAvailableLanguages();

        SetLanguage(language);

        translations = InitializeTranslations(Path.Combine(TranslationsRootDirectory, CurrentLanguage));
    }

    public event EventHandler ChangedLanguage;

    public event EventHandler Initialized;

    public event EventHandler RefreshedAvailableLanguages;

    public string TranslationsRootDirectory { get; }

    public string CurrentLanguage { get; private set; }

    public ReadOnlyCollection<string> AvailableLanguages { get; private set; }

    public bool LogMissingTranslations { get; set; }

    // TODO: Consider embedding the ui translation file at minimum rather than this minimal set
    private static Dictionary<string, Dictionary<string, string>> MinimalTranslations =>
        minimalTranslations ??= new()
        {
            ["systemMessage"] = new Dictionary<string, string>()
            {
                ["noTranslations"] = "There are no translations found in '{0}'",
            },
            ["mainWindow"] = new Dictionary<string, string>()
            {
                ["settingsButton"] = "Settings",
            },
            ["settingType"] = new Dictionary<string, string>()
            {
                ["translation"] = "Translation",
            },
            ["translationSettingsPane"] = new Dictionary<string, string>()
            {
                ["reloadTranslationButton"] = "Reload Translation",
                ["selectedLanguageLabel"] = "Select language",
                ["refreshAvailableLanguagesButton"] = "Refresh",
            },
        };

    public string this[string tableKey, string translationKey]
    {
        get
        {
            if (string.IsNullOrEmpty(tableKey))
                throw new ArgumentException($"'{nameof(tableKey)}' cannot be null or empty.", nameof(tableKey));

            if (string.IsNullOrEmpty(translationKey))
                throw new ArgumentException($"'{nameof(translationKey)}' cannot be null or empty.", nameof(translationKey));

            if (!TryGetTranslation(tableKey, translationKey, out var translation))
                translation = translationKey;

            return translation;
        }
    }

    public bool ContainsTranslationTable(string key) =>
        string.IsNullOrEmpty(key)
            ? throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key))
            : translations.ContainsKey(key);

    public bool ContainsTranslation(string tableKey, string translationKey)
    {
        if (string.IsNullOrEmpty(tableKey))
            throw new ArgumentException($"'{nameof(tableKey)}' cannot be null or empty.", nameof(tableKey));

        if (string.IsNullOrEmpty(translationKey))
            throw new ArgumentException($"'{nameof(translationKey)}' cannot be null or empty.", nameof(translationKey));

        if (!translations.TryGetValue(tableKey, out var table))
            return false;

        return table.ContainsKey(translationKey);
    }

    public bool TryGetTranslation(string tableKey, string translationKey, out string translation)
    {
        if (string.IsNullOrEmpty(tableKey))
            throw new ArgumentException($"'{nameof(tableKey)}' cannot be null or empty.", nameof(tableKey));

        if (string.IsNullOrEmpty(translationKey))
            throw new ArgumentException($"'{nameof(translationKey)}' cannot be null or empty.", nameof(translationKey));

        translation = translationKey;

        if (!translations.TryGetValue(tableKey, out var table))
        {
            if (LogMissingTranslations)
                Plugin.Logger.LogInfo($"Translation table key '{tableKey}' does not exist.");

            return false;
        }

        if (!table.TryGetValue(translationKey, out translation))
        {
            if (LogMissingTranslations)
                Plugin.Logger.LogInfo($"Translation table '{tableKey}' does not contain translation '{translationKey}'.");

            return false;
        }

        return true;
    }

    public void RefreshAvailableLanguages()
    {
        Directory.CreateDirectory(TranslationsRootDirectory);

        try
        {
            AvailableLanguages = new([.. new DirectoryInfo(TranslationsRootDirectory)
                .GetDirectories()
                .Select(static directory => directory.Name)]);
        }
        catch
        {
            AvailableLanguages = new([]);
        }

        if (AvailableLanguages.Count is 0)
            Plugin.Logger.LogWarning($"There are no translations in '{TranslationsRootDirectory}'");

        RefreshedAvailableLanguages?.Invoke(this, EventArgs.Empty);
    }

    public void RefreshCurrentTranslations()
    {
        translations = InitializeTranslations(Path.Combine(TranslationsRootDirectory, CurrentLanguage));

        Initialized?.Invoke(this, EventArgs.Empty);
    }

    public void SetLanguage(string language)
    {
        if (string.IsNullOrEmpty(language))
            throw new ArgumentException($"'{nameof(language)}' cannot be null or empty.", nameof(language));

        if (string.Equals(CurrentLanguage, language, StringComparison.OrdinalIgnoreCase))
            return;

        CurrentLanguage = AvailableLanguages.Contains(language, StringComparer.Ordinal)
            ? language
            : string.Empty;

        translations = InitializeTranslations(Path.Combine(TranslationsRootDirectory, CurrentLanguage));

        ChangedLanguage?.Invoke(this, EventArgs.Empty);
        Initialized?.Invoke(this, EventArgs.Empty);
    }

    private static Dictionary<string, Dictionary<string, string>> InitializeTranslations(string translationsDirectory)
    {
        if (string.IsNullOrEmpty(translationsDirectory))
            return MinimalTranslations;

        var allTranslations = new Dictionary<string, Dictionary<string, string>>();
        var jsonSerializer = new JsonSerializer();
        var directoryInfo = new DirectoryInfo(translationsDirectory);

        IEnumerable<FileInfo> translationFiles;

        try
        {
            translationFiles = directoryInfo.GetFiles("*.json");
        }
        catch
        {
            Plugin.Logger.LogWarning($"Could not get translations from '{translationsDirectory}'");

            return MinimalTranslations;
        }

        foreach (var translationFile in translationFiles)
        {
            try
            {
                using var fileStream = translationFile.OpenRead();
                using var streamReader = new StreamReader(fileStream, Encoding.UTF8);
                using var jsonReader = new JsonTextReader(streamReader);

                var translations = jsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonReader);

                foreach (var (table, translation) in translations)
                    allTranslations[table] = new(translation, StringComparer.OrdinalIgnoreCase);
            }
            catch (IOException)
            {
                Plugin.Logger.LogWarning($"Could not read translation file '{translationFile}'");

                return MinimalTranslations;
            }
            catch
            {
                Plugin.Logger.LogWarning($"Could not parse translation file '{translationFile}'");

                return MinimalTranslations;
            }
        }

        return allTranslations.Count is 0 ? MinimalTranslations : allTranslations;
    }
}

using MeidoPhotoStudio.Plugin.Framework.Extensions;
using Newtonsoft.Json;

namespace MeidoPhotoStudio.Plugin.Core.Localization;

public class Translation
{
    private static Dictionary<string, Dictionary<string, string>> minimalTranslations;

    private readonly string translationsRootDirectory;
    private Dictionary<string, Dictionary<string, string>> translations;

    public Translation(string translationsRootDirectory, string currentLanguage)
    {
        if (string.IsNullOrEmpty(translationsRootDirectory))
            throw new ArgumentException($"'{nameof(translationsRootDirectory)}' cannot be null or empty.", nameof(translationsRootDirectory));

        if (string.IsNullOrEmpty(currentLanguage))
            throw new ArgumentException($"'{nameof(currentLanguage)}' cannot be null or empty.", nameof(currentLanguage));

        this.translationsRootDirectory = translationsRootDirectory;

        CurrentLanguage = currentLanguage;

        translations = InitializeTranslations(Path.Combine(this.translationsRootDirectory, CurrentLanguage));
    }

    public event EventHandler ChangedLanguage;

    public event EventHandler Initialized;

    public string CurrentLanguage { get; private set; }

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
            return false;

        if (!table.TryGetValue(translationKey, out translation))
            return false;

        return true;
    }

    public void Refresh()
    {
        translations = InitializeTranslations(Path.Combine(translationsRootDirectory, CurrentLanguage));

        Initialized?.Invoke(this, EventArgs.Empty);
    }

    public void SetLanguage(string language)
    {
        if (string.Equals(CurrentLanguage, language, StringComparison.OrdinalIgnoreCase))
            return;

        CurrentLanguage = language;

        translations = InitializeTranslations(Path.Combine(translationsRootDirectory, language));

        ChangedLanguage?.Invoke(this, EventArgs.Empty);
        Initialized?.Invoke(this, EventArgs.Empty);
    }

    private static Dictionary<string, Dictionary<string, string>> InitializeTranslations(string translationsDirectory)
    {
        if (string.IsNullOrEmpty(translationsDirectory))
            return MinimalTranslations;

        var allTranslations = new Dictionary<string, Dictionary<string, string>>();
        var jsonSerializer = new JsonSerializer();

        foreach (var translationFile in Directory.GetFiles(translationsDirectory, "*.json"))
        {
            try
            {
                using var fileStream = File.OpenRead(translationFile);
                using var streamReader = new StreamReader(fileStream, Encoding.UTF8);
                using var jsonReader = new JsonTextReader(streamReader);

                var translations = jsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonReader);

                foreach (var (table, translation) in translations)
                    allTranslations[table] = new(translation, StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                Plugin.Logger.LogWarning($"Could not load translations from '{translationFile}'");

                return MinimalTranslations;
            }
        }

        return allTranslations.Count is 0 ? MinimalTranslations : allTranslations;
    }
}

namespace MeidoPhotoStudio.Plugin;

public static class Constants
{
    public const string SceneDirectory = "Scenes";
    public const string ConfigDirectory = "MeidoPhotoStudio";
    public const string TranslationDirectory = "Translations";

    public static readonly string ScenesPath;
    public static readonly string ConfigPath;

    static Constants()
    {
        ConfigPath = Path.Combine(BepInEx.Paths.ConfigPath, ConfigDirectory);

        ScenesPath = Path.Combine(ConfigPath, SceneDirectory);

        var directories = new[] { ConfigPath, ScenesPath };

        foreach (var directory in directories)
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
    }

    public enum Window
    {
        Main,
        Message,
        Save,
        Settings,
    }
}

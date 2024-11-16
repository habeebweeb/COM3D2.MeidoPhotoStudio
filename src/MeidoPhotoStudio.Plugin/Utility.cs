namespace MeidoPhotoStudio.Plugin;

// TODO: 🤮 This and the Constants class are a huge disgrace.
public static class Utility
{
    private static readonly BepInEx.Logging.ManualLogSource Logger =
        BepInEx.Logging.Logger.CreateLogSource(Plugin.PluginName);

    public static void LogInfo(object data) =>
        Logger.LogInfo(data);

    public static void LogMessage(object data) =>
        Logger.LogMessage(data);

    public static void LogWarning(object data) =>
        Logger.LogWarning(data);

    public static void LogError(object data) =>
        Logger.LogError(data);

    public static void LogDebug(object data) =>
        Logger.LogDebug(data);
}

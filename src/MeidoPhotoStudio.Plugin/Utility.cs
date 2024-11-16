namespace MeidoPhotoStudio.Plugin;

// TODO: 🤮 This and the Constants class are a huge disgrace.
public static class Utility
{
    private static readonly BepInEx.Logging.ManualLogSource Logger =
        BepInEx.Logging.Logger.CreateLogSource(Plugin.PluginName);

    public static string Timestamp =>
        $"{DateTime.Now:yyyyMMddHHmmss}";

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

    public static bool SeekPngEnd(Stream stream)
    {
        var buffer = new byte[8];

        var pngHeader = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };

        stream.Read(buffer, 0, 8);

        if (!buffer.SequenceEqual(pngHeader))
            return false;

        var pngEnd = Encoding.ASCII.GetBytes("IEND");

        buffer = new byte[4];

        do
        {
            stream.Read(buffer, 0, 4);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);

            var length = BitConverter.ToUInt32(buffer, 0);

            stream.Read(buffer, 0, 4);
            stream.Seek(length + 4L, SeekOrigin.Current);
        }
        while (!buffer.SequenceEqual(pngEnd));

        return true;
    }
}

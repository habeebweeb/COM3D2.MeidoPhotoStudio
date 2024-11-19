using BepInEx.Configuration;

namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class BodyShapeKeyConfiguration(ConfigFile configFile)
    : ShapeKeyConfiguration(configFile, "Character", "Body", DefaultBlockList)
{
    private static readonly string[] DefaultBlockList = ["arml", "hara", "munel", "munes", "munetare", "regfat", "regmeet"];
}

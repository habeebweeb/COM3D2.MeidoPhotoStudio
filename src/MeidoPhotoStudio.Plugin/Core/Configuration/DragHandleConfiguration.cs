using BepInEx.Configuration;

namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class DragHandleConfiguration
{
    private const string Section = "Drag Handles";

    private readonly ConfigFile configFile;

    public DragHandleConfiguration(ConfigFile configFile)
    {
        this.configFile = configFile ?? throw new ArgumentNullException(nameof(configFile));

        SmallTransformCube = this.configFile.Bind(Section, "Small Transform Drag Handles", false);
        CharacterTransformCube = this.configFile.Bind(Section, "Character Transform Drag Handle", false);
    }

    public ConfigEntry<bool> SmallTransformCube { get; }

    public ConfigEntry<bool> CharacterTransformCube { get; }
}

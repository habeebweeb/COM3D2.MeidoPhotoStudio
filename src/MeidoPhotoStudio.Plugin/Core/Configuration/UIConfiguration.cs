using BepInEx.Configuration;
using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class UIConfiguration
{
    private const string Section = "UI";

    public UIConfiguration(ConfigFile configFile)
    {
        _ = configFile ?? throw new ArgumentNullException(nameof(configFile));

        WindowWidth = configFile.Bind(
            Section,
            "Main Window Width",
            MainWindow.MinimumWindowWidth,
            "Width for the main window",
            new AcceptableValueRange<int>(MainWindow.MinimumWindowWidth, int.MaxValue));
    }

    public ConfigEntry<int> WindowWidth { get; }
}

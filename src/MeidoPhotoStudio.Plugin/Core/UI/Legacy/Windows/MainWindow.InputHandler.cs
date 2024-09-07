using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Framework.Input;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

/// <summary>Main window input handler.</summary>
public partial class MainWindow
{
    public class InputHandler(MainWindow mainWindow, InputConfiguration inputConfiguration) : IInputHandler
    {
        private readonly MainWindow mainWindow = mainWindow
            ?? throw new ArgumentNullException(nameof(mainWindow));

        private readonly InputConfiguration inputConfiguration = inputConfiguration
            ?? throw new ArgumentNullException(nameof(inputConfiguration));

        public bool Active { get; } = true;

        public void CheckInput()
        {
            if (inputConfiguration[Shortcut.ToggleMainWindow].IsDown())
                mainWindow.Visible = !mainWindow.Visible;
        }
    }
}

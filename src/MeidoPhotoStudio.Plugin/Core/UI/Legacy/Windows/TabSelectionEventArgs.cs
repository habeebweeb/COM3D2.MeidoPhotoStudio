namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class TabSelectionEventArgs(MainWindow.Tab tab) : EventArgs
{
    public MainWindow.Tab Tab { get; } = tab;
}

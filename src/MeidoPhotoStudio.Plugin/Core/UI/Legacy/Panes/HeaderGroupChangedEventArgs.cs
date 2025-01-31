namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class HeaderGroupChangedEventArgs(PaneHeader changedHeader) : EventArgs
{
    public PaneHeader ChangedHeader { get; } = changedHeader
        ?? throw new ArgumentNullException(nameof(changedHeader));
}

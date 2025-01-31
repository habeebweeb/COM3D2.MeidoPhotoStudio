namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class HeaderGroup : IEnumerable<PaneHeader>
{
    private readonly List<PaneHeader> paneHeaders = [];

    public event EventHandler<HeaderGroupChangedEventArgs> ChangedHeader;

    public void Register(PaneHeader paneHeader)
    {
        _ = paneHeader ?? throw new ArgumentNullException(nameof(paneHeader));

        if (paneHeaders.Contains(paneHeader))
            return;

        paneHeaders.Add(paneHeader);

        paneHeader.ControlEvent += OnHeaderEnabledChanged;
    }

    public bool Deregister(PaneHeader paneHeader)
    {
        _ = paneHeader ?? throw new ArgumentNullException(nameof(paneHeader));

        if (!paneHeaders.Remove(paneHeader))
            return false;

        paneHeader.ControlEvent -= OnHeaderEnabledChanged;

        return true;
    }

    public bool AnyOtherOpen(PaneHeader paneHeader) =>
        paneHeader is null ? throw new ArgumentNullException(nameof(paneHeader)) :
        !paneHeaders.Contains(paneHeader) ? throw new InvalidOperationException($"{nameof(PaneHeader)} is not a part of this group")
        : paneHeaders
            .Where(header => header != paneHeader)
            .Any(header => header.Enabled);

    public IEnumerator<PaneHeader> GetEnumerator() =>
        paneHeaders.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    private void OnHeaderEnabledChanged(object sender, EventArgs e)
    {
        var header = (PaneHeader)sender;

        if (!Input.GetKey(KeyCode.LeftControl))
            return;

        var headerWasAlreadyOpen = !header.Enabled;

        if (headerWasAlreadyOpen)
            header.SetEnabledWithoutNotify(true);

        foreach (var otherHeader in paneHeaders.Where(otherHeader => header != otherHeader))
            otherHeader.SetEnabledWithoutNotify(false);

        ChangedHeader?.Invoke(this, new(header));
    }
}

using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class BaseMainWindowPane : BaseWindow, IEnumerable<BasePane>
{
    protected TabsPane tabsPane;

    protected virtual bool DrawTabs { get; } = true;

    protected virtual bool Scrollable { get; } = true;

    public void SetTabsPane(TabsPane tabsPane) =>
        this.tabsPane = tabsPane;

    public override void Draw()
    {
        if (DrawTabs)
            tabsPane.Draw();

        if (Scrollable)
            scrollPos = GUILayout.BeginScrollView(scrollPos);

        foreach (var pane in Panes)
            pane.Draw();

        if (Scrollable)
            GUILayout.EndScrollView();

        GUI.enabled = true;
    }

    public override void UpdatePanes()
    {
        if (!ActiveWindow)
            return;

        base.UpdatePanes();
    }

    public void Add(BasePane pane) =>
        AddPane(pane);

    public IEnumerator<BasePane> GetEnumerator() =>
        Panes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}

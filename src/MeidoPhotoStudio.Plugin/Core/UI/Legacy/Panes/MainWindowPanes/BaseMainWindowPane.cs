using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class BaseMainWindowPane : BasePane
{
    protected TabsPane tabsPane;

    protected virtual bool DrawTabs { get; } = true;

    protected virtual bool Scrollable { get; } = true;

    protected Vector2 ScrollPosition { get; set; }

    public void SetTabsPane(TabsPane tabsPane) =>
        this.tabsPane = tabsPane;

    public override void Draw()
    {
        if (DrawTabs)
            tabsPane.Draw();

        if (Scrollable)
            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition);

        foreach (var pane in Panes)
            pane.Draw();

        if (Scrollable)
            GUILayout.EndScrollView();

        GUI.enabled = true;
    }
}

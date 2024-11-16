using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class BaseMainWindowPane : BasePane
{
    protected virtual bool Scrollable { get; } = true;

    protected Vector2 ScrollPosition { get; set; }

    public override void Draw()
    {
        GUI.enabled = Parent.Enabled;

        if (Scrollable)
            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition);

        foreach (var pane in Panes)
            pane.Draw();

        GUI.enabled = Parent.Enabled;

        if (Scrollable)
            GUILayout.EndScrollView();
    }
}

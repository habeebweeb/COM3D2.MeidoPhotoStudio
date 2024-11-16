using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class CharacterWindowTabPane : BasePane
{
    private Vector2 scrollPosition;

    public override void Draw()
    {
        GUI.enabled = Parent.Enabled;

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        foreach (var pane in Panes)
            pane.Draw();

        GUI.enabled = Parent.Enabled;

        GUILayout.EndScrollView();
    }
}

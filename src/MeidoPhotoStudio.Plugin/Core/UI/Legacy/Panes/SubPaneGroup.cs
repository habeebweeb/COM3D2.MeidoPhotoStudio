using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class SubPaneGroup(GUIContent headerContent, bool open = false) : BasePane
{
    private const string PaneBackgroundBase64 =
        """
        iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAOklEQVQ4y+2TsREAIAjE8p5TOiBr
        YsVpizQWpE+6iIOTQwDzklcy4ID0KAc2KNKBDvwRUGEmi5lKO2/jugh3pas0yQAAAABJRU5ErkJg
        gg==
        """;

    private static readonly LazyStyle VerticalStyle = new(
        0,
        static () => new(GUI.skin.box)
        {
            normal = { background = UIUtility.LoadTextureFromBase64(16, 16, PaneBackgroundBase64) },
            padding = new(0, 0, 5, 5),
            margin = new(0, 0, 8, 8),
        });

    public SubPaneGroup(string label, bool open = false)
        : this(new GUIContent(label), open)
    {
    }

    public string HeaderLabel
    {
        get => Header.Label;
        set => Header.Label = value;
    }

    public GUIContent HeaderContent
    {
        get => Header.Content;
        set => Header.Content = value;
    }

    public SubPaneHeader Header { get; } = new(headerContent, open);

    public override void Draw()
    {
        GUILayout.BeginVertical(VerticalStyle);

        Header.Draw();

        if (Header.Enabled)
        {
            GUILayout.Space(UIUtility.Scaled(3f));

            for (var i = 0; i < PaneCount; i++)
            {
                this[i].Draw();

                if (i != PaneCount - 1)
                    UIUtility.DrawBlackLine();
            }
        }

        GUILayout.EndVertical();
    }
}

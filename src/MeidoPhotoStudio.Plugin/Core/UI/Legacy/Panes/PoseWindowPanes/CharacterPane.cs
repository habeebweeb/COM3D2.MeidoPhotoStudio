using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class CharacterPane : BasePane
{
    private readonly Translation translation;
    private readonly TabSelectionController tabSelectionController;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly Dictionary<CharacterWindowTab, Toggle> tabs = [];
    private readonly Dictionary<CharacterWindowTab, CharacterWindowTabPane> windowPanes =
        new(EnumEqualityComparer<CharacterWindowTab>.Instance);

    private readonly Toggle.Group tabGroup;
    private readonly Label noCharactersLabel;
    private readonly LazyStyle tabStyle = new(StyleSheet.TextSize, static () => new(GUI.skin.button));
    private readonly LazyStyle labelStyle = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
        });

    private CharacterWindowTabPane currentTab;

    public CharacterPane(
        Translation translation,
        TabSelectionController tabSelectionController,
        SelectionController<CharacterController> characterSelectionController)
    {
        this.translation = translation ?? throw new ArgumentNullException(nameof(translation));
        this.tabSelectionController = tabSelectionController
            ?? throw new ArgumentNullException(nameof(tabSelectionController));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.tabSelectionController.TabSelected += OnTabSelected;

        tabGroup = new();
        noCharactersLabel = new(new LocalizableGUIContent(translation, "characterPane", "noCharactersLabel"));
    }

    public enum CharacterWindowTab
    {
        Pose,
        Face,
        Body,
    }

    public CharacterWindowTabPane this[CharacterWindowTab tab]
    {
        get => windowPanes[tab];
        set => AddTab(tab, value);
    }

    public override void Draw()
    {
        GUI.enabled = Parent.Enabled;

        if (characterSelectionController.Current is null)
        {
            noCharactersLabel.Draw(labelStyle);

            return;
        }

        GUILayout.BeginHorizontal();

        foreach (var tab in tabGroup)
            tab.Draw(tabStyle);

        GUILayout.EndHorizontal();

        UIUtility.DrawBlackLine();

        currentTab.Draw();

        GUI.enabled = Parent.Enabled;
    }

    public override void Activate()
    {
        base.Activate();

        SelectTab(CharacterWindowTab.Pose);
    }

    private void AddTab(CharacterWindowTab tab, CharacterWindowTabPane pane)
    {
        _ = pane ?? throw new ArgumentNullException(nameof(pane));

        windowPanes[tab] = pane;
        Add(pane);

        var toggle = new Toggle(new LocalizableGUIContent(translation, "characterPaneTabs", tab.ToLower()));

        toggle.ControlEvent += (sender, _) =>
        {
            if (sender is not Toggle { Value: true })
                return;

            currentTab = pane;
        };

        tabs[tab] = toggle;
        tabGroup.Add(toggle);
    }

    private void OnTabSelected(object sender, TabSelectionEventArgs e)
    {
        if (e.Tab is not (MainWindow.Tab.CharacterPose or MainWindow.Tab.CharacterFace))
            return;

        SelectTab(e.Tab switch
        {
            MainWindow.Tab.CharacterPose => CharacterWindowTab.Pose,
            MainWindow.Tab.CharacterFace => CharacterWindowTab.Face,
            _ => CharacterWindowTab.Pose,
        });
    }

    private void SelectTab(CharacterWindowTab tab)
    {
        tabs[tab].SetEnabledWithoutNotify(true);
        currentTab = windowPanes[tab];
    }
}

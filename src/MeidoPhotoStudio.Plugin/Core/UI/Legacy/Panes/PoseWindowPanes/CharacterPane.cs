using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class CharacterPane : BasePane
{
    private readonly TabSelectionController tabSelectionController;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly List<CharacterWindowTab> tabs = [];
    private readonly Dictionary<CharacterWindowTab, CharacterWindowTabPane> windowPanes =
        new(EnumEqualityComparer<CharacterWindowTab>.Instance);

    private readonly Label noCharactersLabel;
    private readonly SelectionGrid tabSelectionGrid;
    private readonly LazyStyle tabStyle = new(StyleSheet.TextSize, static () => new(GUI.skin.button));
    private readonly LazyStyle labelStyle = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
        });

    private CharacterWindowTabPane currentTab;

    public CharacterPane(
        TabSelectionController tabSelectionController,
        SelectionController<CharacterController> characterSelectionController)
    {
        this.tabSelectionController = tabSelectionController
            ?? throw new ArgumentNullException(nameof(tabSelectionController));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.tabSelectionController.TabSelected += OnTabSelected;

        tabSelectionGrid = new SelectionGrid([]);
        tabSelectionGrid.ControlEvent += OnTabChanged;

        noCharactersLabel = new(Translation.Get("characterPane", "noCharactersLabel"));
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

        tabSelectionGrid.Draw(tabStyle);
        UIUtility.DrawBlackLine();

        currentTab.Draw();

        GUI.enabled = Parent.Enabled;
    }

    public override void Activate()
    {
        base.Activate();

        tabSelectionGrid.SelectedItemIndex = 0;
    }

    protected override void ReloadTranslation()
    {
        tabSelectionGrid.SetItemsWithoutNotify(Translation.GetArray("characterPaneTabs", tabs.Select(static tab => tab.ToLower())));
        noCharactersLabel.Text = Translation.Get("characterPane", "noCharactersLabel");
    }

    private void AddTab(CharacterWindowTab tab, CharacterWindowTabPane pane)
    {
        _ = pane ?? throw new ArgumentNullException(nameof(pane));

        windowPanes[tab] = pane;
        Add(pane);
        tabs.Add(tab);

        tabSelectionGrid.SetItems(Translation.GetArray("characterPaneTabs", tabs.Select(static tab => tab.ToLower())), 0);
    }

    private void OnTabSelected(object sender, TabSelectionEventArgs e)
    {
        if (e.Tab is not (MainWindow.Tab.CharacterPose or MainWindow.Tab.CharacterFace))
            return;

        tabSelectionGrid.SelectedItemIndex = e.Tab switch
        {
            MainWindow.Tab.CharacterPose => 0,
            MainWindow.Tab.CharacterFace => 1,
            _ => 0,
        };
    }

    private void OnTabChanged(object sender, EventArgs e)
    {
        var tab = tabSelectionGrid.SelectedItemIndex switch
        {
            0 => CharacterWindowTab.Pose,
            1 => CharacterWindowTab.Face,
            2 => CharacterWindowTab.Body,
            _ => CharacterWindowTab.Pose,
        };

        currentTab = windowPanes[tab];
    }
}

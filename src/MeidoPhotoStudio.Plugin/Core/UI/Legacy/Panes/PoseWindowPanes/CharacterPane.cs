using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class CharacterPane : BasePane
{
    private static readonly string[] CharacterTabTranslationKeys = ["bodyTab", "faceTab"];

    private readonly TabSelectionController tabSelectionController;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly Dictionary<CharacterWindowTab, CharacterWindowTabPane> windowPanes =
        new(EnumEqualityComparer<CharacterWindowTab>.Instance);

    private readonly Label noCharactersLabel;
    private readonly SelectionGrid tabs;
    private readonly LazyStyle tabStyle = new(13, static () => new(GUI.skin.button));
    private readonly LazyStyle labelStyle = new(
        13,
        () => new(GUI.skin.label)
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

        tabs = new SelectionGrid(Translation.GetArray("characterPane", CharacterTabTranslationKeys));
        tabs.ControlEvent += OnTabChanged;

        noCharactersLabel = new(Translation.Get("characterPane", "noCharactersLabel"));
    }

    public enum CharacterWindowTab
    {
        Pose,
        Face,
    }

    public CharacterWindowTabPane this[CharacterWindowTab tab]
    {
        get => windowPanes[tab];
        set
        {
            windowPanes[tab] = value;
            Add(value);
        }
    }

    public override void Draw()
    {
        if (characterSelectionController.Current is null)
        {
            noCharactersLabel.Draw(labelStyle);

            return;
        }

        tabs.Draw(tabStyle);
        MpsGui.WhiteLine();

        currentTab.Draw();

        GUI.enabled = true;
    }

    public override void Activate()
    {
        base.Activate();

        tabs.SelectedItemIndex = 0;
    }

    protected override void ReloadTranslation()
    {
        tabs.SetItemsWithoutNotify(Translation.GetArray("characterPane", CharacterTabTranslationKeys));
        noCharactersLabel.Text = Translation.Get("characterPane", "noCharactersLabel");
    }

    private void OnTabSelected(object sender, TabSelectionEventArgs e)
    {
        if (e.Tab is not (Constants.Window.Pose or Constants.Window.Face))
            return;

        tabs.SelectedItemIndex = e.Tab switch
        {
            Constants.Window.Pose => 0,
            Constants.Window.Face => 1,
            _ => 0,
        };
    }

    private void OnTabChanged(object sender, EventArgs e)
    {
        var tab = tabs.SelectedItemIndex switch
        {
            0 => CharacterWindowTab.Pose,
            1 => CharacterWindowTab.Face,
            _ => CharacterWindowTab.Pose,
        };

        currentTab = windowPanes[tab];
    }
}

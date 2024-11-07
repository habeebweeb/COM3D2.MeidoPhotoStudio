using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class AttachedAccessoryPane : BasePane
{
    private const int NoAccessoryIndex = 0;
    private const string NoAccessoryTranslationKey = "noAccessory";

    private static readonly MPN KousokuUpper = SafeMpn.GetValue(nameof(MPN.kousoku_upper));
    private static readonly MPN KousokuLower = SafeMpn.GetValue(nameof(MPN.kousoku_lower));
    private static readonly MPN[] AccessoryCategory = [KousokuUpper, KousokuLower];

    private static readonly string[] AccessoryCategoryTranslationKeys = ["upperAccessoryTab", "lowerAccessoryTab"];

    private readonly MenuPropRepository menuPropRepository;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly PaneHeader paneHeader;
    private readonly Dropdown<MenuFilePropModel> accessoryDropdown;
    private readonly SelectionGrid accessoryCategoryGrid;
    private readonly Button detachAllAccessoriesButton;
    private readonly Label initializingLabel;

    private bool menuDatabaseBusy;

    public AttachedAccessoryPane(
        MenuPropRepository menuPropRepository, SelectionController<CharacterController> characterSelectionController)
    {
        this.menuPropRepository = menuPropRepository ?? throw new ArgumentNullException(nameof(menuPropRepository));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.characterSelectionController.Selecting += OnCharacterSelectionChanging;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        paneHeader = new(Translation.Get("attachMpnPropPane", "header"), true);

        accessoryCategoryGrid = new(Translation.GetArray("attachMpnPropPane", AccessoryCategoryTranslationKeys));
        accessoryCategoryGrid.ControlEvent += OnAccessoryCategoryChanged;

        accessoryDropdown = new((model, _) =>
            new LabelledDropdownItem(model is null
                ? Translation.Get("attachMpnPropPane", NoAccessoryTranslationKey)
                : Translation.Get("mpnAttachPropNames", model.Filename)));

        accessoryDropdown.SelectionChanged += OnAccessoryChanged;

        detachAllAccessoriesButton = new(Translation.Get("attachMpnPropPane", "detachAllButton"));
        detachAllAccessoriesButton.ControlEvent += OnDetachAllButtonPressed;

        initializingLabel = new(Translation.Get("systemMessage", "initializing"));

        if (menuPropRepository.Busy)
        {
            menuDatabaseBusy = true;

            menuPropRepository.InitializedProps += OnMenuDatabaseReady;
        }
        else
        {
            Initialize();
        }

        void OnMenuDatabaseReady(object sender, EventArgs e)
        {
            menuDatabaseBusy = false;

            Initialize();

            menuPropRepository.InitializedProps -= OnMenuDatabaseReady;
        }

        void Initialize() =>
            accessoryDropdown.SetItems(AccessoryList());
    }

    private ClothingController CurrentClothing =>
        characterSelectionController.Current?.Clothing;

    private MPN CurrentCategory =>
        AccessoryCategory[accessoryCategoryGrid.SelectedItemIndex];

    public override void Draw()
    {
        var enabled = characterSelectionController.Current is not null;

        GUI.enabled = enabled;

        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        if (menuDatabaseBusy)
        {
            initializingLabel.Draw();
        }
        else
        {
            accessoryCategoryGrid.Draw();
            MpsGui.BlackLine();

            DrawDropdown(accessoryDropdown);
            MpsGui.BlackLine();

            detachAllAccessoriesButton.Draw();
        }
    }

    protected override void ReloadTranslation()
    {
        paneHeader.Label = Translation.Get("attachMpnPropPane", "header");
        accessoryCategoryGrid.SetItemsWithoutNotify(Translation.GetArray("attachMpnPropPane", AccessoryCategoryTranslationKeys));
        detachAllAccessoriesButton.Label = Translation.Get("attachMpnPropPane", "detachAllButton");

        accessoryDropdown.Reformat();
        initializingLabel.Text = Translation.Get("systemMessage", "initializing");
    }

    private void OnAccessoryCategoryChanged(object sender, EventArgs e)
    {
        if (menuPropRepository.Busy)
            return;

        accessoryDropdown.SetItemsWithoutNotify(AccessoryList());

        UpdateAccessoryDropdownSelection();
    }

    private void OnAccessoryChanged(object sender, EventArgs e)
    {
        if (menuPropRepository.Busy)
            return;

        if (CurrentClothing is null)
            return;

        if (accessoryDropdown.SelectedItemIndex is NoAccessoryIndex)
        {
            if (CurrentCategory == SafeMpn.GetValue(nameof(MPN.kousoku_lower)))
                CurrentClothing.DetachLowerAccessory();
            else
                CurrentClothing.DetachUpperAccessory();
        }
        else
        {
            CurrentClothing.AttachAccessory(accessoryDropdown.SelectedItem);
        }
    }

    private void OnDetachAllButtonPressed(object sender, EventArgs e)
    {
        if (CurrentClothing is null)
            return;

        CurrentClothing.DetachAllAccessories();

        accessoryDropdown.SetSelectedIndexWithoutNotify(NoAccessoryIndex);
    }

    private void OnCharacterSelectionChanging(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        e.Selected.Clothing.PropertyChanged -= OnClothingPropertyChanged;
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        e.Selected.Clothing.PropertyChanged += OnClothingPropertyChanged;

        UpdateAccessoryDropdownSelection();
    }

    private void OnClothingPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var controller = (ClothingController)sender;

        (var changedCategory, var changedAccessory) = e.PropertyName switch
        {
            nameof(ClothingController.AttachedLowerAccessory) => (KousokuLower, controller.AttachedLowerAccessory),
            nameof(ClothingController.AttachedUpperAccessory) => (KousokuUpper, controller.AttachedUpperAccessory),
            _ => (MPN.null_mpn, null),
        };

        if (changedCategory is MPN.null_mpn || CurrentCategory != changedCategory)
            return;

        if (changedAccessory is null)
        {
            accessoryDropdown.SetSelectedIndexWithoutNotify(0);

            return;
        }

        var accessoryIndex = accessoryDropdown.Skip(1).IndexOf(changedAccessory);

        if (accessoryIndex < 0)
            return;

        accessoryDropdown.SetSelectedIndexWithoutNotify(accessoryIndex + 1);
    }

    private void UpdateAccessoryDropdownSelection()
    {
        if (CurrentClothing is null)
            return;

        var currentAccessory = CurrentCategory == KousokuLower
            ? CurrentClothing.AttachedLowerAccessory
            : CurrentClothing.AttachedUpperAccessory;

        if (currentAccessory is null)
        {
            accessoryDropdown.SetSelectedIndexWithoutNotify(NoAccessoryIndex);

            return;
        }

        var accessoryIndex = accessoryDropdown.Skip(1).IndexOf(currentAccessory);

        if (accessoryIndex < 0)
            return;

        accessoryDropdown.SetSelectedIndexWithoutNotify(accessoryIndex + 1);
    }

    private IEnumerable<MenuFilePropModel> AccessoryList() =>
        menuPropRepository.Busy
            ? []
            : new MenuFilePropModel[] { null }
                .Concat(menuPropRepository[AccessoryCategory[accessoryCategoryGrid.SelectedItemIndex]]);
}

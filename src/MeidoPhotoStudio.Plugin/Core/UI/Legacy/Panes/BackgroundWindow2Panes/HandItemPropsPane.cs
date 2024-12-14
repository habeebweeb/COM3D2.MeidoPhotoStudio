using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class HandItemPropsPane : BasePane
{
    private static readonly MPN HandItem = SafeMpn.GetValue(nameof(MPN.handitem));

    private readonly PropService propService;
    private readonly Dropdown<MenuFilePropModel> propDropdown;
    private readonly Button addPropButton;
    private readonly Label initializingLabel;
    private readonly Label noHandItemsLabel;
    private readonly SearchBar<MenuFilePropModel> searchBar;

    private bool menuDatabaseBusy = false;
    private bool hasHandItems;

    public HandItemPropsPane(
        PropService propService,
        MenuPropRepository menuPropRepository)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        _ = menuPropRepository ?? throw new ArgumentNullException(nameof(menuPropRepository));

        searchBar = new(SearchSelector, PropFormatter)
        {
            Placeholder = Translation.Get("handItemPropsPane", "searchBarPlaceholder"),
        };

        searchBar.SelectedValue += OnSearchSelected;

        propDropdown = new(formatter: PropFormatter);

        addPropButton = new(Translation.Get("propsPane", "addProp"));
        addPropButton.ControlEvent += OnAddPropButtonPressed;

        initializingLabel = new(Translation.Get("systemMessage", "initializing"));
        noHandItemsLabel = new(Translation.Get("handItemPropsPane", "noHandItems"));

        if (menuPropRepository.Busy)
        {
            menuDatabaseBusy = true;
            menuPropRepository.InitializedProps += OnMenuDatabaseIndexed;
        }
        else
        {
            Initialize();
        }

        static LabelledDropdownItem PropFormatter(MenuFilePropModel prop, int index) =>
            new(prop.Name);

        void OnMenuDatabaseIndexed(object sender, EventArgs e)
        {
            menuDatabaseBusy = false;
            menuPropRepository.InitializedProps -= OnMenuDatabaseIndexed;

            Initialize();
        }

        void Initialize()
        {
            var handItems = menuPropRepository.ContainsCategory(HandItem)
                ? menuPropRepository[HandItem]
                : [];

            propDropdown.SetItems(handItems);

            hasHandItems = handItems.Any();
        }

        IEnumerable<MenuFilePropModel> SearchSelector(string query) =>
            menuPropRepository.Busy || !menuPropRepository.ContainsCategory(HandItem)
                ? []
                : menuPropRepository[HandItem].Where(model =>
                    model.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    Path.GetFileNameWithoutExtension(model.Filename).Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    public override void Draw()
    {
        if (menuDatabaseBusy)
        {
            initializingLabel.Draw();

            return;
        }

        if (!hasHandItems)
        {
            noHandItemsLabel.Draw();

            return;
        }

        DrawTextFieldWithScrollBarOffset(searchBar);

        DrawDropdown(propDropdown);

        UIUtility.DrawBlackLine();

        addPropButton.Draw();
    }

    protected override void ReloadTranslation()
    {
        initializingLabel.Text = Translation.Get("systemMessage", "initializing");
        noHandItemsLabel.Text = Translation.Get("handItemPropsPane", "noHandItems");
        propDropdown.Reformat();

        addPropButton.Label = Translation.Get("propsPane", "addProp");
        searchBar.Placeholder = Translation.Get("handItemPropsPane", "searchBarPlaceholder");
        searchBar.Reformat();
    }

    private void OnSearchSelected(object sender, SearchBarSelectionEventArgs<MenuFilePropModel> e) =>
        propService.Add(e.Item);

    private void OnAddPropButtonPressed(object sender, EventArgs e) =>
        propService.Add(propDropdown.SelectedItem);
}

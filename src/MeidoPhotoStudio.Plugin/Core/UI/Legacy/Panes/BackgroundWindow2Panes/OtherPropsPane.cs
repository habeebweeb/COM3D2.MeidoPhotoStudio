using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class OtherPropsPane : BasePane
{
    private readonly PropService propService;
    private readonly OtherPropRepository otherPropRepository;
    private readonly Dropdown<string> propCategoryDropdown;
    private readonly Dropdown<OtherPropModel> propDropdown;
    private readonly Button addPropButton;
    private readonly SearchBar<OtherPropModel> searchBar;

    public OtherPropsPane(Translation translation, PropService propService, OtherPropRepository otherPropRepository)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.otherPropRepository = otherPropRepository ?? throw new ArgumentNullException(nameof(otherPropRepository));

        translation.Initialized += OnTranslationInitialized;

        searchBar = new(SearchSelector, PropFormatter)
        {
            PlaceholderContent = new LocalizableGUIContent(translation, "otherPropsPane", "searchBarPlaceholder"),
        };

        searchBar.SelectedValue += OnSearchSelected;

        var categories = otherPropRepository.Categories.ToArray();

        propCategoryDropdown = new(categories, formatter: CategoryFormatter);
        propCategoryDropdown.SelectionChanged += OnPropCategoryDropdownChanged;

        propDropdown = new(this.otherPropRepository[propCategoryDropdown.SelectedItem], formatter: PropFormatter);

        addPropButton = new(new LocalizableGUIContent(translation, "propsPane", "addProp"));
        addPropButton.ControlEvent += OnAddPropButtonPressed;

        static LabelledDropdownItem PropFormatter(OtherPropModel prop, int index) =>
            new(prop.Name);

        LabelledDropdownItem CategoryFormatter(string category, int index) =>
            new(translation["otherPropCategories", category]);

        IEnumerable<OtherPropModel> SearchSelector(string query) =>
            otherPropRepository.Where(model => model.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

        void OnTranslationInitialized(object sender, EventArgs e)
        {
            propCategoryDropdown.Reformat();
            propDropdown.Reformat();
            searchBar.Reformat();
        }
    }

    public override void Draw()
    {
        DrawTextFieldWithScrollBarOffset(searchBar);

        DrawDropdown(propCategoryDropdown);
        DrawDropdown(propDropdown);

        UIUtility.DrawBlackLine();

        addPropButton.Draw();
    }

    private void OnSearchSelected(object sender, SearchBarSelectionEventArgs<OtherPropModel> e) =>
        propService.Add(e.Item);

    private void OnPropCategoryDropdownChanged(object sender, EventArgs e) =>
        propDropdown.SetItems(otherPropRepository[propCategoryDropdown.SelectedItem]);

    private void OnAddPropButtonPressed(object sender, EventArgs e) =>
        propService.Add(propDropdown.SelectedItem);
}

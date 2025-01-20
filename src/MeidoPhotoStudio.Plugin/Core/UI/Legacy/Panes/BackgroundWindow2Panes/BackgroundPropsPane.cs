using MeidoPhotoStudio.Plugin.Core.Database.Background;
using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class BackgroundPropsPane : BasePane
{
    private readonly PropService propService;
    private readonly BackgroundPropRepository backgroundPropRepository;
    private readonly Dropdown<BackgroundCategory> propCategoryDropdown;
    private readonly Dropdown<BackgroundPropModel> propDropdown;
    private readonly Button addPropButton;
    private readonly SearchBar<BackgroundPropModel> searchBar;

    public BackgroundPropsPane(
        Translation translation, PropService propService, BackgroundPropRepository backgroundPropRepository)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.backgroundPropRepository = backgroundPropRepository ?? throw new ArgumentNullException(nameof(backgroundPropRepository));

        translation.Initialized += OnTranslationInitialized;

        searchBar = new(SearchSelector, PropFormatter)
        {
            PlaceholderContent = new LocalizableGUIContent(translation, "backgroundPropsPane", "searchBarPlaceholder"),
        };

        searchBar.SelectedValue += OnSearchSelected;

        var categories = backgroundPropRepository.Categories.OrderBy(category => category).ToArray();

        propCategoryDropdown = new(categories, Array.IndexOf(categories, BackgroundCategory.COM3D2), CategoryFormatter);
        propCategoryDropdown.SelectionChanged += OnPropCategoryDropdownChanged;

        propDropdown = new(
            this.backgroundPropRepository[propCategoryDropdown.SelectedItem],
            formatter: PropFormatter);

        addPropButton = new(new LocalizableGUIContent(translation, "propsPane", "addProp"));
        addPropButton.ControlEvent += OnAddPropButtonPressed;

        static LabelledDropdownItem PropFormatter(BackgroundPropModel prop, int index) =>
            new(prop.Name);

        LabelledDropdownItem CategoryFormatter(BackgroundCategory category, int index)
        {
            var translationKey = category switch
            {
                BackgroundCategory.CM3D2 => "cm3d2",
                BackgroundCategory.COM3D2 => "com3d2",
                BackgroundCategory.MyRoomCustom => "myRoomCustom",
                _ => throw new NotSupportedException($"{nameof(category)} is not supported"),
            };

            return new(translation["backgroundSource", translationKey]);
        }

        IEnumerable<BackgroundPropModel> SearchSelector(string query) =>
            backgroundPropRepository
                .Where(model => model.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    model.AssetName.Contains(query, StringComparison.OrdinalIgnoreCase));

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

    private void OnSearchSelected(object sender, SearchBarSelectionEventArgs<BackgroundPropModel> e) =>
        propService.Add(e.Item);

    private void OnPropCategoryDropdownChanged(object sender, EventArgs e) =>
        propDropdown.SetItems(backgroundPropRepository[propCategoryDropdown.SelectedItem], 0);

    private void OnAddPropButtonPressed(object sender, EventArgs e) =>
        propService.Add(propDropdown.SelectedItem);
}

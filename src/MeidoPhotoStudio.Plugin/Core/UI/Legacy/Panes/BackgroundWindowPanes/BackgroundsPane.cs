using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Background;
using MeidoPhotoStudio.Plugin.Core.Database.Background;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class BackgroundsPane : BasePane
{
    private readonly BackgroundService backgroundService;
    private readonly BackgroundRepository backgroundRepository;
    private readonly BackgroundDragHandleService backgroundDragHandleService;
    private readonly Dropdown<BackgroundCategory> backgroundCategoryDropdown;
    private readonly Dropdown<BackgroundModel> backgroundDropdown;
    private readonly Toggle dragHandleEnabledToggle;
    private readonly Toggle backgroundVisibleToggle;
    private readonly SubPaneHeader colourModeToggle;
    private readonly Slider redSlider;
    private readonly Slider greenSlider;
    private readonly Slider blueSlider;
    private readonly PaneHeader paneHeader;
    private readonly SearchBar<BackgroundModel> searchBar;

    public BackgroundsPane(
        Translation translation,
        BackgroundService backgroundService,
        BackgroundRepository backgroundRepository,
        BackgroundDragHandleService backgroundDragHandleService)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.backgroundService = backgroundService ?? throw new ArgumentNullException(nameof(backgroundService));
        this.backgroundRepository = backgroundRepository ?? throw new ArgumentNullException(nameof(backgroundRepository));
        this.backgroundDragHandleService = backgroundDragHandleService ?? throw new ArgumentNullException(nameof(backgroundDragHandleService));

        this.backgroundService.PropertyChanged += OnBackgroundPropertyChanged;

        translation.Initialized += OnTranslationInitialized;

        paneHeader = new(new LocalizableGUIContent(translation, "backgroundsPane", "header"));

        searchBar = new(SearchSelector, PropFormatter)
        {
            PlaceholderContent = new LocalizableGUIContent(translation, "backgroundsPane", "searchBarPlaceholder"),
        };

        searchBar.SelectedValue += OnSearchSelected;

        backgroundCategoryDropdown = new(BackgroundCategoryFormatter);
        backgroundCategoryDropdown.SelectionChanged += OnChangedCategory;

        backgroundDropdown = new(PropFormatter);
        backgroundDropdown.SelectionChanged += OnChangedBackground;

        dragHandleEnabledToggle = new(
            new LocalizableGUIContent(translation, "backgroundsPane", "dragHandleVisible"),
            backgroundDragHandleService.Enabled);

        dragHandleEnabledToggle.ControlEvent += OnToggledDragHandleEnabled;

        backgroundVisibleToggle = new(
            new LocalizableGUIContent(translation, "backgroundsPane", "backgroundVisible"),
            backgroundService.BackgroundVisible);

        backgroundVisibleToggle.ControlEvent += OnToggledBackgroundVisible;

        colourModeToggle = new(new LocalizableGUIContent(translation, "backgroundsPane", "colour"), false);

        var backgroundColour = backgroundService.BackgroundColour;

        redSlider = new(
            new LocalizableGUIContent(translation, "backgroundsPane", "red"),
            0f,
            1f,
            backgroundColour.r,
            backgroundColour.r)
        {
            HasReset = true,
            HasTextField = true,
        };

        redSlider.ControlEvent += OnColourSliderChanged;

        greenSlider = new(
            new LocalizableGUIContent(translation, "backgroundsPane", "green"),
            0f,
            1f,
            backgroundColour.g,
            backgroundColour.g)
        {
            HasReset = true,
            HasTextField = true,
        };

        greenSlider.ControlEvent += OnColourSliderChanged;

        blueSlider = new(
            new LocalizableGUIContent(translation, "backgroundsPane", "blue"),
            0f,
            1f,
            backgroundColour.b,
            backgroundColour.b)
        {
            HasReset = true,
            HasTextField = true,
        };

        blueSlider.ControlEvent += OnColourSliderChanged;

        LabelledDropdownItem BackgroundCategoryFormatter(BackgroundCategory category, int index)
        {
            var translationKey = category switch
            {
                BackgroundCategory.COM3D2 => "com3d2",
                BackgroundCategory.CM3D2 => "cm3d2",
                BackgroundCategory.MyRoomCustom => "myRoomCustom",
                _ => throw new InvalidEnumArgumentException(nameof(category), (int)category, typeof(BackgroundCategory)),
            };

            return new(translation["backgroundSource", translationKey]);
        }

        IEnumerable<BackgroundModel> SearchSelector(string query) =>
            backgroundRepository.Where(model => model.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

        IDropdownItem PropFormatter(BackgroundModel model, int index) =>
            new LabelledDropdownItem(model.Name);

        void OnTranslationInitialized(object sender, EventArgs e)
        {
            searchBar.Reformat();
            backgroundCategoryDropdown.Reformat();
            backgroundDropdown.Reformat();
        }
    }

    public override void Draw()
    {
        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        DrawTextFieldWithScrollBarOffset(searchBar);

        DrawDropdown(backgroundCategoryDropdown);
        DrawDropdown(backgroundDropdown);

        DrawToggles();

        UIUtility.DrawBlackLine();

        DrawColourSliders();

        void DrawToggles()
        {
            GUILayout.BeginHorizontal();

            backgroundVisibleToggle.Draw();

            dragHandleEnabledToggle.Draw(GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();
        }

        void DrawColourSliders()
        {
            colourModeToggle.Draw();

            if (!colourModeToggle.Enabled)
                return;

            redSlider.Draw();
            greenSlider.Draw();
            blueSlider.Draw();
        }
    }

    public override void Activate()
    {
        base.Activate();

        var background = backgroundService.CurrentBackground;

        InitializeCategoryDropdown(background);
        InitializeBackgroundDropdown(background);

        void InitializeCategoryDropdown(BackgroundModel background)
        {
            var categories = backgroundRepository.Categories.ToArray();

            var categoryIndex = Array.IndexOf(categories, background.Category);

            if (categoryIndex < 0)
                categoryIndex = 0;

            backgroundCategoryDropdown.SetItemsWithoutNotify(categories, categoryIndex);
        }

        void InitializeBackgroundDropdown(BackgroundModel background)
        {
            var backgrounds = backgroundRepository[background.Category].ToArray();

            var backgroundIndex = Array.IndexOf(backgrounds, background);

            if (backgroundIndex < 0)
                backgroundIndex = 0;

            backgroundDropdown.SetItemsWithoutNotify(backgrounds, backgroundIndex);
        }
    }

    private void OnToggledDragHandleEnabled(object sender, EventArgs e) =>
        backgroundDragHandleService.Enabled = dragHandleEnabledToggle.Value;

    private void OnToggledBackgroundVisible(object sender, EventArgs e) =>
        backgroundService.BackgroundVisible = backgroundVisibleToggle.Value;

    private void OnColourSliderChanged(object sender, EventArgs e) =>
        backgroundService.BackgroundColour = new(redSlider.Value, greenSlider.Value, blueSlider.Value);

    private void OnChangedCategory(object sender, EventArgs e) =>
        backgroundDropdown.SetItems(backgroundRepository[backgroundCategoryDropdown.SelectedItem], 0);

    private void OnBackgroundPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var service = (BackgroundService)sender;

        if (e.PropertyName is nameof(BackgroundService.CurrentBackground))
        {
            UpdatePanel(service);
        }
        else if (e.PropertyName is nameof(BackgroundService.BackgroundVisible))
        {
            backgroundVisibleToggle.SetEnabledWithoutNotify(service.BackgroundVisible);
        }
        else if (e.PropertyName is nameof(BackgroundService.BackgroundColour))
        {
            redSlider.SetValueWithoutNotify(service.BackgroundColour.r);
            greenSlider.SetValueWithoutNotify(service.BackgroundColour.g);
            blueSlider.SetValueWithoutNotify(service.BackgroundColour.b);
        }

        void UpdatePanel(BackgroundService service)
        {
            if (service.CurrentBackground == backgroundDropdown.SelectedItem)
                return;

            var categoryIndex = GetCategoryIndex(service.CurrentBackground);
            var oldCategoryIndex = backgroundCategoryDropdown.SelectedItemIndex;

            backgroundCategoryDropdown.SetSelectedIndexWithoutNotify(categoryIndex);

            if (categoryIndex != oldCategoryIndex)
                backgroundDropdown.SetItemsWithoutNotify(backgroundRepository[service.CurrentBackground.Category]);

            var backgroundIndex = GetBackgroundIndex(service.CurrentBackground);

            backgroundDropdown.SetSelectedIndexWithoutNotify(backgroundIndex);

            int GetCategoryIndex(BackgroundModel background)
            {
                var categoryIndex = backgroundCategoryDropdown.IndexOf(background.Category);

                return categoryIndex < 0 ? 0 : categoryIndex;
            }

            int GetBackgroundIndex(BackgroundModel currentBackground)
            {
                var backgroundIndex = backgroundDropdown.IndexOf(currentBackground);

                return backgroundIndex < 0 ? 0 : backgroundIndex;
            }
        }
    }

    private void OnSearchSelected(object sender, SearchBarSelectionEventArgs<BackgroundModel> e) =>
        backgroundService.ChangeBackground(e.Item);

    private void OnChangedBackground(object sender, EventArgs e) =>
        backgroundService.ChangeBackground(backgroundDropdown.SelectedItem);
}

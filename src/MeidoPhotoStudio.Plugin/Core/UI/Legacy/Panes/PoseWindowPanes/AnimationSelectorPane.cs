using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Database.Character;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class AnimationSelectorPane : BasePane
{
    private const int GameAnimation = 0;
    private const int CustomAnimation = 1;

    private static readonly string[] AnimationSourceTranslationKeys = ["baseTab", "customTab"];

    private readonly SelectionGrid animationSourceGrid;
    private readonly Dropdown<string> animationCategoryDropdown;
    private readonly Dropdown<IAnimationModel> animationDropdown;
    private readonly GameAnimationRepository gameAnimationRepository;
    private readonly CustomAnimationRepository customAnimationRepository;
    private readonly CharacterUndoRedoService characterUndoRedoService;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly CustomAnimationRepositorySorter customAnimationRepositorySorter;
    private readonly PaneHeader paneHeader;
    private readonly Framework.UI.Legacy.ComboBox animationCategoryComboBox;
    private readonly TextField animationNameTextField;
    private readonly Toggle saveAnimationToggle;
    private readonly Button savePoseButton;
    private readonly Label initializingLabel;
    private readonly Label noAnimationsLabel;
    private readonly Header animationDirectoryHeader;
    private readonly Header animationFilenameHeader;
    private readonly Button refreshButton;
    private readonly Label savedAnimationLabel;
    private readonly SearchBar<IAnimationModel> searchBar;

    private bool showSavedAnimationLabel;
    private float saveTime;

    public AnimationSelectorPane(
        GameAnimationRepository gameAnimationRepository,
        CustomAnimationRepository customAnimationRepository,
        CharacterUndoRedoService characterUndoRedoService,
        SelectionController<CharacterController> characterSelectionController,
        CustomAnimationRepositorySorter customAnimationRepositorySorter)
    {
        this.gameAnimationRepository = gameAnimationRepository ?? throw new ArgumentNullException(nameof(gameAnimationRepository));
        this.customAnimationRepository = customAnimationRepository ?? throw new ArgumentNullException(nameof(customAnimationRepository));
        this.characterUndoRedoService = characterUndoRedoService ?? throw new ArgumentNullException(nameof(characterUndoRedoService));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));
        this.customAnimationRepositorySorter = customAnimationRepositorySorter ?? throw new ArgumentNullException(nameof(customAnimationRepositorySorter));

        this.customAnimationRepository.AddedAnimation += OnAnimationAdded;
        this.customAnimationRepository.Refreshed += OnCustomAnimationRepositoryRefreshed;
        this.characterSelectionController.Selecting += OnCharacterSelectionChanging;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        animationSourceGrid = new(Translation.GetArray("posePane", AnimationSourceTranslationKeys));
        animationSourceGrid.ControlEvent += OnAnimationSourceChanged;

        searchBar = new(SearchSelector, Formatter)
        {
            Placeholder = Translation.Get("posePane", "searchBarPlaceholder"),
        };

        searchBar.SelectedValue += OnSearchSelected;

        animationCategoryDropdown = new(
            GetAnimationCategoryFormatter(animationSourceGrid.SelectedItemIndex is CustomAnimation));

        animationCategoryDropdown.SelectionChanged += OnAnimationCategoryChanged;

        animationDropdown = new(
            AnimationList(animationSourceGrid.SelectedItemIndex is CustomAnimation), formatter: Formatter);

        animationDropdown.SelectionChanged += OnAnimationChanged;

        paneHeader = new(Translation.Get("posePane", "header"), true);

        saveAnimationToggle = new(Translation.Get("posePane", "saveToggle"), false);
        animationCategoryComboBox = new(this.customAnimationRepository.Categories)
        {
            Placeholder = Translation.Get("posePane", "categorySearchBarPlaceholder"),
        };

        animationNameTextField = new()
        {
            Placeholder = Translation.Get("posePane", "nameTextFieldPlaceholder"),
        };

        savePoseButton = new(Translation.Get("posePane", "saveButton"));
        savePoseButton.ControlEvent += OnSavePoseButtonPushed;

        refreshButton = new(Translation.Get("posePane", "refreshButton"));
        refreshButton.ControlEvent += OnRefreshButtonPushed;

        animationDirectoryHeader = new(Translation.Get("posePane", "categoryHeader"));
        animationFilenameHeader = new(Translation.Get("posePane", "nameHeader"));

        initializingLabel = new(Translation.Get("systemMessage", "initializing"));
        noAnimationsLabel = new(Translation.Get("posePane", "noAnimations"));

        savedAnimationLabel = new(Translation.Get("posePane", "savedAnimationLabel"));

        if (gameAnimationRepository.Busy)
            gameAnimationRepository.InitializedAnimations += OnGameAnimationsRepositoryReady;
        else
            InitializeGameAnimations();

        void OnGameAnimationsRepositoryReady(object sender, EventArgs e)
        {
            InitializeGameAnimations();

            gameAnimationRepository.InitializedAnimations -= OnGameAnimationsRepositoryReady;
        }

        void InitializeGameAnimations()
        {
            if (animationSourceGrid.SelectedItemIndex is CustomAnimation)
                return;

            animationCategoryDropdown.SetItemsWithoutNotify(AnimationCategoryList(custom: false), 0);
            animationCategoryDropdown.Formatter = GetAnimationCategoryFormatter(custom: false);

            animationDropdown.SetItemsWithoutNotify(AnimationList(custom: false), 0);
        }

        IDropdownItem Formatter(IAnimationModel model, int index) =>
            new LabelledDropdownItem($"{index + 1}: {model.Name}");

        IEnumerable<IAnimationModel> SearchSelector(string query)
        {
            var repository = animationSourceGrid.SelectedItemIndex is GameAnimation
                ? gameAnimationRepository.Cast<IAnimationModel>()
                : customAnimationRepository.Cast<IAnimationModel>();

            return repository.Where(Selector);

            bool Selector(IAnimationModel model)
            {
                var search = model.Custom ? model.Name : model.Filename;

                return search.Contains(query, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    private CharacterController Character =>
        characterSelectionController.Current;

    private AnimationController CurrentAnimation =>
        characterSelectionController.Current?.Animation;

    public override void Draw()
    {
        var enabled = characterSelectionController.Current is not null;

        GUI.enabled = enabled;

        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        animationSourceGrid.Draw();
        MpsGui.BlackLine();

        DrawTextFieldWithScrollBarOffset(searchBar);

        GUI.enabled = enabled;

        if (animationSourceGrid.SelectedItemIndex is GameAnimation && gameAnimationRepository.Busy)
            initializingLabel.Draw();
        else
            DrawDropdowns();

        MpsGui.BlackLine();

        GUILayout.BeginHorizontal();

        saveAnimationToggle.Draw();

        if (animationSourceGrid.SelectedItemIndex is CustomAnimation)
        {
            GUILayout.FlexibleSpace();

            refreshButton.Draw();
        }

        GUILayout.EndHorizontal();

        if (saveAnimationToggle.Value)
            DrawAddAnimation();

        void DrawDropdowns()
        {
            if (!animationCategoryDropdown.Any())
            {
                noAnimationsLabel.Draw();
            }
            else if (!animationDropdown.Any())
            {
                DrawDropdown(animationCategoryDropdown);

                noAnimationsLabel.Draw();
            }
            else
            {
                DrawDropdown(animationCategoryDropdown);
                DrawDropdown(animationDropdown);
            }
        }

        void DrawAddAnimation()
        {
            animationDirectoryHeader.Draw();
            DrawComboBox(animationCategoryComboBox);

            animationFilenameHeader.Draw();
            DrawTextFieldWithScrollBarOffset(animationNameTextField);

            MpsGui.BlackLine();

            savePoseButton.Draw();

            if (!showSavedAnimationLabel)
                return;

            if (Time.time - saveTime >= 2.5f)
            {
                showSavedAnimationLabel = false;

                return;
            }

            savedAnimationLabel.Draw();
        }
    }

    protected override void ReloadTranslation()
    {
        animationSourceGrid.SetItemsWithoutNotify(Translation.GetArray("posePane", AnimationSourceTranslationKeys));

        if (animationSourceGrid.SelectedItemIndex is GameAnimation)
            animationCategoryDropdown.Reformat();

        paneHeader.Label = Translation.Get("posePane", "header");
        saveAnimationToggle.Label = Translation.Get("posePane", "saveToggle");
        savePoseButton.Label = Translation.Get("posePane", "saveButton");
        refreshButton.Label = Translation.Get("posePane", "refreshButton");
        animationCategoryComboBox.Placeholder = Translation.Get("posePane", "categorySearchBarPlaceholder");
        animationNameTextField.Placeholder = Translation.Get("posePane", "nameTextFieldPlaceholder");
        animationDirectoryHeader.Text = Translation.Get("posePane", "categoryHeader");
        animationFilenameHeader.Text = Translation.Get("posePane", "nameHeader");
        initializingLabel.Text = Translation.Get("systemMessage", "initializing");
        noAnimationsLabel.Text = Translation.Get("posePane", "noAnimations");
        savedAnimationLabel.Text = Translation.Get("posePane", "savedAnimationLabel");
        searchBar.Placeholder = Translation.Get("posePane", "searchBarPlaceholder");
        searchBar.Reformat();
    }

    private static Func<string, int, IDropdownItem> GetAnimationCategoryFormatter(bool custom)
    {
        return custom ? CustomAnimationCategoryFormatter : GameAnimationCategoryFormatter;

        static LabelledDropdownItem CustomAnimationCategoryFormatter(string category, int index) =>
            new(category);

        static LabelledDropdownItem GameAnimationCategoryFormatter(string category, int index) =>
            new(Translation.Get("poseGroupDropdown", category));
    }

    private void OnAnimationAdded(object sender, AddedAnimationEventArgs e)
    {
        if (characterSelectionController.Current is null)
            return;

        if (!animationCategoryComboBox.Contains(e.Animation.Category))
            animationCategoryComboBox.SetItems(customAnimationRepository.Categories);

        if (animationSourceGrid.SelectedItemIndex is not CustomAnimation)
            return;

        var currentCategory = animationCategoryDropdown.SelectedItem;

        if (!animationCategoryDropdown.Contains(e.Animation.Category))
            animationCategoryDropdown.SetItemsWithoutNotify(AnimationCategoryList(e.Animation.Custom));

        var currentCategoryIndex = animationCategoryDropdown.FindIndex(
            category => string.Equals(category, currentCategory, StringComparison.Ordinal));

        animationCategoryDropdown.SetSelectedIndexWithoutNotify(currentCategoryIndex);

        if (!string.Equals(e.Animation.Category, currentCategory, StringComparison.Ordinal))
            return;

        var currentAnimation = animationDropdown.SelectedItem;

        animationDropdown.SetItemsWithoutNotify(AnimationList(e.Animation.Custom));

        var currentAnimationIndex = animationDropdown.FindIndex(
            animation => animation.Equals(currentAnimation));

        animationDropdown.SetSelectedIndexWithoutNotify(currentAnimationIndex);
    }

    private void OnCustomAnimationRepositoryRefreshed(object sender, EventArgs e)
    {
        if (animationSourceGrid.SelectedItemIndex is not CustomAnimation)
            return;

        if (customAnimationRepository.ContainsCategory(animationCategoryDropdown.SelectedItem))
        {
            var currentCategory = animationCategoryDropdown.SelectedItem;
            var newCategories = AnimationCategoryList(custom: true).ToArray();

            animationCategoryComboBox.SetItems(newCategories);

            var categoryIndex = newCategories.FindIndex(category => string.Equals(currentCategory, category, StringComparison.Ordinal));

            if (categoryIndex < 0)
            {
                animationCategoryDropdown.SetItemsWithoutNotify(newCategories, 0);
                animationDropdown.SetItemsWithoutNotify(AnimationList(custom: true), 0);

                return;
            }

            animationCategoryDropdown.SetItemsWithoutNotify(newCategories, categoryIndex);

            var currentAnimationModel = animationDropdown.SelectedItem;

            var newAnimations = AnimationList(custom: true).ToArray();
            var animationIndex = newAnimations.FindIndex(animation => animation == currentAnimationModel);

            if (animationIndex < 0)
                animationIndex = 0;

            animationDropdown.SetItemsWithoutNotify(newAnimations, animationIndex);
        }
        else
        {
            var newCategories = AnimationCategoryList(custom: true).ToArray();

            animationCategoryDropdown.SetItems(newCategories, 0);
            animationCategoryComboBox.SetItems(newCategories);
        }
    }

    private void OnAnimationSourceChanged(object sender, EventArgs e)
    {
        var custom = animationSourceGrid.SelectedItemIndex is CustomAnimation;

        animationCategoryDropdown.SetItemsWithoutNotify(AnimationCategoryList(custom), 0);
        animationCategoryDropdown.Formatter = GetAnimationCategoryFormatter(custom);

        animationDropdown.SetItemsWithoutNotify(AnimationList(custom), 0);

        if (animationDropdown.SelectedItem is not null)
            ChangeAnimation(animationDropdown.SelectedItem);

        searchBar.ClearQuery();
    }

    private void OnAnimationCategoryChanged(object sender, DropdownEventArgs<string> e)
    {
        if (e.PreviousSelectedItemIndex == e.SelectedItemIndex)
            animationDropdown.SelectedItemIndex = 0;
        else
            animationDropdown.SetItems(AnimationList(animationSourceGrid.SelectedItemIndex is CustomAnimation), 0);
    }

    private void OnCharacterSelectionChanging(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        e.Selected.Animation.PropertyChanged -= OnAnimationPropertyChanged;
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        e.Selected.Animation.PropertyChanged += OnAnimationPropertyChanged;

        UpdatePanel(CurrentAnimation.Animation);
    }

    private void OnRefreshButtonPushed(object sender, EventArgs e) =>
        customAnimationRepository.Refresh();

    private void OnSavePoseButtonPushed(object sender, EventArgs e)
    {
        if (characterSelectionController.Current is null)
            return;

        var pose = characterSelectionController.Current.IK.GetAnimationData();
        var category = animationCategoryComboBox.Value;
        var name = animationNameTextField.Value;

        if (string.IsNullOrEmpty(category))
            category = customAnimationRepository.RootCategoryName;

        if (string.IsNullOrEmpty(name))
            name = "custom_pose";

        customAnimationRepository.Add(pose, category, name);

        animationNameTextField.Value = string.Empty;

        showSavedAnimationLabel = true;
        saveTime = Time.time;
    }

    private void UpdatePanel(IAnimationModel animation)
    {
        var animationSource = animation.Custom ? CustomAnimation : GameAnimation;

        if (animationSource is GameAnimation && gameAnimationRepository.Busy)
            return;

        if (animationSource != animationSourceGrid.SelectedItemIndex)
        {
            animationSourceGrid.SetValueWithoutNotify(animationSource);

            animationCategoryDropdown.SetItemsWithoutNotify(AnimationCategoryList(animation.Custom));
            animationCategoryDropdown.Formatter = GetAnimationCategoryFormatter(animation.Custom);

            var categoryIndex = GetCategoryIndex(animation);

            animationCategoryDropdown.SetSelectedIndexWithoutNotify(categoryIndex);

            var newAnimationList = AnimationList(animation.Custom);
            var animationIndex = GetAnimationIndex(newAnimationList, animation);

            animationDropdown.SetItemsWithoutNotify(newAnimationList, animationIndex);
        }
        else
        {
            var categoryIndex = GetCategoryIndex(animation);
            var oldCategoryIndex = animationCategoryDropdown.SelectedItemIndex;

            animationCategoryDropdown.SetSelectedIndexWithoutNotify(categoryIndex);

            if (categoryIndex != oldCategoryIndex)
                animationDropdown.SetItemsWithoutNotify(AnimationList(animation.Custom));

            var animationIndex = GetAnimationIndex(animationDropdown, animation);

            animationDropdown.SetSelectedIndexWithoutNotify(animationIndex);
        }

        int GetCategoryIndex(IAnimationModel currentAnimation)
        {
            var categoryIndex = animationCategoryDropdown.FindIndex(category =>
                string.Equals(category, currentAnimation.Category, StringComparison.OrdinalIgnoreCase));

            return categoryIndex < 0 ? 0 : categoryIndex;
        }

        int GetAnimationIndex(IEnumerable<IAnimationModel> animationList, IAnimationModel currentAnimation)
        {
            if (currentAnimation.Custom)
            {
                var customAnimation = (CustomAnimationModel)currentAnimation;

                return customAnimationRepository.ContainsCategory(currentAnimation.Category)
                    ? animationList
                        .Cast<CustomAnimationModel>()
                        .FindIndex(animation => animation.ID == customAnimation.ID)
                    : 0;
            }
            else
            {
                var gameAnimation = (GameAnimationModel)currentAnimation;

                return gameAnimationRepository.ContainsCategory(currentAnimation.Category)
                    ? animationList
                        .Cast<GameAnimationModel>()
                        .FindIndex(animation => string.Equals(animation.ID, gameAnimation.ID, StringComparison.OrdinalIgnoreCase))
                    : 0;
            }
        }
    }

    private void OnAnimationChanged(object sender, DropdownEventArgs<IAnimationModel> e)
    {
        if (animationSourceGrid.SelectedItemIndex is GameAnimation && gameAnimationRepository.Busy)
            return;

        if (e.Item is null)
            return;

        ChangeAnimation(e.Item);
    }

    private void OnSearchSelected(object sender, SearchBarSelectionEventArgs<IAnimationModel> e)
    {
        if (e.Item is null)
            return;

        ChangeAnimation(e.Item);
    }

    private void ChangeAnimation(IAnimationModel animation)
    {
        if (characterSelectionController.Current is not CharacterController character)
            return;

        if (Character.IK.Dirty)
        {
            characterUndoRedoService[Character].StartPoseChange();
            character.Animation.Apply(animation);
            characterUndoRedoService[Character].EndPoseChange();
        }
        else
        {
            character.Animation.Apply(animation);
        }
    }

    private void OnAnimationPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not nameof(AnimationController.Animation))
            return;

        var controller = (AnimationController)sender;

        if (controller.Animation.Equals(animationDropdown.SelectedItem))
            return;

        UpdatePanel(controller.Animation);
    }

    private IEnumerable<string> AnimationCategoryList(bool custom) =>
        custom ? customAnimationRepositorySorter.GetCategories(customAnimationRepository) :
        gameAnimationRepository.Busy ? [] :
        gameAnimationRepository.Categories;

    private IEnumerable<IAnimationModel> AnimationList(bool custom)
    {
        return custom ? GetCustomAnimtions() : GetGameAnimations();

        IEnumerable<IAnimationModel> GetGameAnimations() =>
            gameAnimationRepository.Busy || !animationCategoryDropdown.Any() ? [] :
            gameAnimationRepository[animationCategoryDropdown.SelectedItem].Cast<IAnimationModel>();

        IEnumerable<IAnimationModel> GetCustomAnimtions() =>
            animationCategoryDropdown.Any() ? customAnimationRepositorySorter.GetAnimations(
                animationCategoryDropdown.SelectedItem, customAnimationRepository)
                .Cast<IAnimationModel>() :
            [];
    }
}

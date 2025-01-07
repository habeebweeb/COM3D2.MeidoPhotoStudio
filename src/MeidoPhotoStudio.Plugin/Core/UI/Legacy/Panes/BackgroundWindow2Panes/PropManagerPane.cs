using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.Service;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class PropManagerPane : BasePane
{
    private static readonly string[] GizmoSpaceTranslationKeys = ["gizmoSpaceLocal", "gizmoSpaceWorld"];

    private readonly PropService propService;
    private readonly FavouritePropRepository favouritePropRepository;
    private readonly PropDragHandleService propDragHandleService;
    private readonly SelectionController<PropController> propSelectionController;
    private readonly TransformClipboard transformClipboard;
    private readonly Dropdown<PropController> propDropdown;
    private readonly Dictionary<PropController, string> propNames = [];
    private readonly Toggle dragPointToggle;
    private readonly Toggle gizmoToggle;
    private readonly Toggle shadowCastingToggle;
    private readonly Toggle visibleToggle;
    private readonly Button deletePropButton;
    private readonly Button copyPropButton;
    private readonly SelectionGrid gizmoMode;
    private readonly TransformInputPane transformInputPane;
    private readonly Button focusButton;
    private readonly Button addToFavouritesButton;
    private readonly Button removeFromFavouritesButton;
    private readonly PaneHeader paneHeader;
    private readonly Toggle toggleAllDragHandles;
    private readonly Toggle toggleAllGizmos;
    private readonly Label gizmoSpaceLabel;
    private readonly Header toggleAllHandlesHeader;
    private readonly Label noPropsLabel;

    private bool isFavouriteProp;

    public PropManagerPane(
        PropService propService,
        FavouritePropRepository favouritePropRepository,
        PropDragHandleService propDragHandleService,
        SelectionController<PropController> propSelectionController,
        TransformClipboard transformClipboard)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.favouritePropRepository = favouritePropRepository ?? throw new ArgumentNullException(nameof(favouritePropRepository));
        this.propDragHandleService = propDragHandleService ?? throw new ArgumentNullException(nameof(propDragHandleService));
        this.propSelectionController = propSelectionController ?? throw new ArgumentNullException(nameof(propSelectionController));
        this.transformClipboard = transformClipboard ?? throw new ArgumentNullException(nameof(transformClipboard));

        this.propService.AddedProp += OnAddedProp;
        this.propService.RemovedProp += OnRemovedProp;
        this.favouritePropRepository.AddedFavouriteProp += OnFavouritePropAddedOrRemoved;
        this.favouritePropRepository.RemovedFavouriteProp += OnFavouritePropAddedOrRemoved;
        this.favouritePropRepository.Refreshed += OnFavouritePropRepositoryRefreshed;
        this.propSelectionController.Selecting += OnSelectingProp;
        this.propSelectionController.Selected += OnSelectedProp;

        propDropdown = new(formatter: PropNameFormatter);
        propDropdown.SelectionChanged += OnPropDropdownSelectionChange;

        dragPointToggle = new(Translation.Get("propManagerPane", "dragPointToggle"));
        dragPointToggle.ControlEvent += OnDragPointToggleChanged;

        gizmoToggle = new(Translation.Get("propManagerPane", "gizmoToggle"));
        gizmoToggle.ControlEvent += OnGizmoToggleChanged;

        shadowCastingToggle = new(Translation.Get("propManagerPane", "shadowCastingToggle"));
        shadowCastingToggle.ControlEvent += OnShadowCastingToggleChanged;

        visibleToggle = new(Translation.Get("propManagerPane", "visibleToggle"), true);
        visibleToggle.ControlEvent += OnVisibleToggleChanged;

        copyPropButton = new(Translation.Get("propManagerPane", "copyButton"));
        copyPropButton.ControlEvent += OnCopyButtonPressed;

        deletePropButton = new(Translation.Get("propManagerPane", "deleteButton"));
        deletePropButton.ControlEvent += OnDeleteButtonPressed;

        gizmoMode = new(Translation.GetArray("propManagerPane", GizmoSpaceTranslationKeys));
        gizmoMode.ControlEvent += OnGizmoModeToggleChanged;

        focusButton = new(Translation.Get("propManagerPane", "focusPropButton"));
        focusButton.ControlEvent += OnFocusButtonPushed;

        toggleAllDragHandles = new(Translation.Get("propManagerPane", "allDragHandleToggle"), true);
        toggleAllDragHandles.ControlEvent += OnToggleAllDragHandlesChanged;

        toggleAllGizmos = new(Translation.Get("propManagerPane", "allGizmoToggle"), true);
        toggleAllGizmos.ControlEvent += OnToggleAllGizmosChanged;

        gizmoSpaceLabel = new(Translation.Get("propManagerPane", "gizmoSpaceToggle"));

        addToFavouritesButton = new(Translation.Get("propManagerPane", "addFavouriteButton"));
        addToFavouritesButton.ControlEvent += OnAddFavouritePropButtonPushed;

        removeFromFavouritesButton = new(Translation.Get("propManagerPane", "removeFavouriteButton"));
        removeFromFavouritesButton.ControlEvent += OnRemoveFavouritePropButtonPushed;

        transformInputPane = new(this.transformClipboard);
        Add(transformInputPane);

        toggleAllHandlesHeader = new(Translation.Get("propManagerPane", "toggleAllHandlesHeader"));
        paneHeader = new(Translation.Get("propManagerPane", "header"), true);

        noPropsLabel = new(Translation.Get("propManagerPane", "noProps"));

        LabelledDropdownItem PropNameFormatter(PropController prop, int index) =>
            new(propNames[prop]);
    }

    private PropController CurrentProp =>
        propSelectionController.Current;

    public override void Draw()
    {
        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        if (propService.Count is 0)
        {
            noPropsLabel.Draw();

            return;
        }

        DrawDropdown(propDropdown);

        UIUtility.DrawBlackLine();

        var noExpandWidth = GUILayout.ExpandWidth(false);

        GUILayout.BeginHorizontal();
        dragPointToggle.Draw(noExpandWidth);
        GUILayout.FlexibleSpace();
        focusButton.Draw(noExpandWidth);
        copyPropButton.Draw(noExpandWidth);
        deletePropButton.Draw(noExpandWidth);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        gizmoToggle.Draw(noExpandWidth);
        GUILayout.FlexibleSpace();

        var guiEnabled = Parent.Enabled;

        GUI.enabled = guiEnabled && gizmoToggle.Value;

        gizmoSpaceLabel.Draw();
        gizmoMode.Draw();

        GUI.enabled = guiEnabled;

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        visibleToggle.Draw(noExpandWidth);

        GUI.enabled = guiEnabled && visibleToggle.Value;
        shadowCastingToggle.Draw(noExpandWidth);
        GUI.enabled = guiEnabled;

        GUILayout.EndHorizontal();

        UIUtility.DrawBlackLine();

        if (isFavouriteProp)
            removeFromFavouritesButton.Draw();
        else
            addToFavouritesButton.Draw();

        toggleAllHandlesHeader.Draw();
        UIUtility.DrawBlackLine();

        GUILayout.BeginHorizontal();

        toggleAllDragHandles.Draw();
        toggleAllGizmos.Draw();

        GUILayout.EndHorizontal();

        UIUtility.DrawBlackLine();

        transformInputPane.Draw();
    }

    protected override void ReloadTranslation()
    {
        dragPointToggle.Label = Translation.Get("propManagerPane", "dragPointToggle");
        gizmoToggle.Label = Translation.Get("propManagerPane", "gizmoToggle");
        shadowCastingToggle.Label = Translation.Get("propManagerPane", "shadowCastingToggle");
        visibleToggle.Label = Translation.Get("propManagerPane", "visibleToggle");
        copyPropButton.Label = Translation.Get("propManagerPane", "copyButton");
        deletePropButton.Label = Translation.Get("propManagerPane", "deleteButton");
        gizmoMode.SetItemsWithoutNotify(Translation.GetArray("propManagerPane", GizmoSpaceTranslationKeys));
        focusButton.Label = Translation.Get("propManagerPane", "focusPropButton");
        toggleAllDragHandles.Label = Translation.Get("propManagerPane", "allDragHandleToggle");
        toggleAllGizmos.Label = Translation.Get("propManagerPane", "allGizmoToggle");
        gizmoSpaceLabel.Text = Translation.Get("propManagerPane", "gizmoSpaceToggle");

        toggleAllHandlesHeader.Text = Translation.Get("propManagerPane", "toggleAllHandlesHeader");
        paneHeader.Label = Translation.Get("propManagerPane", "header");
        noPropsLabel.Text = Translation.Get("propManagerPane", "noProps");

        addToFavouritesButton.Label = Translation.Get("propManagerPane", "addFavouriteButton");
        removeFromFavouritesButton.Label = Translation.Get("propManagerPane", "removeFavouriteButton");
    }

    private void UpdateControls()
    {
        transformInputPane.Target = CurrentProp;

        if (CurrentProp is null)
            return;

        shadowCastingToggle.SetEnabledWithoutNotify(CurrentProp.ShadowCasting);
        visibleToggle.SetEnabledWithoutNotify(CurrentProp.Visible);

        var dragHandleController = propDragHandleService[CurrentProp];

        dragPointToggle.SetEnabledWithoutNotify(dragHandleController.Enabled);
        gizmoToggle.SetEnabledWithoutNotify(dragHandleController.GizmoEnabled);
        gizmoMode.SetValueWithoutNotify((int)dragHandleController.GizmoMode);
    }

    private void OnToggleAllDragHandlesChanged(object sender, EventArgs e)
    {
        foreach (var controller in propDragHandleService)
            controller.Enabled = toggleAllDragHandles.Value;
    }

    private void OnToggleAllGizmosChanged(object sender, EventArgs e)
    {
        foreach (var controller in propDragHandleService)
            controller.GizmoEnabled = toggleAllGizmos.Value;
    }

    private void OnAddedProp(object sender, PropServiceEventArgs e)
    {
        propNames[e.PropController] = UniquePropName(new(propNames.Values), e.PropController.PropModel);
        propDropdown.SetItems(propService, propService.Count - 1);

        static string UniquePropName(HashSet<string> currentNames, IPropModel propModel)
        {
            var propName = PropName(propModel);
            var newPropName = propName;
            var index = 1;

            while (currentNames.Contains(newPropName))
            {
                index++;
                newPropName = $"{propName} ({index})";
            }

            return newPropName;
        }

        static string PropName(IPropModel propModel) =>
            propModel.Name;
    }

    private void OnRemovedProp(object sender, PropServiceEventArgs e)
    {
        if (propService.Count is 0)
        {
            propDropdown.Clear();
            propNames.Clear();

            return;
        }

        var propIndex = propDropdown.SelectedItemIndex >= propService.Count
            ? propService.Count - 1
            : propDropdown.SelectedItemIndex;

        propNames.Remove(e.PropController);
        propDropdown.SetItems(propService, propIndex);
    }

    private void OnSelectingProp(object sender, SelectionEventArgs<PropController> e)
    {
        if (e.Selected is not PropController prop)
            return;

        prop.PropertyChanged -= OnPropPropertyChanged;

        var dragHandleController = propDragHandleService[prop];

        dragHandleController.PropertyChanged -= OnDragHandlePropertyChanged;
    }

    private void OnSelectedProp(object sender, SelectionEventArgs<PropController> e)
    {
        if (e.Selected is not PropController prop)
            return;

        prop.PropertyChanged += OnPropPropertyChanged;

        var dragHandleController = propDragHandleService[prop];

        dragHandleController.PropertyChanged += OnDragHandlePropertyChanged;

        propDropdown.SetSelectedIndexWithoutNotify(e.Index);

        isFavouriteProp = favouritePropRepository.ContainsProp(prop.PropModel);

        UpdateControls();
    }

    private void OnPropPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var prop = (PropController)sender;

        if (e.PropertyName is nameof(PropController.ShadowCasting))
            shadowCastingToggle.SetEnabledWithoutNotify(prop.ShadowCasting);
        else if (e.PropertyName is nameof(PropController.Visible))
            visibleToggle.SetEnabledWithoutNotify(prop.Visible);
    }

    private void OnDragHandlePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var controller = (PropDragHandleController)sender;

        if (e.PropertyName is nameof(PropDragHandleController.Enabled))
            dragPointToggle.SetEnabledWithoutNotify(controller.Enabled);
        else if (e.PropertyName is nameof(PropDragHandleController.GizmoMode))
            gizmoMode.SetValueWithoutNotify((int)controller.GizmoMode);
        else if (e.PropertyName is nameof(PropDragHandleController.GizmoEnabled))
            gizmoToggle.SetEnabledWithoutNotify(controller.GizmoEnabled);
    }

    private void OnPropDropdownSelectionChange(object sender, EventArgs e)
    {
        if (propService.Count is 0)
            return;

        propSelectionController.Select(propDropdown.SelectedItem);
    }

    private void OnDragPointToggleChanged(object sender, EventArgs e)
    {
        var controller = propDragHandleService[CurrentProp];

        controller.Enabled = dragPointToggle.Value;
    }

    private void OnGizmoToggleChanged(object sender, EventArgs e)
    {
        var controller = propDragHandleService[CurrentProp];

        controller.GizmoEnabled = gizmoToggle.Value;
    }

    private void OnShadowCastingToggleChanged(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        CurrentProp.ShadowCasting = shadowCastingToggle.Value;
    }

    private void OnVisibleToggleChanged(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        CurrentProp.Visible = visibleToggle.Value;
    }

    private void OnCopyButtonPressed(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        propService.Clone(propService.IndexOf(CurrentProp));
    }

    private void OnDeleteButtonPressed(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        propService.Remove(propService.IndexOf(CurrentProp));
    }

    private void OnGizmoModeToggleChanged(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        var controller = propDragHandleService[CurrentProp];

        controller.GizmoMode = (CustomGizmo.GizmoMode)gizmoMode.SelectedItemIndex;
    }

    private void OnFocusButtonPushed(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        CurrentProp.Focus();
    }

    private void OnFavouritePropAddedOrRemoved(object sender, FavouritePropRepositoryEventArgs e)
    {
        if (CurrentProp is null)
            return;

        if (e.FavouriteProp.PropModel != CurrentProp.PropModel)
            return;

        isFavouriteProp = favouritePropRepository.ContainsProp(e.FavouriteProp.PropModel);
    }

    private void OnFavouritePropRepositoryRefreshed(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        isFavouriteProp = favouritePropRepository.ContainsProp(CurrentProp.PropModel);
    }

    private void OnAddFavouritePropButtonPushed(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        if (favouritePropRepository.ContainsProp(CurrentProp.PropModel))
            return;

        favouritePropRepository.Add(CurrentProp.PropModel);
    }

    private void OnRemoveFavouritePropButtonPushed(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        if (!favouritePropRepository.ContainsProp(CurrentProp.PropModel))
            return;

        favouritePropRepository.Remove(CurrentProp.PropModel);
    }
}

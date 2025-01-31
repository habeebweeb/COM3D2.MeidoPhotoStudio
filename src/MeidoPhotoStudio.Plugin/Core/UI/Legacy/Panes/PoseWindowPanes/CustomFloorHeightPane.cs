using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class CustomFloorHeightPane : BasePane
{
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly Toggle customFloorHeightToggle;
    private readonly RepeatButton decreaseFloorHeightButton;
    private readonly RepeatButton increaseFloorHeightButton;
    private readonly NumericalTextField floorHeightTextfield;
    private readonly Button resetFloorHeightButton;

    public CustomFloorHeightPane(Translation translation, SelectionController<CharacterController> characterSelectionController)
    {
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));
        this.characterSelectionController.Selecting += OnCharacterSelectionChanging;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        customFloorHeightToggle = new(
            new LocalizableGUIContent(translation, "maidPoseWindow", "customFloorHeightEnabledToggle"), false);

        customFloorHeightToggle.ControlEvent += OnCustomFloorHeightToggleChanged;

        decreaseFloorHeightButton = new("<", 3f);
        decreaseFloorHeightButton.ControlEvent += OnDecreaseFloorHeightButtonPushed;

        increaseFloorHeightButton = new(">", 3f);
        increaseFloorHeightButton.ControlEvent += OnIncreaseFloorHeightButtonPushed;

        floorHeightTextfield = new(0f);
        floorHeightTextfield.ControlEvent += OnFloorHeightChanged;

        resetFloorHeightButton = new("|");
        resetFloorHeightButton.ControlEvent += OnResetFloorHeightButtonPushed;
    }

    private CharacterController CurrentCharacter =>
        characterSelectionController.Current;

    public override void Draw()
    {
        var enabled = Parent.Enabled && characterSelectionController.Current is not null;

        GUI.enabled = enabled;

        GUILayout.BeginHorizontal();

        var noExpandWidth = GUILayout.ExpandWidth(false);

        customFloorHeightToggle.Draw(noExpandWidth);

        GUI.enabled = enabled && customFloorHeightToggle.Value;

        decreaseFloorHeightButton.Draw(noExpandWidth);
        increaseFloorHeightButton.Draw(noExpandWidth);

        floorHeightTextfield.Draw(GUILayout.Width(70f));

        resetFloorHeightButton.Draw(GUILayout.ExpandWidth(false));

        GUILayout.EndHorizontal();
    }

    private void OnCharacterSelectionChanging(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        var clothing = e.Selected.Clothing;

        clothing.PropertyChanged -= OnClothingPropertyChanged;
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        var clothing = e.Selected.Clothing;

        clothing.PropertyChanged += OnClothingPropertyChanged;

        customFloorHeightToggle.SetEnabledWithoutNotify(clothing.CustomFloorHeight);
        floorHeightTextfield.SetValueWithoutNotify(clothing.FloorHeight);
    }

    private void OnCustomFloorHeightToggleChanged(object sender, EventArgs e)
    {
        if (CurrentCharacter is null)
            return;

        CurrentCharacter.Clothing.CustomFloorHeight = customFloorHeightToggle.Value;
    }

    private void OnIncreaseFloorHeightButtonPushed(object sender, EventArgs e)
    {
        if (CurrentCharacter is null)
            return;

        if (!CurrentCharacter.Clothing.CustomFloorHeight)
            return;

        CurrentCharacter.Clothing.FloorHeight += 0.01f;
    }

    private void OnDecreaseFloorHeightButtonPushed(object sender, EventArgs e)
    {
        if (CurrentCharacter is null)
            return;

        if (!CurrentCharacter.Clothing.CustomFloorHeight)
            return;

        CurrentCharacter.Clothing.FloorHeight -= 0.01f;
    }

    private void OnFloorHeightChanged(object sender, EventArgs e)
    {
        if (CurrentCharacter is null)
            return;

        CurrentCharacter.Clothing.FloorHeight = floorHeightTextfield.Value;
    }

    private void OnResetFloorHeightButtonPushed(object sender, EventArgs e)
    {
        if (CurrentCharacter is null)
            return;

        CurrentCharacter.Clothing.FloorHeight = 0f;
    }

    private void OnClothingPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var clothingController = (ClothingController)sender;

        if (e.PropertyName is nameof(ClothingController.CustomFloorHeight))
            customFloorHeightToggle.SetEnabledWithoutNotify(clothingController.CustomFloorHeight);
        else if (e.PropertyName is nameof(ClothingController.FloorHeight))
            floorHeightTextfield.SetValueWithoutNotify(clothingController.FloorHeight);
    }
}

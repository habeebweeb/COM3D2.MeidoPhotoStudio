using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class CharacterPlacementPane : BasePane
{
    private readonly PlacementService characterPlacementController;
    private readonly Dropdown<PlacementService.Placement> placementDropdown;
    private readonly Button applyPlacementButton;
    private readonly Header header;

    public CharacterPlacementPane(Translation translation, PlacementService characterPlacementController)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.characterPlacementController = characterPlacementController ?? throw new ArgumentNullException(nameof(characterPlacementController));

        placementDropdown = new(
            Enum.GetValues(typeof(PlacementService.Placement))
                .Cast<PlacementService.Placement>()
                .ToArray(),
            formatter: PlacementTypeFormatter);

        translation.Initialized += OnTranslationInitialized;

        applyPlacementButton = new(new LocalizableGUIContent(translation, "placementPane", "applyButton"));
        applyPlacementButton.ControlEvent += OnPlacementButtonPushed;

        header = new(new LocalizableGUIContent(translation, "placementPane", "header"));

        LabelledDropdownItem PlacementTypeFormatter(PlacementService.Placement placement, int index) =>
            new(translation["placementDropdown", placement.ToLower()]);

        void OnTranslationInitialized(object sender, EventArgs e) =>
            placementDropdown.Reformat();
    }

    public override void Draw()
    {
        header.Draw();
        UIUtility.DrawBlackLine();

        GUILayout.BeginHorizontal();
        placementDropdown.Draw(GUILayout.Width(150));
        applyPlacementButton.Draw();
        GUILayout.EndHorizontal();
    }

    private void OnPlacementButtonPushed(object sender, EventArgs e) =>
        characterPlacementController.ApplyPlacement(placementDropdown.SelectedItem);
}

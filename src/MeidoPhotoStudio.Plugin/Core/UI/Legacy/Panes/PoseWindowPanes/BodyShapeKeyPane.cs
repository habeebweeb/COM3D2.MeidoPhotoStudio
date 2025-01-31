using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class BodyShapeKeyPane : BasePane
{
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly ShapeKeysPane shapeKeysPane;

    public BodyShapeKeyPane(
        Translation translation,
        SelectionController<CharacterController> characterSelectionController,
        BodyShapeKeyConfiguration bodyShapeKeyConfiguration,
        ShapeKeyRangeConfiguration shapeKeyRangeConfiguration)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        _ = bodyShapeKeyConfiguration ?? throw new ArgumentNullException(nameof(bodyShapeKeyConfiguration));
        _ = shapeKeyRangeConfiguration ?? throw new ArgumentNullException(nameof(shapeKeyRangeConfiguration));

        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        shapeKeysPane = new(translation, bodyShapeKeyConfiguration, shapeKeyRangeConfiguration)
        {
            DrawRefreshRangeButton = true,
        };

        Add(shapeKeysPane);
    }

    public override void Draw()
    {
        var guiEnabled = Parent.Enabled;

        GUI.enabled = guiEnabled;

        shapeKeysPane.Draw();
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e) =>
        shapeKeysPane.ShapeKeyController = e.Selected?.Body;
}

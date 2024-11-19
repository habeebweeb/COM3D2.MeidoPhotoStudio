using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class BodyShapeKeyPane : BasePane
{
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly ShapeKeysPane shapeKeysPane;
    private readonly PaneHeader paneHeader;

    public BodyShapeKeyPane(
        SelectionController<CharacterController> characterSelectionController,
        BodyShapeKeyConfiguration bodyShapeKeyConfiguration,
        ShapeKeyRangeConfiguration shapeKeyRangeConfiguration)
    {
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        _ = bodyShapeKeyConfiguration ?? throw new ArgumentNullException(nameof(bodyShapeKeyConfiguration));
        _ = shapeKeyRangeConfiguration ?? throw new ArgumentNullException(nameof(shapeKeyRangeConfiguration));

        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        paneHeader = new(Translation.Get("bodyShapeKeyPane", "header"));

        shapeKeysPane = new(bodyShapeKeyConfiguration, shapeKeyRangeConfiguration)
        {
            DrawRefreshRangeButton = true,
        };

        Add(shapeKeysPane);
    }

    public override void Draw()
    {
        var guiEnabled = Parent.Enabled;

        GUI.enabled = guiEnabled;

        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        shapeKeysPane.Draw();
    }

    protected override void ReloadTranslation() =>
        paneHeader.Label = Translation.Get("bodyShapeKeyPane", "header");

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e) =>
        shapeKeysPane.ShapeKeyController = e.Selected?.Body;
}

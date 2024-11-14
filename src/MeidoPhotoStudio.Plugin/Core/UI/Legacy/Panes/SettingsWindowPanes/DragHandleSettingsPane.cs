using MeidoPhotoStudio.Plugin.Core.Character.Pose;
using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class DragHandleSettingsPane : BasePane
{
    private readonly DragHandleConfiguration configuration;
    private readonly IKDragHandleService ikDragHandleService;
    private readonly PropDragHandleService propDragHandleService;
    private readonly Toggle smallDragHandleToggle;
    private readonly Toggle characterTransformDragHandleToggle;

    public DragHandleSettingsPane(
        DragHandleConfiguration configuration,
        IKDragHandleService ikDragHandleService,
        PropDragHandleService propDragHandleService)
    {
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.ikDragHandleService = ikDragHandleService ?? throw new ArgumentNullException(nameof(ikDragHandleService));
        this.propDragHandleService = propDragHandleService ?? throw new ArgumentNullException(nameof(propDragHandleService));

        this.configuration.SmallTransformCube.SettingChanged += OnSettingsChanged;
        this.configuration.CharacterTransformCube.SettingChanged += OnSettingsChanged;

        smallDragHandleToggle = new(
            Translation.Get("dragHandleSettingsPane", "smallDragHandleToggle"),
            this.configuration.SmallTransformCube.Value);

        smallDragHandleToggle.ControlEvent += OnSmallDragHandleToggleChanged;

        characterTransformDragHandleToggle = new(
            Translation.Get("dragHandleSettingsPane", "characterCubeDragHandleToggle"),
            this.configuration.CharacterTransformCube.Value);

        characterTransformDragHandleToggle.ControlEvent += OnCharacterTransformDragHandleToggleChanged;
    }

    public override void Draw()
    {
        smallDragHandleToggle.Draw();
        characterTransformDragHandleToggle.Draw();
    }

    protected override void ReloadTranslation()
    {
        smallDragHandleToggle.Label = Translation.Get("dragHandleSettingsPane", "smallDragHandleToggle");
        characterTransformDragHandleToggle.Label = Translation.Get("dragHandleSettingsPane", "characterCubeDragHandleToggle");
    }

    private void OnSmallDragHandleToggleChanged(object sender, EventArgs e)
    {
        configuration.SmallTransformCube.Value = smallDragHandleToggle.Value;

        propDragHandleService.SmallHandle = configuration.SmallTransformCube.Value;
        ikDragHandleService.SmallHandle = configuration.SmallTransformCube.Value;
    }

    private void OnCharacterTransformDragHandleToggleChanged(object sender, EventArgs e)
    {
        configuration.CharacterTransformCube.Value = characterTransformDragHandleToggle.Value;

        ikDragHandleService.CubeEnabled = configuration.CharacterTransformCube.Value;
    }

    private void OnSettingsChanged(object sender, EventArgs e)
    {
        smallDragHandleToggle.SetEnabledWithoutNotify(configuration.SmallTransformCube.Value);
        characterTransformDragHandleToggle.SetEnabledWithoutNotify(configuration.CharacterTransformCube.Value);
    }
}

using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class GravityDragHandleController : DragHandleControllerBase
{
    private readonly GravityController gravityController;
    private readonly CharacterController characterController;
    private readonly SelectionController<CharacterController> selectionController;
    private readonly TransformBackup transformBackup;

    private TransformBackup startingTransform;
    private MoveWorldXZMode moveWorldXZ;
    private MoveWorldYMode moveWorldY;
    private IgnoreMode ignore;

    public GravityDragHandleController(
        DragHandle dragHandle,
        GravityController gravityController,
        CharacterController characterController,
        SelectionController<CharacterController> selectionController)
        : base(dragHandle)
    {
        this.gravityController = gravityController ?? throw new ArgumentNullException(nameof(gravityController));
        this.characterController = characterController ?? throw new ArgumentNullException(nameof(characterController));
        this.selectionController = selectionController ?? throw new ArgumentNullException(nameof(selectionController));

        this.gravityController.EnabledChanged += OnEnabledChanged;

        transformBackup = new(gravityController.Transform, Space.Self);

        CurrentMode = MoveWorldXZ;
    }

    public bool AutoSelect { get; set; }

    public DragHandleMode MoveWorldXZ =>
        moveWorldXZ ??= new MoveWorldXZMode(this);

    public DragHandleMode MoveWorldY =>
        moveWorldY ??= new MoveWorldYMode(this);

    public DragHandleMode Ignore =>
        ignore ??= new IgnoreMode(this);

    public float HandleSize
    {
        get => DragHandle.Size;
        set => DragHandle.Size = value;
    }

    protected override void OnDestroying() =>
        gravityController.EnabledChanged -= OnEnabledChanged;

    private void OnEnabledChanged(object sender, EventArgs e) =>
        CurrentMode = MoveWorldXZ;

    private abstract class BaseMode(GravityDragHandleController controller) : DragHandleMode
    {
        protected readonly GravityDragHandleController controller = controller;

        public override void OnModeEnter() =>
            controller.DragHandleActive = controller.gravityController.Enabled;

        public override void OnClicked()
        {
            controller.startingTransform = new(controller.gravityController.Transform, Space.Self);

            if (controller.AutoSelect)
                controller.selectionController.Select(controller.characterController);
        }

        public override void OnCancelled() =>
            controller.startingTransform.Apply(controller.gravityController.Transform);

        public override void OnDoubleClicked() =>
            controller.transformBackup.ApplyPosition(controller.gravityController.Transform);
    }

    private class MoveWorldXZMode(GravityDragHandleController controller) : BaseMode(controller)
    {
        public override void OnModeEnter()
        {
            base.OnModeEnter();

            controller.DragHandle.MovementType = DragHandle.MoveType.XZ;
        }
    }

    private class MoveWorldYMode(GravityDragHandleController controller) : BaseMode(controller)
    {
        public override void OnModeEnter()
        {
            base.OnModeEnter();

            controller.DragHandle.MovementType = DragHandle.MoveType.Y;
        }
    }

    private class IgnoreMode(GravityDragHandleController controller)
        : DragHandleMode
    {
        private readonly GravityDragHandleController controller = controller;

        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
            controller.GizmoActive = false;
        }

        public override void OnModeExit()
        {
            controller.DragHandleActive = true;
            controller.GizmoActive = true;
        }
    }
}

using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Props;

public class PropDragHandleController : GeneralDragHandleController
{
    private readonly PropController propController;
    private readonly PropService propService;
    private readonly SelectionController<PropController> propSelectionController;
    private readonly TabSelectionController tabSelectionController;

    private PropSelectMode select;
    private PropDeleteMode delete;
    private DragHandleMode moveWorldXZ;
    private DragHandleMode moveWorldY;
    private DragHandleMode rotateLocalXZ;
    private DragHandleMode rotateWorldY;
    private DragHandleMode rotateLocalY;
    private DragHandleMode scale;

    public PropDragHandleController(
        DragHandle dragHandle,
        Transform target,
        CustomGizmo gizmo,
        PropController propController,
        PropService propService,
        SelectionController<PropController> propSelectionController,
        TabSelectionController tabSelectionController)
        : base(dragHandle, gizmo, target)
    {
        this.propController = propController ?? throw new ArgumentNullException(nameof(propController));
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.propSelectionController = propSelectionController ?? throw new ArgumentNullException(nameof(propSelectionController));
        this.tabSelectionController = tabSelectionController ?? throw new ArgumentNullException(nameof(tabSelectionController));

        Gizmo.gameObject.SetActive(false);
    }

    public override DragHandleMode Select =>
        select ??= new PropSelectMode(this);

    public override DragHandleMode Delete =>
        delete ??= new PropDeleteMode(this);

    // TODO: I don't think having to override every single one of these just to change behaviour is a good idea :/
    public override DragHandleMode MoveWorldXZ =>
        moveWorldXZ ??= new TransformMode(this, base.MoveWorldXZ);

    public override DragHandleMode MoveWorldY =>
        moveWorldY ??= new TransformMode(this, base.MoveWorldY);

    public override DragHandleMode RotateLocalXZ =>
        rotateLocalXZ ??= new TransformMode(this, base.RotateLocalXZ);

    public override DragHandleMode RotateWorldY =>
        rotateWorldY ??= new TransformMode(this, base.RotateWorldY);

    public override DragHandleMode RotateLocalY =>
        rotateLocalY ??= new TransformMode(this, base.RotateLocalY);

    public override DragHandleMode Scale =>
        scale ??= new TransformMode(this, base.Scale);

    public float HandleSize
    {
        get => DragHandle.Size;
        set => DragHandle.Size = value;
    }

    public float GizmoSize
    {
        get => Gizmo.offsetScale;
        set => Gizmo.offsetScale = value;
    }

    private class TransformMode(
        PropDragHandleController controller,
        DragHandleMode originalMode)
        : WrapperDragHandleMode<DragHandleMode>(originalMode)
    {
        public override void OnClicked()
        {
            base.OnClicked();

            controller.propSelectionController.Select(controller.propController);
        }

        public override void OnGizmoClicked()
        {
            base.OnGizmoClicked();

            controller.propSelectionController.Select(controller.propController);
        }
    }

    private class PropSelectMode(PropDragHandleController controller) : SelectMode<PropDragHandleController>(controller)
    {
        public override void OnClicked()
        {
            base.OnClicked();

            Controller.propSelectionController.Select(Controller.propController);
            Controller.tabSelectionController.SelectTab(MainWindow.Tab.Props);
        }

        public override void OnDoubleClicked() =>
            Controller.propController.Focus();
    }

    private class PropDeleteMode(PropDragHandleController controller) : DeleteMode<PropDragHandleController>(controller)
    {
        public override void OnClicked() =>
            Controller.propService.Remove(Controller.propController);
    }
}

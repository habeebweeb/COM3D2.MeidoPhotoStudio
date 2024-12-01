using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class MiddleLimbDragHandleController(
    DragHandle dragHandle,
    CustomGizmo gizmo,
    CharacterController characterController,
    CharacterUndoRedoController undoRedoController,
    SelectionController<CharacterController> selectionController,
    Transform middleBone,
    Transform ikTarget)
    : CharacterIKDragHandleController(dragHandle, gizmo, characterController, undoRedoController, selectionController, middleBone, ikTarget)
{
    private DragHandleMode drag;
    private RotateMode rotate;
    private RotateBoneMode rotateBone;

    public override DragHandleMode Drag =>
        drag ??= new DragMode(this, Chain);

    public DragHandleMode Rotate =>
        BoneMode ? Ignore : rotate ??= new RotateMode(this);

    public DragHandleMode RotateBone =>
        rotateBone ??= new RotateBoneMode(this);

    protected override Transform[] Chain { get; } = [middleBone.parent, middleBone];

    private new class DragMode(MiddleLimbDragHandleController controller, Transform[] chain)
        : CharacterIKDragHandleController.DragMode(controller, chain)
    {
        public override void OnModeEnter()
        {
            base.OnModeEnter();

            controller.DragHandle.Scale = controller.BoneMode ? Vector3.one * 0.04f : Vector3.one * 0.1f;
        }
    }

    private class RotateMode(MiddleLimbDragHandleController controller)
        : PoseableMode(controller)
    {
        private static Vector2 MouseDelta =>
            new(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        public override void OnModeEnter()
        {
            controller.DragHandleActive = !controller.BoneMode;
            controller.DragHandle.MovementType = DragHandle.MoveType.None;
            controller.DragHandle.Visible = false;
            controller.GizmoActive = false;
        }

        public override void OnClicked()
        {
            base.OnClicked();

            controller.AnimationController.Playing = false;
        }

        public override void OnDragging()
        {
            var parent = controller.Bone.parent;
            var (deltaX, _) = MouseDelta;

            parent.Rotate(Vector3.right, -deltaX * 7f);
        }
    }

    private class RotateBoneMode(MiddleLimbDragHandleController controller)
        : PoseableMode(controller)
    {
        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
            controller.DragHandle.MovementType = DragHandle.MoveType.None;
            controller.DragHandle.Visible = controller.BoneMode;

            controller.GizmoActive = controller.BoneMode;
            controller.GizmoMode = CustomGizmo.GizmoMode.Local;
            controller.Gizmo.CurrentGizmoType = CustomGizmo.GizmoType.Rotate;

            if (controller.IKController.LimitLimbRotations)
                controller.Gizmo.SetVisibleRotateHandles(false, false, true);
            else
                controller.Gizmo.SetVisibleRotateHandles(true, true, true);
        }

        public override void OnGizmoClicked()
        {
            base.OnGizmoClicked();

            controller.AnimationController.Playing = false;
        }

        public override void OnGizmoDragging()
        {
            if (!controller.IKController.LimitLimbRotations)
                return;

            controller.LimitRotation();
        }
    }
}

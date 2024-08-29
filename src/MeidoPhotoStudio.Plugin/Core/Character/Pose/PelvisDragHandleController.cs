using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class PelvisDragHandleController : CharacterDragHandleController
{
    private readonly Transform pelvisBone;

    private NoneMode none;
    private RotateMode rotate;
    private RotateAlternateMode rotateAlternate;

    public PelvisDragHandleController(
        DragHandle dragHandle,
        CustomGizmo gizmo,
        CharacterController characterController,
        CharacterUndoRedoController undoRedoController)
        : base(dragHandle, gizmo, characterController, undoRedoController) =>
        pelvisBone = IKController.GetBone("Bip01 Pelvis");

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public DragHandleMode Rotate =>
        rotate ??= new RotateMode(this);

    public DragHandleMode RotateAlternate =>
        rotateAlternate ??= new RotateAlternateMode(this);

    private class NoneMode(PelvisDragHandleController controller)
        : PoseableMode(controller)
    {
        public override void OnModeEnter()
        {
            controller.GizmoActive = false;
            controller.DragHandleActive = false;
            controller.DragHandle.Visible = false;
            controller.DragHandle.MovementType = DragHandle.MoveType.None;
        }
    }

    private class RotateMode(PelvisDragHandleController controller)
        : PoseableMode(controller)
    {
        private static Vector2 MouseDelta =>
            new(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));

        public override void OnModeEnter()
        {
            controller.DragHandleActive = !controller.BoneMode;
            controller.GizmoActive = controller.BoneMode;
        }

        public override void OnClicked()
        {
            base.OnClicked();

            controller.AnimationController.Playing = false;
        }

        public override void OnDragging()
        {
            var (deltaX, deltaY) = MouseDelta;

            var cameraTransform = Camera.transform;
            var cameraForward = cameraTransform.forward;
            var cameraRight = cameraTransform.right;

            controller.pelvisBone.Rotate(cameraForward, deltaX * 5f, Space.World);
            controller.pelvisBone.Rotate(cameraRight, deltaY * 5f, Space.World);
        }

        public override void OnGizmoClicked()
        {
            base.OnGizmoClicked();

            controller.AnimationController.Playing = false;
        }
    }

    private class RotateAlternateMode(PelvisDragHandleController controller)
        : PoseableMode(controller)
    {
        private static Vector2 MouseDelta =>
            new(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));

        public override void OnModeEnter()
        {
            controller.DragHandleActive = !controller.BoneMode;
            controller.GizmoActive = false;
        }

        public override void OnClicked()
        {
            base.OnClicked();

            controller.AnimationController.Playing = false;
        }

        public override void OnDragging()
        {
            var (deltaX, _) = MouseDelta;

            controller.pelvisBone.Rotate(Vector3.right, deltaX * 6f);
        }
    }
}

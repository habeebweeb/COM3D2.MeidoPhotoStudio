using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public abstract class CharacterDragHandleController : DragHandleControllerBase, ICharacterDragHandleController
{
    private readonly CharacterController characterController;

    private Quaternion[] boneBackup;
    private bool boneMode;
    private DragHandleMode ignore;
    private bool iKEnabled = true;

    public CharacterDragHandleController(
        CustomGizmo gizmo,
        CharacterController characterController,
        CharacterUndoRedoController characterUndoRedoController,
        SelectionController<CharacterController> selectionController)
        : base(gizmo)
    {
        CharacterController = characterController ?? throw new ArgumentNullException(nameof(characterController));
        UndoRedoController = characterUndoRedoController ?? throw new ArgumentNullException(nameof(characterUndoRedoController));
        SelectionController = selectionController ?? throw new ArgumentNullException(nameof(selectionController));
    }

    public CharacterDragHandleController(
        DragHandle dragHandle,
        CharacterController characterController,
        CharacterUndoRedoController characterUndoRedoController,
        SelectionController<CharacterController> selectionController)
        : base(dragHandle)
    {
        CharacterController = characterController ?? throw new ArgumentNullException(nameof(characterController));
        UndoRedoController = characterUndoRedoController ?? throw new ArgumentNullException(nameof(characterUndoRedoController));
        SelectionController = selectionController ?? throw new ArgumentNullException(nameof(selectionController));
    }

    public CharacterDragHandleController(
        DragHandle dragHandle,
        CustomGizmo gizmo,
        CharacterController characterController,
        CharacterUndoRedoController characterUndoRedoController,
        SelectionController<CharacterController> selectionController)
        : base(dragHandle, gizmo)
    {
        CharacterController = characterController ?? throw new ArgumentNullException(nameof(characterController));
        UndoRedoController = characterUndoRedoController ?? throw new ArgumentNullException(nameof(characterUndoRedoController));
        SelectionController = selectionController ?? throw new ArgumentNullException(nameof(selectionController));
    }

    public bool BoneMode
    {
        get => boneMode;
        set
        {
            boneMode = value;

            CurrentMode.OnModeEnter();
        }
    }

    public bool IKEnabled
    {
        get =>
            Destroyed
                ? throw new InvalidOperationException("Drag handle controller is destroyed.")
                : iKEnabled;
        set
        {
            if (Destroyed)
                throw new InvalidOperationException("Drag handle controller is destroyed.");

            iKEnabled = value;

            CurrentMode.OnModeEnter();
        }
    }

    public virtual DragHandleMode Ignore =>
        ignore ??= new IgnoreMode(this);

    protected abstract Transform[] Transforms { get; }

    protected CharacterController CharacterController
    {
        get => characterController;
        private init
        {
            characterController = value;

            if (DragHandle)
                characterController.ChangedTransform += ResizeDragHandle;
        }
    }

    protected SelectionController<CharacterController> SelectionController { get; set; }

    protected AnimationController AnimationController =>
        CharacterController.Animation;

    protected IKController IKController =>
        CharacterController.IK;

    protected HeadController HeadController =>
        CharacterController.Head;

    protected CharacterUndoRedoController UndoRedoController { get; }

    protected override void OnDestroying() =>
        characterController.ChangedTransform -= ResizeDragHandle;

    private void ResizeDragHandle(object sender, TransformChangeEventArgs e)
    {
        if (!DragHandle || e.Type is not TransformChangeEventArgs.TransformType.Scale)
            return;

        DragHandle.Size = CharacterController.GameObject.transform.localScale.x;
    }

    private void BackupBoneRotations()
    {
        boneBackup ??= new Quaternion[Transforms.Length];

        for (var i = 0; i < Transforms.Length; i++)
            boneBackup[i] = Transforms[i].localRotation;
    }

    private void ApplyBackupBoneRotations()
    {
        foreach (var (bone, backup) in Transforms.Zip(boneBackup))
            bone.localRotation = backup;
    }

    protected abstract class PoseableMode(CharacterDragHandleController controller)
        : DragHandleMode
    {
        private readonly CharacterDragHandleController controller = controller;

        public override void OnClicked()
        {
            controller.UndoRedoController.StartPoseChange();
            controller.IKController.Dirty = true;

            controller.BackupBoneRotations();
            controller.SelectionController.Select(controller.CharacterController);
        }

        public override void OnReleased() =>
            controller.UndoRedoController.EndPoseChange();

        public override void OnGizmoClicked()
        {
            controller.UndoRedoController.StartPoseChange();
            controller.BackupBoneRotations();
            controller.SelectionController.Select(controller.CharacterController);
        }

        public override void OnGizmoReleased() =>
            controller.UndoRedoController.EndPoseChange();

        public override void OnCancelled() =>
            controller.ApplyBackupBoneRotations();

        public override void OnGizmoCancelled() =>
            controller.ApplyBackupBoneRotations();
    }

    // TODO: Refactor various controls with a "none" mode to use this instead
    protected class IgnoreMode(CharacterDragHandleController controller)
        : PoseableMode(controller)
    {
        private readonly CharacterDragHandleController controller = controller;

        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
            controller.GizmoActive = false;
        }
    }
}

using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class CharacterGeneralDragHandleController : GeneralDragHandleController, ICharacterDragHandleController
{
    private readonly CharacterController character;
    private readonly SelectionController<CharacterController> selectionController;
    private readonly TabSelectionController tabSelectionController;

    private bool ikEnabled = true;
    private CharacterSelectMode select;

    public CharacterGeneralDragHandleController(
        DragHandle dragHandle,
        Transform target,
        CharacterController character,
        SelectionController<CharacterController> selectionController,
        TabSelectionController tabSelectionController)
        : base(dragHandle, target)
    {
        this.character = character ?? throw new ArgumentNullException(nameof(character));
        this.selectionController = selectionController ?? throw new ArgumentNullException(nameof(selectionController));
        this.tabSelectionController = tabSelectionController ?? throw new ArgumentNullException(nameof(tabSelectionController));

        this.character.ChangedTransform += OnTransformChanged;

        TransformBackup = new(Space.World, Vector3.zero, Quaternion.identity, Vector3.one);
    }

    public override bool Enabled
    {
        get => !IsCube || base.Enabled;
        set
        {
            if (!IsCube)
                return;

            base.Enabled = value;
        }
    }

    public bool IsCube { get; init; }

    public bool ScalesWithCharacter { get; init; }

    public bool BoneMode { get; set; }

    public float HandleSize
    {
        get => DragHandle.Size;
        set
        {
            if (!IsCube)
                return;

            DragHandle.Size = value;
        }
    }

    public float GizmoSize
    {
        get => Gizmo.offsetScale;
        set
        {
            if (!IsCube)
                return;

            if (!Gizmo)
                return;

            Gizmo.offsetScale = value;
        }
    }

    public bool IKEnabled
    {
        get => Destroyed
            ? throw new InvalidOperationException("Drag handle controller is destroyed.")
            : ikEnabled;
        set
        {
            if (Destroyed)
                throw new InvalidOperationException("Drag handle controller is destroyed.");

            ikEnabled = value;

            CurrentMode.OnModeEnter();
        }
    }

    public override DragHandleMode MoveWorldXZ =>
        IKEnabled ? base.MoveWorldXZ : None;

    public override DragHandleMode MoveWorldY =>
        IKEnabled ? base.MoveWorldY : None;

    public override DragHandleMode RotateLocalXZ =>
        IKEnabled ? base.RotateLocalXZ : None;

    public override DragHandleMode RotateWorldY =>
        IKEnabled ? base.RotateWorldY : None;

    public override DragHandleMode RotateLocalY =>
        IKEnabled ? base.RotateLocalY : None;

    public override DragHandleMode Scale =>
        IKEnabled ? base.Scale : None;

    public override DragHandleMode Select =>
        select ??= new CharacterSelectMode(this);

    public override DragHandleMode Delete =>
        None;

    protected override void OnDestroying() =>
        character.ChangedTransform -= OnTransformChanged;

    private void OnTransformChanged(object sender, TransformChangeEventArgs e)
    {
        if (!ScalesWithCharacter)
            return;

        if (e.Type is not TransformChangeEventArgs.TransformType.Scale)
            return;

        DragHandle.Size = character.GameObject.transform.localScale.x;
    }

    private class CharacterSelectMode(CharacterGeneralDragHandleController controller)
        : SelectMode<CharacterGeneralDragHandleController>(controller)
    {
        public override void OnClicked()
        {
            base.OnClicked();

            Controller.selectionController.Select(Controller.character);
            Controller.tabSelectionController.SelectTab(MainWindow.Tab.CharacterPose);
        }

        public override void OnDoubleClicked() =>
            Controller.character.FocusOnBody();
    }
}

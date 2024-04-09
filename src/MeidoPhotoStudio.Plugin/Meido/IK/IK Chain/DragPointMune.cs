using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class DragPointMune : DragPointChain
{
    private bool isMuneL;
    private int inv = 1;

    public override void Set(Transform myObject)
    {
        base.Set(myObject);

        isMuneL = myObject.name[5] is 'L'; // Mune_L_Sub

        if (isMuneL)
            inv *= -1;
    }

    protected override void ApplyDragType() =>
        ApplyProperties(CurrentDragType is not LegacyDragHandleMode.None, false, false);

    protected override void OnMouseDown()
    {
        base.OnMouseDown();

        meido.SetMune(false, isMuneL);
    }

    protected override void OnDoubleClick()
    {
        if (CurrentDragType is not LegacyDragHandleMode.None)
            meido.SetMune(true, isMuneL);
    }

    protected override void Drag()
    {
        if (isPlaying)
            meido.Stop = true;

        if (CurrentDragType is LegacyDragHandleMode.RotateEyesChest)
        {
            Porc(IK, ikCtrlData, ikChain[JointUpper], ikChain[JointMiddle], ikChain[JointLower]);
            InitializeRotation();
        }

        if (CurrentDragType is LegacyDragHandleMode.RotateEyesChestAlternate)
        {
            var mouseDelta = MouseDelta();

            ikChain[JointLower].localRotation = jointRotation[JointLower];

            // TODO: Reorder operands for better performance
            ikChain[JointLower].Rotate(Vector3.up * (-mouseDelta.x / 1.5f) * inv);
            ikChain[JointLower].Rotate(Vector3.forward * (mouseDelta.y / 1.5f) * inv);
        }
    }
}

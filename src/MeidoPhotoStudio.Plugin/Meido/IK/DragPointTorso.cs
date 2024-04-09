using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class DragPointTorso : DragPointMeido
{
    // TODO: Rename these to something more descriptive
    private static readonly float[] Blah = new[] { 0.03f, 0.1f, 0.09f, 0.07f };
    private static readonly float[] Something = new[] { 0.08f, 0.15f };

    private readonly Quaternion[] spineRotation = new Quaternion[4];
    private readonly Transform[] spine = new Transform[4];

    public override void Set(Transform myObject)
    {
        base.Set(myObject);

        var spine = myObject;

        for (var i = 0; i < this.spine.Length; i++)
        {
            this.spine[i] = spine;

            spine = spine.parent;
        }
    }

    protected override void ApplyDragType()
    {
        if (IsBone)
            ApplyProperties(false, false, false);
        else
            ApplyProperties(CurrentDragType is not LegacyDragHandleMode.None, false, false);
    }

    protected override void OnMouseDown()
    {
        base.OnMouseDown();

        for (var i = 0; i < spine.Length; i++)
            spineRotation[i] = spine[i].localRotation;
    }

    protected override void Drag()
    {
        if (CurrentDragType is LegacyDragHandleMode.None)
            return;

        if (isPlaying)
            meido.Stop = true;

        var mouseDelta = MouseDelta();

        if (CurrentDragType is LegacyDragHandleMode.RotateBody)
            for (var i = 0; i < spine.Length; i++)
            {
                spine[i].localRotation = spineRotation[i];
                spine[i].Rotate(camera.transform.forward, -mouseDelta.x / 1.5f * Blah[i], Space.World);
                spine[i].Rotate(camera.transform.right, mouseDelta.y * Blah[i], Space.World);
            }

        if (CurrentDragType is LegacyDragHandleMode.RotateBodyAlternate)
            for (var i = 0; i < spine.Length; i++)
            {
                spine[i].localRotation = spineRotation[i];
                spine[i].Rotate(Vector3.right * (mouseDelta.x / 1.5f * Something[i / 2]));
            }
    }
}

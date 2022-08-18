using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Rendering;

namespace MeidoPhotoStudio.Plugin;

public class DragPointProp : DragPointGeneral
{
    public string AssetName = string.Empty;

    private List<Renderer> renderers;

    public AttachPointInfo AttachPointInfo { get; private set; } = AttachPointInfo.Empty;

    public PropInfo Info { get; set; }

    public string Name =>
        MyGameObject.name;

    public bool ShadowCasting
    {
        get => renderers.Count is not 0 && renderers.Any(r => r.shadowCastingMode is ShadowCastingMode.On);
        set
        {
            foreach (var renderer in renderers)
                renderer.shadowCastingMode = value ? ShadowCastingMode.On : ShadowCastingMode.Off;
        }
    }

    public override void Set(Transform myObject)
    {
        base.Set(myObject);

        DefaultRotation = MyObject.rotation;
        DefaultPosition = MyObject.position;
        DefaultScale = MyObject.localScale;
        renderers = new(MyObject.GetComponentsInChildren<Renderer>());
    }

    public void AttachTo(Meido meido, AttachPoint point, bool keepWorldPosition = true)
    {
        var attachPoint = meido?.IKManager.GetAttachPointTransform(point);

        AttachPointInfo = meido is null ? AttachPointInfo.Empty : new(point, meido);

        // TODO: Use transform.SetPositionAndRotation MyObject.position = position;
        var position = MyObject.position;
        var rotation = MyObject.rotation;
        var scale = MyObject.localScale;

        MyObject.transform.SetParent(attachPoint, keepWorldPosition);

        if (keepWorldPosition)
        {
            MyObject.rotation = rotation;
        }
        else
        {
            MyObject.localPosition = Vector3.zero;
            MyObject.rotation = Quaternion.identity;
        }

        MyObject.localScale = scale;

        if (!attachPoint)
            Utility.FixGameObjectScale(MyGameObject);
    }

    public void DetachFrom(bool keepWorldPosition = true) =>
        AttachTo(null, AttachPoint.None, keepWorldPosition);

    public void DetachTemporary()
    {
        MyObject.transform.SetParent(null, true);
        Utility.FixGameObjectScale(MyGameObject);
    }

    protected override void ApplyDragType()
    {
        var active = DragPointEnabled && Transforming || Special;

        ApplyProperties(active, active, GizmoEnabled && Rotating);
        ApplyColours();
    }

    protected override void OnDestroy()
    {
        Destroy(MyGameObject);

        base.OnDestroy();
    }
}

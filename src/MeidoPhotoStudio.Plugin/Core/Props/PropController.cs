using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Service;
using UnityEngine.Rendering;

namespace MeidoPhotoStudio.Plugin.Core.Props;

public class PropController : INotifyPropertyChanged
{
    private readonly TransformWatcher transformWatcher;

    public PropController(IPropModel propModel, GameObject prop, TransformWatcher transformWatcher, ShapeKeyController shapeKeyController = null)
    {
        InitialTransform = new(prop.transform);
        GameObject = prop ? prop : throw new ArgumentNullException(nameof(prop));
        PropModel = propModel ?? throw new ArgumentNullException(nameof(propModel));
        this.transformWatcher = transformWatcher ? transformWatcher : throw new ArgumentNullException(nameof(transformWatcher));
        ShapeKeyController = shapeKeyController;

        this.transformWatcher.Subscribe(GameObject.transform, RaiseTransformChanged);
    }

    public event EventHandler<TransformChangeEventArgs> TransformChanged;

    public event PropertyChangedEventHandler PropertyChanged;

    public TransformBackup InitialTransform { get; init; }

    public GameObject GameObject { get; }

    public IPropModel PropModel { get; }

    public ShapeKeyController ShapeKeyController { get; }

    public bool ShadowCasting
    {
        get => Renderers.Any(static renderer => renderer.shadowCastingMode is not ShadowCastingMode.Off);
        set
        {
            foreach (var renderer in Renderers)
                renderer.shadowCastingMode = value ? ShadowCastingMode.On : ShadowCastingMode.Off;

            RaisePropertyChanged(nameof(ShadowCasting));
        }
    }

    public bool Visible
    {
        get => Renderers.Any(static renderer => renderer.enabled);
        set
        {
            foreach (var renderer in Renderers)
                renderer.enabled = value;

            RaisePropertyChanged(nameof(Visible));
        }
    }

    private IEnumerable<Renderer> Renderers =>
        GameObject
            ? GameObject.GetComponentsInChildren<Renderer>()
            : [];

    public void Focus()
    {
        if (!GameObject)
            return;

        var propPosition = GameObject.transform.position;
        var cameraAngle = GameMain.Instance.MainCamera.transform.eulerAngles;
        var cameraDistance = GameMain.Instance.MainCamera.GetDistance();

        WfCameraMoveSupportUtility.StartMove(propPosition, cameraDistance, new(cameraAngle.y, cameraAngle.x), 0.45f);
    }

    internal void Destroy()
    {
        if (!GameObject)
            return;

        transformWatcher.Unsubscribe(GameObject.transform);

        if (PropModel is MenuFilePropModel)
        {
            var renderer = GameObject.GetComponentInChildren<SkinnedMeshRenderer>();

            if (renderer)
            {
                foreach (var material in renderer.materials)
                {
                    if (material)
                    {
                        Object.DestroyImmediate(material.mainTexture);
                        Object.DestroyImmediate(material);
                    }
                }

                Object.DestroyImmediate(renderer.sharedMesh);

                Object.DestroyImmediate(renderer);
            }
        }

        Object.Destroy(GameObject);
    }

    private void RaisePropertyChanged(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        PropertyChanged?.Invoke(this, new(name));
    }

    private void RaiseTransformChanged(TransformChangeEventArgs.TransformType type) =>
        TransformChanged?.Invoke(this, new(type));
}

namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public abstract class BaseWindow
{
    private static int id = 765;

    private Rect rect;

    public BaseWindow()
    {
        ID = id++;

        Translation.ReloadTranslationEvent += OnTranslationReloaded;
    }

    public int ID { get; }

    public virtual Rect WindowRect
    {
        get => rect;
        set => rect = value with
        {
            x = Mathf.Clamp(value.x, -value.width + Utility.GetPix(20), Screen.width - Utility.GetPix(20)),
            y = Mathf.Clamp(value.y, -value.height + Utility.GetPix(20), Screen.height - Utility.GetPix(20)),
        };
    }

    public virtual bool Visible { get; set; }

    public virtual bool Enabled { get; set; }

    public virtual void GUIFunc(int id)
    {
        Draw();
        GUI.DragWindow();
    }

    public abstract void Draw();

    public virtual void Activate()
    {
    }

    public virtual void Deactivate()
    {
    }

    public virtual void OnScreenDimensionsChanged(Vector2 newScreenDimensions)
    {
    }

    internal void Update()
    {
        if (!Visible || UnityEngine.Input.mouseScrollDelta.y is 0f)
            return;

        var mousePos = new Vector2(UnityEngine.Input.mousePosition.x, Screen.height - UnityEngine.Input.mousePosition.y);

        if (WindowRect.Contains(mousePos))
            UnityEngine.Input.ResetInputAxes();
    }

    protected virtual void ReloadTranslation()
    {
    }

    private void OnTranslationReloaded(object sender, EventArgs e) =>
        ReloadTranslation();
}

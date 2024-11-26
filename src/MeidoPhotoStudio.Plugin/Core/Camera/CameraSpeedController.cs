namespace MeidoPhotoStudio.Plugin.Core.Camera;

public class CameraSpeedController : IActivateable
{
    private readonly float fastMoveSpeed = 0.1f;
    private readonly float fastZoomSpeed = 3f;
    private readonly float slowMoveSpeed = 0.004f;
    private readonly float slowZoomSpeed = 0.1f;
    private readonly float defaultMoveSpeed;
    private readonly float defaultZoomSpeed;

    private UltimateOrbitCamera ultimateOrbitCamera;
    private Speed currentCameraSpeed = Speed.Default;

    public CameraSpeedController()
    {
        if (VRMode)
            return;

        defaultMoveSpeed = UltimateOrbitCamera.moveSpeed;
        defaultZoomSpeed = UltimateOrbitCamera.zoomSpeed;
    }

    private enum Speed
    {
        Default,
        Fast,
        Slow,
    }

    private static bool VRMode =>
        GameMain.Instance.VRMode;

    private UltimateOrbitCamera UltimateOrbitCamera =>
        VRMode ? null :
        ultimateOrbitCamera ? ultimateOrbitCamera :
        ultimateOrbitCamera = GameMain.Instance.MainCamera.GetComponent<UltimateOrbitCamera>();

    public void ApplyFastSpeed()
    {
        if (VRMode)
            return;

        if (currentCameraSpeed is Speed.Fast)
            return;

        currentCameraSpeed = Speed.Fast;

        UltimateOrbitCamera.moveSpeed = fastMoveSpeed;
        UltimateOrbitCamera.zoomSpeed = fastZoomSpeed;
    }

    public void ApplySlowSpeed()
    {
        if (VRMode)
            return;

        if (currentCameraSpeed is Speed.Slow)
            return;

        currentCameraSpeed = Speed.Slow;

        UltimateOrbitCamera.moveSpeed = slowMoveSpeed;
        UltimateOrbitCamera.zoomSpeed = slowZoomSpeed;
    }

    public void ApplyDefaultSpeed()
    {
        if (VRMode)
            return;

        if (currentCameraSpeed is Speed.Default)
            return;

        currentCameraSpeed = Speed.Default;

        UltimateOrbitCamera.moveSpeed = defaultMoveSpeed;
        UltimateOrbitCamera.zoomSpeed = defaultZoomSpeed;
    }

    void IActivateable.Activate()
    {
    }

    void IActivateable.Deactivate() =>
        ApplyDefaultSpeed();
}

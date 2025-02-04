using MeidoPhotoStudio.Plugin.Core.Camera;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class CameraPane : BasePane
{
    private readonly CameraController cameraController;
    private readonly CameraSaveSlotController cameraSaveSlotController;
    private readonly Toggle.Group cameraGroup;
    private readonly Slider zRotationSlider;
    private readonly Slider fovSlider;

    public CameraPane(
        Translation translation, CameraController cameraController, CameraSaveSlotController cameraSaveSlotController)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.cameraController = cameraController
            ?? throw new ArgumentNullException(nameof(cameraController));

        this.cameraSaveSlotController = cameraSaveSlotController
            ?? throw new ArgumentNullException(nameof(cameraSaveSlotController));

        this.cameraController.CameraChange += OnCameraChanged;

        var camera = GameMain.Instance.MainCamera.camera;
        var cameraRotation = camera.transform.eulerAngles;

        zRotationSlider = new(
            new LocalizableGUIContent(translation, "cameraPane", "zRotation"), 0f, 360f, cameraRotation.z)
        {
            HasReset = true,
            HasTextField = true,
        };

        zRotationSlider.ControlEvent += OnZRotationChanged;

        var fieldOfView = camera.fieldOfView;

        fovSlider = new(new LocalizableGUIContent(translation, "cameraPane", "fov"), 20f, 150f, fieldOfView, fieldOfView)
        {
            HasReset = true,
            HasTextField = true,
        };

        fovSlider.ControlEvent += OnFieldOfViewSliderChanged;

        cameraGroup = [..
            Enumerable.Range(1, cameraSaveSlotController.SaveSlotCount)
            .Select(index =>
            {
                var toggle = new Toggle(index.ToString(), index is 1);

                toggle.ControlEvent += OnCameraToggleChanged(index - 1);

                return toggle;

                EventHandler OnCameraToggleChanged(int cameraIndex) =>
                    (sender, _) =>
                    {
                        if (sender is not Toggle { Value: true })
                            return;

                        cameraSaveSlotController.CurrentCameraSlot = cameraIndex;
                    };
            })
        ];
    }

    public override void Draw()
    {
        GUILayout.BeginHorizontal();

        foreach (var cameraToggle in cameraGroup)
            cameraToggle.Draw();

        GUILayout.EndHorizontal();

        zRotationSlider.Draw();
        fovSlider.Draw();
    }

    public override void UpdatePane()
    {
        var camera = GameMain.Instance.MainCamera.camera;

        zRotationSlider.SetValueWithoutNotify(camera.transform.eulerAngles.z);
        fovSlider.SetValueWithoutNotify(camera.fieldOfView);
        cameraGroup[cameraSaveSlotController.CurrentCameraSlot].SetEnabledWithoutNotify(true);
    }

    private void OnZRotationChanged(object sender, EventArgs e)
    {
        var camera = GameMain.Instance.MainCamera.camera;
        var newRotation = camera.transform.eulerAngles;

        newRotation.z = zRotationSlider.Value;
        camera.transform.rotation = Quaternion.Euler(newRotation);
    }

    private void OnFieldOfViewSliderChanged(object sender, EventArgs e) =>
        GameMain.Instance.MainCamera.camera.fieldOfView = fovSlider.Value;

    private void OnCameraChanged(object sender, EventArgs e) =>
        UpdatePane();
}

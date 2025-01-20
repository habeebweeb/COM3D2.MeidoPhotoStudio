using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Core.Scenes;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class SceneManagementPane : BasePane
{
    private readonly SceneBrowserWindow sceneWindow;
    private readonly QuickSaveService quickSaveService;
    private readonly Button manageScenesButton;
    private readonly Button quickSaveButton;
    private readonly Button quickLoadButton;
    private readonly PaneHeader paneHeader;

    public SceneManagementPane(
        Translation translation, SceneBrowserWindow sceneWindow, QuickSaveService quickSaveService)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.sceneWindow = sceneWindow ?? throw new ArgumentNullException(nameof(sceneWindow));
        this.quickSaveService = quickSaveService ?? throw new ArgumentNullException(nameof(quickSaveService));

        paneHeader = new(new LocalizableGUIContent(translation, "sceneManagementPane", "sceneManagementHeader"));

        manageScenesButton = new(new LocalizableGUIContent(translation, "sceneManagementPane", "manageScenesButton"));
        manageScenesButton.ControlEvent += OnManageScenesButtonPushed;

        quickSaveButton = new(new LocalizableGUIContent(translation, "sceneManagementPane", "quickSaveButton"));
        quickSaveButton.ControlEvent += OnQuickSaveButtonPushed;

        quickLoadButton = new(new LocalizableGUIContent(translation, "sceneManagementPane", "quickLoadButton"));
        quickLoadButton.ControlEvent += OnQuickLoadButtonPushed;
    }

    public override void Draw()
    {
        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        manageScenesButton.Draw();
        UIUtility.DrawBlackLine();

        GUILayout.BeginHorizontal();

        quickSaveButton.Draw();
        quickLoadButton.Draw();

        GUILayout.EndHorizontal();
    }

    private void OnManageScenesButtonPushed(object sender, EventArgs e) =>
        sceneWindow.Visible = !sceneWindow.Visible;

    private void OnQuickSaveButtonPushed(object sender, EventArgs e) =>
        quickSaveService.QuickSave();

    private void OnQuickLoadButtonPushed(object sender, EventArgs e) =>
        quickSaveService.QuickLoad();
}

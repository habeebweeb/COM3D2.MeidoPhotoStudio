using BepInEx.Configuration;
using com.workman.cm3d2.scene.dailyEtc;
using MeidoPhotoStudio.Plugin.Core.Background;
using MeidoPhotoStudio.Plugin.Core.Camera;
using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Character.Pose;
using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Database.Background;
using MeidoPhotoStudio.Plugin.Core.Database.Character;
using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Database.Scenes;
using MeidoPhotoStudio.Plugin.Core.Effects;
using MeidoPhotoStudio.Plugin.Core.Lighting;
using MeidoPhotoStudio.Plugin.Core.Message;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Core.SceneManagement;
using MeidoPhotoStudio.Plugin.Core.Scenes;
using MeidoPhotoStudio.Plugin.Core.Serialization;
using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Core.UndoRedo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.Input;
using MeidoPhotoStudio.Plugin.Framework.Menu;
using MeidoPhotoStudio.Plugin.Framework.Service;
using MeidoPhotoStudio.Plugin.Framework.UI;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;
using UnityEngine.SceneManagement;

namespace MeidoPhotoStudio.Plugin.Core;

/// <summary>Core plugin.</summary>
public partial class PluginCore : MonoBehaviour
{
    private readonly IconCache iconCache = new();
    private readonly List<IActivateable> activateables = [];

    private ConfigFile configuration;
    private CustomMaidSceneService customMaidSceneService;
    private CharacterService characterService;
    private DragHandle.ClickHandler dragHandleClickHandler;
    private CustomGizmo.ClickHandler gizmoClickHandler;
    private TransformWatcher transformWatcher;
    private ScreenSizeChecker screenSizeChecker;

    public bool Active { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(this);

        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        if (Active)
            Deactivate(true);

        GameMain.Instance.MainCamera.ResetCalcNearClip();

        SceneManager.sceneUnloaded -= OnSceneUnloaded;

        iconCache.Destroy();

        DragHandle.Builder.DestroyParent();
        LightRepository.DestroyParent();
        Framework.CoroutineRunner.DestroyParent();
        IKController.DestroyParent();
        WfCameraMoveSupportUtility.Destroy();
    }

    private void Start()
    {
        // Configuration
        var configRoot = Path.Combine(BepInEx.Paths.ConfigPath, Plugin.PluginName);
        var presetsPath = Path.Combine(configRoot, "Presets");
        var databasePath = Path.Combine(configRoot, "Database");

        configuration = new ConfigFile(Path.Combine(configRoot, $"{Plugin.PluginName}.cfg"), false);

        var inputConfiguration = new InputConfiguration(configuration);

        var translationConfiguration = new TranslationConfiguration(configuration);
        var faceShapeKeyConfiguration = new FaceShapeKeyConfiguration(configuration);
        var dragHandleConfiguration = new DragHandleConfiguration(configuration);
        var bodyShapeKeyConfiguration = new BodyShapeKeyConfiguration(configuration);
        var menuPropsConfiguration = new MenuPropsConfiguration(configuration);
        var autoSaveConfiguration = new AutoSaveConfiguration(configuration);
        var uiConfiguration = new UIConfiguration(configuration);

        // Translation
        Translation.Initialize(translationConfiguration.CurrentLanguage);

        // Utilities
        screenSizeChecker = gameObject.AddComponent<ScreenSizeChecker>();
        screenSizeChecker.enabled = false;

        transformWatcher = gameObject.AddComponent<TransformWatcher>();

        customMaidSceneService = new CustomMaidSceneService();

        var tabSelectionController = new TabSelectionController();

        // Input
        var inputPollingService = gameObject.AddComponent<InputPollingService>();

        var inputRemapper = gameObject.AddComponent<InputRemapper>();
        inputRemapper.InputPollingService = inputPollingService;

        inputPollingService.AddInputHandler(new InputHandler(this, inputConfiguration, customMaidSceneService));

        // UI
        dragHandleClickHandler = gameObject.AddComponent<DragHandle.ClickHandler>();
        dragHandleClickHandler.enabled = false;

        gizmoClickHandler = gameObject.AddComponent<CustomGizmo.ClickHandler>();
        gizmoClickHandler.enabled = false;

        var windowManager = gameObject.AddComponent<WindowManager>();

        windowManager.PluginCore = this;

        var generalDragHandleInputService = new GeneralDragHandleInputHandler(inputConfiguration);

        AddPluginActiveInputHandler(generalDragHandleInputService);

        // Screenshot
        var screenshotService = gameObject.AddComponent<ScreenshotService>();

        screenshotService.WindowManager = windowManager;

        AddPluginActiveInputHandler(new ScreenshotServiceInputHandler(screenshotService, inputConfiguration));

        // Undo/Redo
        var undoRedoService = new UndoRedoService();

        AddPluginActiveInputHandler(new UndoRedoInputHandler(undoRedoService, inputConfiguration));

        // Characters
        var gameBlendSetRepository = new GameBlendSetRepository();
        var customBlendSetRepository = new CustomBlendSetRepository(Path.Combine(presetsPath, "Face Presets"));
        var gameAnimationRepository = new GameAnimationRepository(databasePath);
        var customAnimationRepository = new CustomAnimationRepository(Path.Combine(presetsPath, "Custom Poses"));
        var customAnimationRepositorySorter = new CustomAnimationRepositorySorter(customAnimationRepository.RootCategoryName);

        var characterRepository = new CharacterRepository();
        var editModeMaidService = new EditModeMaidService(customMaidSceneService, characterRepository);

        characterService = new CharacterService(customMaidSceneService, editModeMaidService, transformWatcher, undoRedoService);

        windowManager.CharacterService = characterService;

        var characterCallController = new CallController(characterRepository, characterService, customMaidSceneService, editModeMaidService);
        var characterSelectionController = new SelectionController<CharacterController>(characterService);
        var facialExpressionBuilder = new FacialExpressionBuilder(faceShapeKeyConfiguration);
        var faceShapekeyRangeConfiguration = new ShapeKeyRangeConfiguration(
            new ShapeKeyRangeSerializer(Path.Combine(databasePath, "face_shapekey_range.json")));

        var bodyShapeKeyRangeConfiguration = new ShapeKeyRangeConfiguration(new ShapeKeyRangeSerializer(Path.Combine(databasePath, "body_shapekey_range.json")));

        AddPluginActiveInputHandler(new CharacterDressingCycler(characterService, inputConfiguration));

        var gravityDragHandleInputService = new GravityDragHandleInputService(inputConfiguration);

        AddPluginActiveInputHandler(gravityDragHandleInputService);

        var gravityDragHandleService = new GravityDragHandleService(gravityDragHandleInputService, characterService);
        var globalGravityService = new GlobalGravityService(characterService);

        var characterDragHandleInputService = new CharacterDragHandleInputService(
            generalDragHandleInputService,
            new UpperLimbDragHandleInputHandler(inputConfiguration),
            new MiddleLimbDragHandleInputHandler(inputConfiguration),
            new LowerLimbDragHandleInputHandler(inputConfiguration),
            new TorsoDragHandleInputHandler(inputConfiguration),
            new HeadDragHandleInputHandler(inputConfiguration),
            new PelvisDragHandleInputHandler(inputConfiguration),
            new SpineDragHandleInputHandler(inputConfiguration),
            new HipDragHandleInputHandler(inputConfiguration),
            new ThighDragHandleInputHandler(inputConfiguration),
            new ChestDragHandleInputHandler(inputConfiguration),
            new ChestSubGizmoInputHandler(inputConfiguration),
            new DigitBaseDragHandleInputHandler(inputConfiguration),
            new DigitDragHandleInputHandler(inputConfiguration),
            new EyeDragHandleInputHandler(inputConfiguration));

        AddPluginActiveInputHandler(characterDragHandleInputService);

        var characterUndoRedoService = new CharacterUndoRedoService(characterService, undoRedoService);

        var ikDragHandleService = new IKDragHandleService(
            characterDragHandleInputService,
            characterService,
            characterUndoRedoService,
            characterSelectionController,
            tabSelectionController)
        {
            SmallHandle = dragHandleConfiguration.SmallTransformCube.Value,
            CubeEnabled = dragHandleConfiguration.CharacterTransformCube.Value,
        };

        AddPluginActiveInputHandler(new AnimationCycler(
            characterService,
            new AnimationCyclingService(
                characterService, characterUndoRedoService, gameAnimationRepository, customAnimationRepository, customAnimationRepositorySorter),
            inputConfiguration));

        // Message
        var messageWindowManager = new MessageWindowManager();

        // Camera
        var cameraController = new CameraController(customMaidSceneService);

        var cameraSaveSlotController = new CameraSaveSlotController(cameraController);
        var cameraSpeedController = new CameraSpeedController();

        AddPluginActiveInputHandler(
            new CameraInputHandler(
                cameraController, cameraSpeedController, cameraSaveSlotController, inputConfiguration));

        // Backgrounds
        var backgroundRepository = new BackgroundRepository();
        var backgroundService = new BackgroundService(backgroundRepository);
        var backgroundDragHandleService = new BackgroundDragHandleService(generalDragHandleInputService, backgroundService);

        // Lights
        var lightRepository = new LightRepository(transformWatcher);

        var lightSelectionController = new SelectionController<LightController>(lightRepository);

        _ = new LightDragHandleRepository(
            generalDragHandleInputService, lightRepository, lightSelectionController, tabSelectionController);

        // Effects
        var bloomController = new BloomController(GameMain.Instance.MainCamera.camera);
        var depthOfFieldController = new DepthOfFieldController(GameMain.Instance.MainCamera.camera);
        var vignetteController = new VignetteController(GameMain.Instance.MainCamera.camera);
        var fogController = new FogController(GameMain.Instance.MainCamera.camera);
        var blurController = new BlurController(GameMain.Instance.MainCamera.camera);
        var sepiaToneController = new SepiaToneController(GameMain.Instance.MainCamera.camera);

        // Props
        var gamePropRepository = new PhotoBgPropRepository();
        var deskPropRepository = new DeskPropRepository();
        var otherPropRepository = new OtherPropRepository(backgroundRepository);
        var backgroundPropRepository = new BackgroundPropRepository(backgroundRepository);
        var myRoomPropRepository = new MyRoomPropRepository();
        var menuPropRepository = new MenuPropRepository(
            menuPropsConfiguration,
            new MenuFileCacheSerializer(Path.Combine(BepInEx.Paths.ConfigPath, Plugin.PluginName)));

        var propService = new PropService(transformWatcher);

        var propSelectionController = new SelectionController<PropController>(propService);

        var propDragHandleService = new PropDragHandleService(
            generalDragHandleInputService,
            propService,
            propSelectionController,
            tabSelectionController)
        {
            SmallHandle = dragHandleConfiguration.SmallTransformCube.Value,
        };

        var propAttachmentService = new PropAttachmentService(characterService, propService);

        var propSchemaMapper = new PropSchemaToPropModelMapper(
            backgroundPropRepository,
            deskPropRepository,
            myRoomPropRepository,
            gamePropRepository,
            menuPropRepository,
            otherPropRepository);

        var propModelSchemaBuilder = new PropModelSchemaBuilder();

        var favouritePropRepository = new FavouritePropRepository(
            new FavouritePropListSerializer(
                Path.Combine(BepInEx.Paths.ConfigPath, Plugin.PluginName),
                propModelSchemaBuilder,
                propSchemaMapper));

        // Scenes
        var transformSchemaBuilder = new TransformSchemaBuilder();

        // TODO: This is kinda stupid tbf. Maybe look into writing a code generator and attributes to create these
        // "schema" things instead of manually building it.
        // Would that even be possible? idk.
        var sceneSchemaBuilder = new SceneSchemaBuilder(
            new CharacterServiceSchemaBuilder(
                characterService,
                globalGravityService,
                new CharacterSchemaBuilder(
                    facialExpressionBuilder,
                    bodyShapeKeyConfiguration,
                    new AnimationModelSchemaBuilder(),
                    new BlendSetModelSchemaBuilder(),
                    propModelSchemaBuilder,
                    transformSchemaBuilder),
                new GlobalGravitySchemaBuilder()),
            new MessageWindowSchemaBuilder(messageWindowManager),
            new CameraSchemaBuilder(cameraSaveSlotController, new CameraInfoSchemaBuilder()),
            new LightRepositorySchemaBuilder(
                lightRepository, new LightSchemaBuilder(new LightPropertiesSchemaBuilder())),
            new EffectsSchemaBuilder(
                bloomController,
                depthOfFieldController,
                fogController,
                vignetteController,
                sepiaToneController,
                blurController,
                new BloomSchemaBuilder(),
                new DepthOfFieldSchemaBuilder(),
                new FogSchemaBuilder(),
                new VignetteSchemaBuilder(),
                new SepiaToneSchemaBuilder(),
                new BlurSchemaBuilder()),
            new BackgroundSchemaBuilder(
                backgroundService,
                new BackgroundModelSchemaBuilder(),
                transformSchemaBuilder),
            new PropsSchemaBuilder(
                propService,
                propDragHandleService,
                propAttachmentService,
                new PropControllerSchemaBuilder(propModelSchemaBuilder, transformSchemaBuilder),
                new DragHandleSchemaBuilder(),
                new AttachPointSchemaBuilder()));

        var sceneLoader = new SceneLoader(
            undoRedoService,
            new CharacterAspectLoader(
                characterService,
                characterRepository,
                editModeMaidService,
                customMaidSceneService,
                globalGravityService,
                gameAnimationRepository,
                customAnimationRepository,
                gameBlendSetRepository,
                customBlendSetRepository,
                menuPropRepository,
                faceShapeKeyConfiguration,
                bodyShapeKeyConfiguration),
            new MessageAspectLoader(messageWindowManager),
            new CameraAspectLoader(cameraSaveSlotController),
            new LightAspectLoader(lightRepository, backgroundService),
            new EffectsAspectLoader(
                bloomController,
                depthOfFieldController,
                vignetteController,
                fogController,
                blurController,
                sepiaToneController),
            new BackgroundAspectLoader(backgroundService, backgroundRepository),
            new PropsAspectLoader(
                propService,
                propDragHandleService,
                propAttachmentService,
                characterService,
                propSchemaMapper));

        var sceneSerializer = new WrappedSerializer(new(), new());
        var quickSaveService = new QuickSaveService(configRoot, characterService, sceneSchemaBuilder, sceneSerializer, sceneLoader);

        AddPluginActiveInputHandler(new QuickSaveInputHandler(
            quickSaveService,
            inputConfiguration));

        var sceneRepository = new SceneRepository(Path.Combine(configRoot, "Scenes"), sceneSerializer);

        var autoSaveService = new AutoSaveService(characterService, sceneRepository, screenshotService, sceneSchemaBuilder)
        {
            Enabled = autoSaveConfiguration.Enabled.Value,
            AutoSaveInterval = autoSaveConfiguration.Frequency.Value,
            Slots = autoSaveConfiguration.Slots.Value,
        };

        // Windows
        var sceneBrowser = new SceneBrowserWindow(
            sceneRepository,
            new(sceneRepository, screenshotService, sceneSchemaBuilder, sceneSerializer, sceneLoader),
            sceneSchemaBuilder,
            screenshotService,
            new(configuration),
            inputRemapper);

        AddPluginActiveInputHandler(new SceneBrowserWindowInputHandler(sceneBrowser, inputConfiguration));

        var messageWindow = new MessageWindow(messageWindowManager, inputRemapper);

        AddPluginActiveInputHandler(new MessageWindow.InputHandler(messageWindow, inputConfiguration));

        var settingsWindow = new SettingsWindow(inputRemapper)
        {
            [SettingsWindow.SettingType.Controls] = new InputSettingsPane(inputConfiguration, inputRemapper),
            [SettingsWindow.SettingType.DragHandle] = new DragHandleSettingsPane(dragHandleConfiguration, ikDragHandleService, propDragHandleService),
            [SettingsWindow.SettingType.AutoSave] = new AutoSaveSettingsPane(autoSaveConfiguration, autoSaveService),
            [SettingsWindow.SettingType.Translation] = new TranslationSettingsPane(),
        };

        var mainWindow = new MainWindow(tabSelectionController, customMaidSceneService, inputRemapper, settingsWindow)
        {
            WindowWidth = uiConfiguration.WindowWidth.Value,

            [MainWindow.Tab.Call] = new CallWindowPane()
            {
                new CharacterPlacementPane(new(characterService)),
                new CharacterCallPane(characterCallController),
            },
            [MainWindow.Tab.Character] = new CharacterWindowPane()
            {
                new CharacterSwitcherPane(
                    characterService, characterSelectionController, customMaidSceneService, editModeMaidService),
                new CharacterPane(tabSelectionController, characterSelectionController)
                {
                    [CharacterPane.CharacterWindowTab.Pose] =
                    [
                        new AnimationSelectorPane(gameAnimationRepository, customAnimationRepository, characterUndoRedoService, characterSelectionController, customAnimationRepositorySorter),
                        new IKPane(ikDragHandleService, characterUndoRedoService,  characterSelectionController),
                        new AnimationPane(characterUndoRedoService, characterSelectionController),
                        new FreeLookPane(characterSelectionController),
                        new DressingPane(characterSelectionController),
                        new GravityControlPane(gravityDragHandleService, globalGravityService, characterSelectionController),
                        new AttachedAccessoryPane(menuPropRepository, characterSelectionController),
                        new HandPresetSelectorPane(new(Path.Combine(presetsPath, "Hand Presets")), characterUndoRedoService, characterSelectionController),
                        new CopyPosePane(characterService, characterUndoRedoService, characterSelectionController),
                    ],
                    [CharacterPane.CharacterWindowTab.Face] =
                    [
                        new BlendSetSelectorPane(
                            gameBlendSetRepository,
                            customBlendSetRepository,
                            facialExpressionBuilder,
                            characterSelectionController),
                        new ExpressionPane(characterSelectionController, faceShapeKeyConfiguration, faceShapekeyRangeConfiguration),
                    ],
                    [CharacterPane.CharacterWindowTab.Body] =
                    [
                        new BodyShapeKeyPane(characterSelectionController, bodyShapeKeyConfiguration, bodyShapeKeyRangeConfiguration)
                    ],
                },
            },
            [MainWindow.Tab.Environment] = new BGWindowPane()
            {
                new SceneManagementPane(sceneBrowser, quickSaveService),
                new BackgroundsPane(backgroundService, backgroundRepository, backgroundDragHandleService),
                new CameraPane(cameraController, cameraSaveSlotController),
                new LightsPane(lightRepository, lightSelectionController),
                new EffectsPane()
                {
                    [EffectsPane.EffectType.Bloom] = new BloomPane(bloomController),
                    [EffectsPane.EffectType.DepthOfField] = new DepthOfFieldPane(depthOfFieldController),
                    [EffectsPane.EffectType.Vignette] = new VignettePane(vignetteController),
                    [EffectsPane.EffectType.Fog] = new FogPane(fogController),
                    [EffectsPane.EffectType.Blur] = new BlurPane(blurController),
                    [EffectsPane.EffectType.SepiaTone] = new SepiaTonePane(sepiaToneController),
                },
            },
            [MainWindow.Tab.Props] = new PropsWindowPane()
            {
                new PropsPane()
                {
                    [PropsPane.PropCategory.Game] = new GamePropsPane(propService, gamePropRepository),
                    [PropsPane.PropCategory.Desk] = new DeskPropsPane(propService, deskPropRepository),
                    [PropsPane.PropCategory.Other] = new OtherPropsPane(propService, otherPropRepository),
                    [PropsPane.PropCategory.HandItem] = new HandItemPropsPane(propService, menuPropRepository),
                    [PropsPane.PropCategory.Background] = new BackgroundPropsPane(
                        propService,
                        backgroundPropRepository),
                    [PropsPane.PropCategory.Menu] = new MenuPropsPane(
                        propService,
                        menuPropRepository,
                        menuPropsConfiguration,
                        iconCache),
                    [PropsPane.PropCategory.MyRoom] = new MyRoomPropsPane(
                        propService, myRoomPropRepository, iconCache),
                    [PropsPane.PropCategory.Favourite] = new FavouritePropsPane(propService, favouritePropRepository, iconCache),
                },
                new PropManagerPane(propService, favouritePropRepository, propDragHandleService, propSelectionController, new()),
                new PropShapeKeyPane(propSelectionController),
                new AttachPropPane(characterService, propAttachmentService, propSelectionController),
            },
        };

        settingsWindow[SettingsWindow.SettingType.UI] = new UISettingsPane(uiConfiguration, mainWindow);

        AddPluginActiveInputHandler(new MainWindow.InputHandler(mainWindow, inputConfiguration));

        windowManager[WindowManager.Window.Main] = mainWindow;
        windowManager[WindowManager.Window.Message] = messageWindow;
        windowManager[WindowManager.Window.Save] = sceneBrowser;
        windowManager[WindowManager.Window.Settings] = settingsWindow;

        dragHandleClickHandler.WindowManager = windowManager;
        gizmoClickHandler.WindowManager = windowManager;

        AddActivateable(cameraController);

        AddActivateable(characterRepository);
        AddActivateable(editModeMaidService);
        AddActivateable(characterService);
        AddActivateable(characterCallController);

        AddActivateable(cameraSaveSlotController);
        AddActivateable(cameraSpeedController);

        AddActivateable(messageWindowManager);

        AddActivateable(backgroundRepository);
        AddActivateable(backgroundService);

        AddActivateable(lightRepository);

        AddActivateable(bloomController);
        AddActivateable(depthOfFieldController);
        AddActivateable(vignetteController);
        AddActivateable(fogController);
        AddActivateable(blurController);
        AddActivateable(sepiaToneController);

        AddActivateable(propService);
        AddActivateable(favouritePropRepository);

        AddActivateable(autoSaveService);

        AddActivateable(windowManager);

        void AddPluginActiveInputHandler<T>(T inputHandler)
            where T : IInputHandler =>
            inputPollingService.AddInputHandler(new PluginActiveInputHandler<T>(this, inputHandler));

        T AddActivateable<T>(T activateable)
            where T : IActivateable
        {
            activateables.Add(activateable);

            return activateable;
        }
    }

    private void Activate()
    {
        if (!GameMain.Instance.SysDlg.IsDecided)
            return;

        dragHandleClickHandler.enabled = true;
        transformWatcher.enabled = true;
        gizmoClickHandler.enabled = true;
        screenSizeChecker.enabled = true;

        foreach (var activateable in activateables)
            activateable.Activate();

        SetDailyPanelActive(false);

        Active = true;
    }

    private void Deactivate(bool force = false)
    {
        if (characterService.Busy)
            return;

        var sysDialog = GameMain.Instance.SysDlg;

        if (!sysDialog.IsDecided && !force)
            return;

        Active = false;

        if (force)
        {
            Exit();

            return;
        }

        sysDialog.Show(
            string.Format(Translation.Get("systemMessage", "exitConfirm"), Plugin.PluginName),
            SystemDialog.TYPE.OK_CANCEL,
            Exit,
            Resume);

        void Resume()
        {
            sysDialog.Close();
            Active = true;
        }

        void Exit()
        {
            sysDialog.Close();

            dragHandleClickHandler.enabled = false;
            transformWatcher.enabled = false;
            gizmoClickHandler.enabled = false;
            screenSizeChecker.enabled = false;

            transformWatcher.Clear();

            foreach (var activateable in activateables)
                activateable.Deactivate();

            // TODO: Should this deactivation stuff be somewhere else?
            if (customMaidSceneService.EditScene)
            {
                SceneEditWindow.BgIconData.GetItemData(SceneEdit.Instance.bgIconWindow.selectedIconId).Exec();
                SceneEditWindow.PoseIconData.GetItemData(SceneEdit.Instance.pauseIconWindow.selectedIconId).ExecScript();

                if (SceneEdit.Instance.viewReset.GetVisibleEyeToCam())
                    SceneEdit.Instance.maid.EyeToCamera(Maid.EyeMoveType.目と顔を向ける, 0.8f);
                else
                    SceneEdit.Instance.maid.EyeToCamera(Maid.EyeMoveType.無視する, 0.8f);
            }
            else
            {
                if (GameMain.Instance.CharacterMgr.status.isDaytime)
                    GameMain.Instance.BgMgr.ChangeBg(DailyAPI.dayBg);
                else
                    GameMain.Instance.BgMgr.ChangeBg(DailyAPI.nightBg);
            }

            configuration.Save();

            SetDailyPanelActive(true);
        }
    }

    private void OnSceneUnloaded(Scene arg0)
    {
        if (Active)
            Deactivate(true);

        GameMain.Instance.MainCamera.ResetCalcNearClip();
    }

    private void ToggleActive()
    {
        if (Active)
            Deactivate();
        else
            Activate();
    }

    private void SetDailyPanelActive(bool active)
    {
        if (!customMaidSceneService.OfficeScene)
            return;

        var uiRoot = GameObject.Find("UI Root");

        if (!uiRoot)
            return;

        var dailyPanel = uiRoot.transform.Find("DailyPanel");

        if (!dailyPanel)
            return;

        dailyPanel.gameObject.SetActive(active);
    }
}

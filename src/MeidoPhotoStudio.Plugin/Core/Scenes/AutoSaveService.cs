using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Database.Scenes;
using MeidoPhotoStudio.Plugin.Core.Serialization;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.Service;

namespace MeidoPhotoStudio.Plugin.Core.Scenes;

public class AutoSaveService : IActivateable
{
    private const string AutoSaveCategoryName = "autosave";

    private readonly CharacterService characterService;
    private readonly SceneRepository sceneRepository;
    private readonly ScreenshotService screenshotService;
    private readonly SceneSchemaBuilder sceneSchemaBuilder;
    private readonly CoroutineRunner autoSaveTimer;

    private bool active = false;
    private SceneModel[] scenes;
    private int slots = 5;
    private int autoSaveInterval = 30;
    private int currentSlot;
    private bool enabled;

    public AutoSaveService(
        CharacterService characterService,
        SceneRepository sceneRepository,
        ScreenshotService screenshotService,
        SceneSchemaBuilder sceneSchemaBuilder)
    {
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.sceneRepository = sceneRepository ?? throw new ArgumentNullException(nameof(sceneRepository));
        this.screenshotService = screenshotService
            ? screenshotService : throw new ArgumentNullException(nameof(screenshotService));
        this.sceneSchemaBuilder = sceneSchemaBuilder ?? throw new ArgumentNullException(nameof(sceneSchemaBuilder));

        InitializeSaveSlots();

        autoSaveTimer = new(AutoSave)
        {
            Name = "Auto Save Coroutine",
        };
    }

    public bool Enabled
    {
        get => enabled;
        set
        {
            if (enabled == value)
                return;

            enabled = value;

            if (enabled)
                StartAutoSaveTimer();
            else
                StopAutoSaveTimer();
        }
    }

    public int Slots
    {
        get => slots;
        set
        {
            var newSlotCount = value;

            if (newSlotCount <= 0)
                newSlotCount = 1;

            if (slots == newSlotCount)
                return;

            slots = newSlotCount;

            InitializeSaveSlots();
        }
    }

    public int AutoSaveInterval
    {
        get => autoSaveInterval;
        set
        {
            var newInterval = value;

            if (newInterval < 10)
                newInterval = 10;

            if (newInterval == autoSaveInterval)
                return;

            autoSaveInterval = value;

            if (Enabled && autoSaveTimer.Running)
                StartAutoSaveTimer();
        }
    }

    private SceneModel CurrentSceneModel =>
        scenes[currentSlot];

    void IActivateable.Activate()
    {
        active = true;
        StartAutoSaveTimer();
    }

    void IActivateable.Deactivate()
    {
        active = false;
        StopAutoSaveTimer();
    }

    private void StartAutoSaveTimer()
    {
        if (!Enabled || !active)
            return;

        autoSaveTimer.Start();
    }

    private void StopAutoSaveTimer() =>
        autoSaveTimer.Stop();

    private IEnumerator AutoSave()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(AutoSaveInterval);

            if (characterService.Busy)
                while (characterService.Busy)
                    yield return new WaitForSeconds(2f);

            if (screenshotService.TakingScreenshot)
                while (screenshotService.TakingScreenshot)
                    yield return new WaitForSeconds(2f);

            screenshotService.TakeScreenshotToTexture(
                screenshot => OnScreenshotTaken(screenshot, CurrentSceneModel),
                new(true, true, false));
        }

        void OnScreenshotTaken(Texture2D screenshot, SceneModel existingScene)
        {
            if (existingScene is SceneModel scene)
                sceneRepository.Delete(scene);

            sceneRepository.AddedScene += OnSceneAdded;

            sceneRepository.Add(sceneSchemaBuilder.Build(), screenshot, AutoSaveCategoryName, $"autosave{DateTime.Now:yyyyMMddHHmmss}");

            void OnSceneAdded(object sender, SceneChangeEventArgs e)
            {
                if (!string.Equals(e.Scene.Category, AutoSaveCategoryName, StringComparison.Ordinal))
                    return;

                sceneRepository.AddedScene -= OnSceneAdded;

                scenes[currentSlot] = e.Scene;

                CycleSlot();
            }
        }
    }

    private void InitializeSaveSlots()
    {
        scenes = new SceneModel[slots];

        if (sceneRepository.ContainsCategory(AutoSaveCategoryName))
            foreach (var (index, model) in sceneRepository[AutoSaveCategoryName]
                .OrderByDescending(static model => File.GetCreationTime(model.Filename))
                .Take(slots)
                .Reverse()
                .WithIndex())
                scenes[index] = model;

        currentSlot = Array.IndexOf(scenes, null);

        if (currentSlot < 0)
            currentSlot = 0;
    }

    private void CycleSlot()
    {
        currentSlot = Wrap(currentSlot + 1, 0, scenes.Length);

        static int Wrap(int value, int min, int max) =>
            value < min ? max :
            value >= max ? min :
            value;
    }
}

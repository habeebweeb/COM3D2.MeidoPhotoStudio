using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class GravityDragHandleService
{
    private static readonly (float Small, float Normal) HandleSize = (0.5f, 1f);

    private readonly GravityDragHandleInputService gravityDragHandleInputService;
    private readonly CharacterService characterService;
    private readonly SelectionController<CharacterController> selectionController;
    private readonly Dictionary<CharacterController, GravityDragHandleSet> dragHandleSets = [];

    private bool smallHandle;

    public GravityDragHandleService(
        GravityDragHandleInputService gravityDragHandleInputService,
        CharacterService characterService,
        SelectionController<CharacterController> selectionController)
    {
        this.gravityDragHandleInputService = gravityDragHandleInputService ?? throw new ArgumentNullException(nameof(gravityDragHandleInputService));
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.selectionController = selectionController ?? throw new ArgumentNullException(nameof(selectionController));

        this.characterService.CalledCharacters += OnCharactersCalled;
        this.characterService.Deactivating += OnDeactivating;
    }

    public bool SmallHandle
    {
        get => smallHandle;
        set
        {
            if (value == smallHandle)
                return;

            smallHandle = value;

            foreach (var (hair, clothing) in dragHandleSets.Values)
            {
                var handleSize = smallHandle ? HandleSize.Small : HandleSize.Normal;

                hair.HandleSize = handleSize;
                clothing.HandleSize = handleSize;
            }
        }
    }

    public GravityDragHandleSet this[CharacterController characterController] =>
        characterController is null
            ? throw new ArgumentNullException(nameof(characterController))
            : dragHandleSets[characterController];

    private void OnDeactivating(object sender, EventArgs e)
    {
        foreach (var dragHandleSet in dragHandleSets.Values)
            DestroyDragHandleSet(dragHandleSet);

        dragHandleSets.Clear();
    }

    private void OnCharactersCalled(object sender, CharacterServiceEventArgs e)
    {
        var oldCharacters = dragHandleSets.Keys.ToArray();

        foreach (var character in oldCharacters.Except(e.LoadedCharacters))
        {
            DestroyDragHandleSet(dragHandleSets[character]);
            character.ProcessingCharacterProps -= OnCharacterProcessing;
            dragHandleSets.Remove(character);
        }

        foreach (var character in e.LoadedCharacters.Except(oldCharacters))
        {
            dragHandleSets[character] = InitializeDragHandleSet(character);
            character.ProcessingCharacterProps += OnCharacterProcessing;
        }
    }

    private void OnCharacterProcessing(object sender, CharacterProcessingEventArgs e)
    {
        var character = sender as CharacterController;

        if (!dragHandleSets.ContainsKey(character))
        {
            character.ProcessingCharacterProps -= OnCharacterProcessing;

            return;
        }

        var mpnStart = (MPN)Enum.Parse(typeof(MPN_TYPE_RANGE), nameof(MPN_TYPE_RANGE.BODY_RELOAD_START)) - 1;
        var mpnEnd = (MPN)Enum.Parse(typeof(MPN_TYPE_RANGE), nameof(MPN_TYPE_RANGE.BODY_RELOAD_END));

        if (!e.ChangingSlots.Any(slot => slot >= mpnStart || slot <= mpnEnd))
            return;

        DestroyDragHandleSet(dragHandleSets[character]);

        character.ProcessedCharacterProps += OnCharacterProcessed;

        void OnCharacterProcessed(object sender, CharacterProcessingEventArgs e)
        {
            dragHandleSets[character] = InitializeDragHandleSet(character);

            character.ProcessedCharacterProps -= OnCharacterProcessed;
        }
    }

    private GravityDragHandleSet InitializeDragHandleSet(CharacterController character)
    {
        if (character.Clothing is null)
            Plugin.Logger.LogDebug("clothing is null");

        if (character.Clothing?.ClothingGravityController is null)
            Plugin.Logger.LogDebug("Clothing gravity controller is null");

        var clothingDragHandle = BuildDragHandle(
            character.Clothing.ClothingGravityController,
            character,
            selectionController);

        var hairDraghandle = BuildDragHandle(
            character.Clothing.HairGravityController,
            character,
            selectionController);

        gravityDragHandleInputService.AddController(clothingDragHandle);
        gravityDragHandleInputService.AddController(hairDraghandle);

        return new()
        {
            ClothingDragHandle = clothingDragHandle,
            HairDragHandle = hairDraghandle,
        };

        GravityDragHandleController BuildDragHandle(
            GravityController gravityController,
            CharacterController character,
            SelectionController<CharacterController> selectionController)
        {
            var gravityControl = gravityController.Transform;

            var dragHandle = new DragHandle.Builder()
            {
                Name = $"[{gravityController.Name}]",
                Target = gravityControl,
                Priority = 10,
                Scale = Vector3.one * 0.12f,
                PositionDelegate = () => gravityControl.position,
                Size = SmallHandle ? HandleSize.Small : HandleSize.Normal,
            }.Build();

            return new GravityDragHandleController(dragHandle, gravityController, character, selectionController)
            {
                Enabled = false,
            };
        }
    }

    private void DestroyDragHandleSet(GravityDragHandleSet dragHandleSet)
    {
        dragHandleSet.HairDragHandle.Destroy();
        dragHandleSet.ClothingDragHandle.Destroy();

        gravityDragHandleInputService.RemoveController(dragHandleSet.HairDragHandle);
        gravityDragHandleInputService.RemoveController(dragHandleSet.ClothingDragHandle);
    }
}

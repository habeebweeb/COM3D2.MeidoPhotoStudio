using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Framework.Input;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class AnimationCycler(
    CharacterService characterService,
    AnimationCyclingService animationCyclingService,
    InputConfiguration inputConfiguration) : IInputHandler
{
    private readonly CharacterService characterService =
        characterService ?? throw new ArgumentNullException(nameof(characterService));

    private readonly AnimationCyclingService animationCyclingService =
        animationCyclingService ?? throw new ArgumentNullException(nameof(animationCyclingService));

    private readonly InputConfiguration inputConfiguration =
        inputConfiguration ?? throw new ArgumentNullException(nameof(inputConfiguration));

    public bool Active =>
        characterService.Count > 0;

    public void CheckInput()
    {
        if (inputConfiguration[Shortcut.CycleNextAnimation].IsDown())
            animationCyclingService.CycleAllNext();
        else if (inputConfiguration[Shortcut.CyclePreviousAnimation].IsDown())
            animationCyclingService.CycleAllPrevious();
    }
}

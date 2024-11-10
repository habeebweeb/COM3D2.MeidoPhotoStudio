using MeidoPhotoStudio.Plugin.Core.Database.Character;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class AnimationCyclingService
{
    private const int Previous = -1;
    private const int Next = 1;

    private readonly CharacterService characterService;
    private readonly GameAnimationRepository gameAnimationRepository;
    private readonly CustomAnimationRepository customAnimationRepository;
    private readonly CustomAnimationRepositorySorter customAnimationRepositorySorter;
    private readonly Dictionary<AnimationController, AnimationPointer> animationPointers = [];
    private readonly List<string> customAnimationCategoryCache = [];
    private readonly List<string> gameAnimationCategoryCache = [];
    private readonly Dictionary<string, List<IAnimationModel>> customAnimationCache = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<IAnimationModel>> gameAnimationCache = new(StringComparer.Ordinal);

    public AnimationCyclingService(
        CharacterService characterService,
        GameAnimationRepository gameAnimationRepository,
        CustomAnimationRepository customAnimationRepository,
        CustomAnimationRepositorySorter customAnimationRepositorySorter)
    {
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.gameAnimationRepository = gameAnimationRepository ?? throw new ArgumentNullException(nameof(gameAnimationRepository));
        this.customAnimationRepository = customAnimationRepository ?? throw new ArgumentNullException(nameof(customAnimationRepository));
        this.customAnimationRepositorySorter = customAnimationRepositorySorter ?? throw new ArgumentNullException(nameof(customAnimationRepositorySorter));

        this.customAnimationRepository.Refreshed += OnCustomAnimationsRefreshed;
        this.customAnimationRepository.AddedAnimation += OnCustomAnimationAdded;

        customAnimationCategoryCache.AddRange(customAnimationRepositorySorter.GetCategories(this.customAnimationRepository));

        if (gameAnimationRepository.Busy)
            gameAnimationRepository.InitializedAnimations += OnGameAnimationRepositoryReady;
        else
            gameAnimationCategoryCache.AddRange(gameAnimationRepository.Categories);

        this.characterService.CalledCharacters += OnCalledCharacters;

        void OnGameAnimationRepositoryReady(object sender, EventArgs e)
        {
            gameAnimationRepository.InitializedAnimations -= OnGameAnimationRepositoryReady;

            gameAnimationCategoryCache.AddRange(gameAnimationRepository.Categories);
        }
    }

    public void CycleAllNext()
    {
        var animations = animationPointers.Keys.ToArray();

        foreach (var animation in animations)
            Cycle(animation, Next);
    }

    public void CycleAllPrevious()
    {
        var animations = animationPointers.Keys.ToArray();

        foreach (var animation in animations)
            Cycle(animation, Previous);
    }

    public void CycleNext(CharacterController character)
    {
        _ = character ?? throw new ArgumentNullException(nameof(character));

        if (!animationPointers.ContainsKey(character.Animation))
            return;

        Cycle(character.Animation, Next);
    }

    public void CyclePrevious(CharacterController character)
    {
        _ = character ?? throw new ArgumentNullException(nameof(character));

        if (!animationPointers.ContainsKey(character.Animation))
            return;

        Cycle(character.Animation, Previous);
    }

    private static int Mod(int value, int length)
    {
        var remainder = value % length;

        return remainder < 0 ? remainder + length : remainder;
    }

    private void OnCalledCharacters(object sender, CharacterServiceEventArgs e)
    {
        var oldControllers = animationPointers.Keys.ToArray();

        foreach (var controller in oldControllers.Except(e.LoadedCharacters.Select(static character => character.Animation)))
        {
            controller.ChangedAnimation -= OnAnimationChanged;

            animationPointers.Remove(controller);
        }

        foreach (var animationController in e.LoadedCharacters.Select(static character => character.Animation).Except(oldControllers))
        {
            animationController.ChangedAnimation += OnAnimationChanged;
            animationPointers.Add(animationController, new(animationController.Animation, 0, 0));
        }
    }

    private void OnCustomAnimationsRefreshed(object sender, EventArgs e)
    {
        customAnimationCache.Clear();
        customAnimationCategoryCache.Clear();
        customAnimationCategoryCache.AddRange(customAnimationRepositorySorter.GetCategories(customAnimationRepository));

        var customControllers = animationPointers.Where(static kvp => kvp.Value.Animation.Custom).ToArray();

        foreach (var (controller, pointer) in customControllers)
        {
            var animation = (CustomAnimationModel)pointer.Animation;
            var categoryIndex = customAnimationCategoryCache.IndexOf(animation.Category, StringComparer.Ordinal);

            if (categoryIndex < 0)
            {
                animationPointers[controller] = pointer with { CategoryIndex = 0, AnimationIndex = 0 };

                continue;
            }

            var animationIndex = GetAnimations(animation.Category, true)
                .Cast<CustomAnimationModel>()
                .FindIndex(other => animation.ID == other.ID);

            if (animationIndex < 0)
                animationIndex = 0;

            animationPointers[controller] = pointer with { CategoryIndex = categoryIndex, AnimationIndex = animationIndex };
        }
    }

    private void OnCustomAnimationAdded(object sender, AddedAnimationEventArgs e)
    {
        if (!customAnimationCategoryCache.Contains(e.Animation.Category))
        {
            customAnimationCategoryCache.Clear();
            customAnimationCategoryCache.AddRange(customAnimationRepositorySorter.GetCategories(customAnimationRepository));

            UpdatePointers();

            return;
        }

        if (!customAnimationCache.ContainsKey(e.Animation.Category))
            return;

        var animations = customAnimationRepositorySorter.GetAnimations(e.Animation.Category, customAnimationRepository);
        var index = animations.IndexOf((CustomAnimationModel)e.Animation);

        customAnimationCache[e.Animation.Category].Insert(index, e.Animation);

        UpdatePointers();

        void UpdatePointers()
        {
            var pointers = animationPointers.Where(static kvp => kvp.Value.Animation.Custom).ToArray();
            var categories = GetCategories(true);

            foreach (var (controller, pointer) in pointers)
            {
                var (animation, oldAnimationIndex) = ((CustomAnimationModel)pointer.Animation, pointer.AnimationIndex);
                var newCategoryIndex = categories.IndexOf(animation.Category, StringComparer.Ordinal);
                var newAnimationIndex = string.Equals(animation.Category, e.Animation.Category, StringComparison.Ordinal)
                    ? GetAnimations(animation.Category, true).Cast<CustomAnimationModel>().FindIndex(other => animation.ID == other.ID)
                    : oldAnimationIndex;

                if (newAnimationIndex < 0)
                    newAnimationIndex = oldAnimationIndex;

                animationPointers[controller] = pointer with
                {
                    CategoryIndex = newCategoryIndex,
                    AnimationIndex = newAnimationIndex,
                };
            }
        }
    }

    private void OnAnimationChanged(object sender, EventArgs e)
    {
        var animationController = (AnimationController)sender;

        if (!animationPointers.ContainsKey(animationController))
        {
            animationController.ChangedAnimation -= OnAnimationChanged;

            return;
        }

        var animation = animationController.Animation;

        if (animationPointers[animationController].Animation.Equals(animation))
            return;

        if (gameAnimationRepository.Busy && !animation.Custom)
            return;

        animationPointers[animationController] = new(
            animation,
            GetCategories(animation.Custom).IndexOf(animation.Category),
            GetAnimations(animation.Category, animation.Custom).IndexOf(animation));
    }

    private void Cycle(AnimationController animationController, int direction)
    {
        var (animation, categoryIndex, animationIndex) = animationPointers[animationController];

        if (animationIndex + direction < 0)
        {
            var animations = CycleAnimation(categoryIndex, animation.Custom);

            if (animations.Count is 0)
                return;

            animationController.Apply(animations[animations.Count - 1]);
        }
        else if (animationIndex + direction >= GetAnimations(animation.Category, animation.Custom).Count)
        {
            var animations = CycleAnimation(categoryIndex, animation.Custom);

            if (animations.Count is 0)
                return;

            animationController.Apply(animations[0]);
        }
        else
        {
            var animations = GetAnimations(animation.Category, animation.Custom);

            animationController.Apply(animations[animationIndex + direction]);
        }

        List<IAnimationModel> CycleAnimation(int currentCategory, bool custom)
        {
            var sign = Math.Sign(direction);
            var categories = GetCategories(custom);

            for (var i = 1; i < categories.Count; i++)
            {
                var checkIndex = Mod(sign * i + currentCategory, categories.Count);

                var animations = GetAnimations(categories[checkIndex], custom);

                if (animations.Count > 0)
                    return animations;
            }

            return [];
        }
    }

    private List<IAnimationModel> GetAnimations(string category, bool custom)
    {
        if (custom)
        {
            if (!customAnimationCache.TryGetValue(category, out var list))
                list = customAnimationCache[category] =
                    [.. customAnimationRepositorySorter.GetAnimations(category, customAnimationRepository)];

            return list;
        }
        else
        {
            if (!gameAnimationCache.TryGetValue(category, out var list))
                list = gameAnimationCache[category] = [.. gameAnimationRepository[category]];

            return list;
        }
    }

    private List<string> GetCategories(bool custom) =>
        custom ? customAnimationCategoryCache : gameAnimationCategoryCache;

    private readonly record struct AnimationPointer(IAnimationModel Animation, int CategoryIndex, int AnimationIndex);
}

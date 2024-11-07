namespace MeidoPhotoStudio.Plugin.Core.Database.Character;

// TODO: At some point the various repository classes need to be generic in order to allow for this to be applicable to
// other repository types. Or even have this sort of thing composable so that this sorting strategy does not need to be
// passed alongside the repository.
public class CustomAnimationRepositorySorter
{
    private readonly IComparer<string> categoryComparer;
    private readonly IComparer<CustomAnimationModel> animationComparer;

    public CustomAnimationRepositorySorter(string rootCategory)
    {
        if (string.IsNullOrEmpty(rootCategory))
            throw new ArgumentException($"'{nameof(rootCategory)}' cannot be null or empty.", nameof(rootCategory));

        var comparer = new Framework.WindowsLogicalStringComparer();

        categoryComparer = Framework.ComparisonComparer<string>.Create((a, b) =>
            a == b ? 0 :
            string.Equals(a, rootCategory, StringComparison.Ordinal) ? -1 :
            string.Equals(b, rootCategory, StringComparison.Ordinal) ? 1 :
            comparer.Compare(a, b));

        animationComparer = Framework.ComparisonComparer<CustomAnimationModel>.Create((a, b) =>
            comparer.Compare(a.Filename, b.Filename));
    }

    public IEnumerable<string> GetCategories(CustomAnimationRepository repository)
    {
        _ = repository ?? throw new ArgumentNullException(nameof(repository));

        return repository.Categories.OrderBy(static category => category, categoryComparer);
    }

    public IEnumerable<CustomAnimationModel> GetAnimations(string category, CustomAnimationRepository repository)
    {
        if (string.IsNullOrEmpty(category))
            throw new ArgumentException($"'{nameof(category)}' cannot be null or empty.", nameof(category));

        _ = repository ?? throw new ArgumentNullException(nameof(repository));

        return repository[category].OrderBy(static animation => animation, animationComparer);
    }
}

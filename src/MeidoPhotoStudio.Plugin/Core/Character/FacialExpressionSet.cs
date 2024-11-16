namespace MeidoPhotoStudio.Plugin.Core.Character;

public class FacialExpressionSet : IEnumerable<KeyValuePair<string, float>>
{
    private readonly Dictionary<string, float> expressionValues;

    public FacialExpressionSet() =>
        expressionValues = [];

    public FacialExpressionSet(IDictionary<string, float> expressionValues)
    {
        _ = expressionValues ?? throw new ArgumentNullException(nameof(expressionValues));
        this.expressionValues = expressionValues.ToDictionary(static kvp => kvp.Key, static kvp => kvp.Value);
    }

    public FacialExpressionSet(IEnumerable<KeyValuePair<string, float>> expressionValues)
    {
        _ = expressionValues ?? throw new ArgumentNullException(nameof(expressionValues));
        this.expressionValues = expressionValues.ToDictionary(static kvp => kvp.Key, static kvp => kvp.Value);
    }

    public float this[string expressionKey] =>
        expressionValues[expressionKey];

    public bool ContainsExpressionKey(string expressionKey) =>
        expressionValues.ContainsKey(expressionKey);

    public bool TryGetExpressionValue(string expressionKey, out float value) =>
        expressionValues.TryGetValue(expressionKey, out value);

    public IEnumerator<KeyValuePair<string, float>> GetEnumerator() =>
        expressionValues.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}

namespace MeidoPhotoStudio.Plugin.Framework.Input;

public abstract class KeyboardInput(params KeyCode[] keys)
{
    private const KeyCode UpperKeyCode = KeyCode.RightApple;

    private static KeyCode[] keyPool = DefaultKeyPool;

    public static KeyCode[] KeyPool
    {
        get => (KeyCode[])keyPool.Clone();
        set
        {
            if (value is null || value.Length is 0)
            {
                keyPool = DefaultKeyPool;

                return;
            }

            keyPool = value.Where(static key => key is not KeyCode.None and < UpperKeyCode).ToArray();
        }
    }

    public IEnumerable<KeyCode> KeyList =>
        Keys;

    protected KeyCode[] Keys { get; } = keys;

    private static KeyCode[] DefaultKeyPool =>
        ((KeyCode[])Enum.GetValues(typeof(KeyCode)))
            .Where(static keyCode => keyCode is not KeyCode.None and < UpperKeyCode)
            .ToArray();

    public abstract bool IsDown();

    public abstract bool IsPressed();

    public abstract bool IsUp();

    public override int GetHashCode() =>
        Keys.Aggregate(Keys.Length, static (accumulator, keyCode) => unchecked(accumulator * 31 + (int)keyCode));

    public override string ToString() =>
        string.Join(" + ", Keys.Select(static key => key.ToString()).ToArray());

    protected static KeyCode[] DeserializeKeyCodes(string keyCodeList) =>
        keyCodeList.Split(new[] { '+', ' ', ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(static key => (KeyCode)Enum.Parse(typeof(KeyCode), key)).ToArray();

    protected bool NoOtherKeysPressed()
    {
        var myKeys = Keys;

        return !keyPool.Any(key => UnityEngine.Input.GetKey(key) && !myKeys.Contains(key));
    }

    protected string Serialize() =>
        ToString();
}

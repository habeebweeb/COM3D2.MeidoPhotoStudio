using BepInEx.Configuration;

namespace MeidoPhotoStudio.Plugin.Framework.Input;

/// <summary>
/// BepInEx's KeyboardShortcut but every key must be pressed and the order of the keys are pressed doesn't matter.
/// </summary>
public class KeyboardHotkey(params KeyCode[] keys) : KeyboardInput(SanitizeKeys(keys))
{
    public static readonly KeyboardHotkey Empty = new();

    static KeyboardHotkey() =>
        TomlTypeConverter.AddConverter(
            typeof(KeyboardHotkey),
            new()
            {
                ConvertToString = static (hotkey, _) => ((KeyboardHotkey)hotkey).Serialize(),
                ConvertToObject = static (data, _) => Deserialize(data),
            });

    public override bool IsDown() =>
        false;

    public override bool IsPressed() =>
        AllKeys() && NoOtherKeysPressed();

    public override bool IsUp() =>
        false;

    public override bool Equals(object obj) =>
        obj is KeyboardHotkey otherHotkey && Keys.SequenceEqual(otherHotkey.Keys);

    public override int GetHashCode() =>
        base.GetHashCode();

    private static KeyboardHotkey Deserialize(string data)
    {
        try
        {
            return new(DeserializeKeyCodes(data));
        }
        catch
        {
            return Empty;
        }
    }

    private static KeyCode[] SanitizeKeys(params KeyCode[] keys) =>
        keys.Length is 0 || keys.Any(static key => key is KeyCode.None)
            ? [KeyCode.None]
            : [.. keys.Distinct().OrderBy(static keyCode => (int)keyCode)];

    private bool AllKeys() =>
        Keys.All(UnityEngine.Input.GetKey);
}

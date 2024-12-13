namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public static class UIUtility
{
    private static readonly List<Texture> CreatedTextures = [];
    private static readonly GUILayoutOption[] LineHeight = [GUILayout.Height(1)];

    private static readonly LazyStyle WhiteLineStyle = new(
        0,
        static () => new(GUI.skin.box)
        {
            margin = new(0, 0, 0, 0),
            normal = { background = CreateTexture(2, 2, Color.white with { a = 0.3f }) },
            padding = new(0, 0, 1, 1),
            border = new(0, 0, 1, 1),
        });

    private static readonly LazyStyle BlackLineStyle = new(
        0,
        static () => new(GUI.skin.box)
        {
            margin = new(0, 0, 0, 0),
            normal = { background = CreateTexture(2, 2, Color.black) },
            padding = new(0, 0, 1, 1),
            border = new(0, 0, 1, 1),
        });

    public static int Scaled(int value)
    {
        var scaleX = Screen.width / 1920f;
        var scaleY = Screen.height / 1080f;

        var scale = 1f + (Mathf.Min(scaleX, scaleY) - 1f) * 0.6f;

        return Mathf.RoundToInt(scale * value);
    }

    public static int Scaled(float value)
    {
        var scaleX = Screen.width / 1920f;
        var scaleY = Screen.height / 1080f;

        var scale = 1f + (Mathf.Min(scaleX, scaleY) - 1f) * 0.6f;

        return Mathf.RoundToInt(scale * value);
    }

    public static Texture2D CreateTexture(int width, int height, Color color)
    {
        var colors = new Color32[width * height];

        for (var i = 0; i < colors.Length; i++)
            colors[i] = color;

        var texture2D = new Texture2D(width, height);

        texture2D.SetPixels32(colors);
        texture2D.Apply();

        CreatedTextures.Add(texture2D);

        return texture2D;
    }

    public static Texture2D LoadTextureFromBase64(int width, int height, string base64)
    {
        if (string.IsNullOrEmpty(base64))
            throw new ArgumentException($"'{nameof(base64)}' cannot be null or empty.", nameof(base64));

        var texture = new Texture2D(width, height, TextureFormat.RGB24, false);

        texture.LoadImage(Convert.FromBase64String(base64));

        texture.Apply();

        CreatedTextures.Add(texture);

        return texture;
    }

    public static void DrawWhiteLine() =>
        GUILayout.Box(GUIContent.none, WhiteLineStyle, LineHeight);

    public static void DrawBlackLine() =>
        GUILayout.Box(GUIContent.none, BlackLineStyle, LineHeight);

    internal static void Destroy()
    {
        foreach (var texture in CreatedTextures)
            if (texture)
                Object.DestroyImmediate(texture);
    }
}

namespace MeidoPhotoStudio.Plugin.Framework.Extensions;

public static class AFileBaseExtensions
{
    private static byte[] buffer;

    public static MemoryStream OpenStream(this AFileBase file)
    {
        _ = file ?? throw new ArgumentNullException(nameof(file));

        var fileSize = file.GetSize();

        if (fileSize is 0 || !file.IsValid())
            throw new InvalidOperationException("aFileBase is not valid");

        if (buffer is null)
            buffer = new byte[CeilPowerOf2(fileSize)];
        else if (file.GetSize() > buffer.Length)
            buffer = new byte[fileSize];

        file.Read(ref buffer, fileSize);

        return new MemoryStream(buffer);

        static int CeilPowerOf2(int x) =>
            x < 2
                ? 1
                : (int)Math.Pow(2, (int)Math.Log(x - 1, 2) + 1);
    }
}

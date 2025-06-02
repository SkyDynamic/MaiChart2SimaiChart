using AssetStudio;

namespace MaiChart2SimaiChart.Util;

public class ImageConvert
{
    public static byte[]? GetMusicJacketPngData(string file)
    {
        var manager = new AssetsManager();
        manager.LoadFiles(file);
        var asset = manager.assetsFileList[0].Objects.Find(it => it.type == ClassIDType.Texture2D);
        if (asset is null) return null;

        var texture = asset as Texture2D;
        return texture.ConvertToStream(ImageFormat.Png, false).GetBuffer();
    }
}
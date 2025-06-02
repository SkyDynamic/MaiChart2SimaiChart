using NAudio.Lame;
using NAudio.Wave;
using Standart.Hash.xxHash;

namespace MaiChart2SimaiChart.Util;

public static class AudioConvert
{
    public static async Task<string?> GetCachedWavPath(string acbAwbPath, int musicId)
    {
        var awb = $"{acbAwbPath}/music{(musicId % 10000):000000}.awb";

        string hash;
        await using (var readStream = File.OpenRead(awb))
        {
            hash = (await xxHash64.ComputeHashAsync(readStream)).ToString();
        }

        var cachePath = Path.Combine(StaticSettings.tempPath, hash + ".wav");
        
        if (!Directory.Exists(StaticSettings.tempPath)) Directory.CreateDirectory(StaticSettings.tempPath);
        
        if (File.Exists(cachePath)) return cachePath;

        var wav = Audio.AcbToWav($"{acbAwbPath}/music{(musicId % 10000):000000}.acb");
        await File.WriteAllBytesAsync(cachePath, wav);
        return cachePath;
    }

    public static void ConvertWavPathToMp3Stream(string wavPath, Stream mp3Stream)
    {
        using var reader = new WaveFileReader(wavPath);
        using var writer = new LameMP3FileWriter(mp3Stream, reader.WaveFormat, 256);
        reader.CopyTo(writer);
    }
}
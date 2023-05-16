using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.IO;
using System.Security.Cryptography;

namespace NanoAnalyzer;

public static class Utils
{
    public static void Shuffle<T>(this T[] array, Random rnd)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = rnd.Next(n--);
            (array[k], array[n]) = (array[n], array[k]);
        }
    }
    public static double Optimize(this double value, int optimize)
    {
        return (int)(value * optimize) / (double)optimize;
    }

    public static System.Drawing.Image ToSystem(this Image image)
    {
        using MemoryStream memory = new MemoryStream();
        image.SaveAsBmp(memory);
        memory.Seek(0, SeekOrigin.Begin);
        return System.Drawing.Image.FromStream(memory);
    }
    public static Image ToSixLabors(this System.Drawing.Image image)
    {
        using MemoryStream memory = new MemoryStream();
        image.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
        memory.Seek(0, SeekOrigin.Begin);
        return Image.Load(memory);
    }
    public static string HashMD5(this Image<Rgba32> image)
    {
        int width = image.Width;
        int height = image.Height;

        using HashAlgorithm hasher = MD5.Create();
        hasher.Initialize();

        byte[] versionHash = new byte[] { 0, 0, 1 };

        hasher.TransformBlock(versionHash, 0, versionHash.Length, null, 0);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Rgba32 color = image[x, y];
                byte[] buffer = new byte[] { color.R, color.G, color.B, color.A };
                hasher.TransformBlock(buffer, 0, buffer.Length, null, 0);

            }
        }
        hasher.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        return Convert.ToHexString(hasher.Hash!);
    }
}

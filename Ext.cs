using SixLabors.ImageSharp;
using System.IO;

namespace NanoAnalyzer
{
    public static class Ext
    {
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
    }
}

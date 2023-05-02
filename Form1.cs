using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using System;
using System.Windows.Forms;
using SixLabors.ImageSharp.ColorSpaces;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.IO;
using SixLabors.ImageSharp.Drawing;
using SixLabors.Fonts;

namespace NanoAnalyzer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private static Random rnd = new Random(10);
        private static void ResetRandom() => rnd = new Random(10);

        private Image<Rgba32>? imageBoxSrc = null;

        private void selectButton_Click(object sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Select image",
                Multiselect = false
            };
            if (dialog.ShowDialog() != DialogResult.OK) return;
            string img = dialog.FileName;
            Image<Rgba32> image = Image.Load<Rgba32>(img);
            image.Mutate(v => v.Grayscale());

            int width = image.Width;
            int height = image.Height;

            double minGray = 1;
            double maxGray = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    double gray = image[x, y].R / (double)byte.MaxValue;
                    if (gray < minGray) minGray = gray;
                    if (gray > maxGray) maxGray = gray;
                }
            }

            double deltaGray = maxGray - minGray;

            int WEIGHT_COUNT = 50;

            int split = byte.MaxValue / WEIGHT_COUNT;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    double gray = image[x, y].R / (double)byte.MaxValue;
                    gray -= minGray;
                    gray /= deltaGray;
                    byte _gray = (byte)Math.Round(gray * byte.MaxValue);
                    //_gray = (byte)((_gray / split) * split);
                    image[x, y] = new Rgba32(_gray, _gray, _gray);
                }
            }

            imageBoxSrc = image;
            imageBox.Image = image.ToSystem();
            extremumImage = null;

            //analyzePoint(rnd.NextDouble(), rnd.NextDouble());
        }

        private enum Mode
        {
            NONE,

            FLOOR,
            NORMAL,
            HALF,
            DEEP
        }

        private static Color GetModeColor(Mode mode) => mode switch
        {
            Mode.NONE => Color.Aqua,
            Mode.FLOOR => Color.Yellow,
            Mode.NORMAL => Color.Orange,
            Mode.HALF => Color.Red,
            Mode.DEEP => Color.Blue,

            _ => throw new NotImplementedException(),
        };
        private static Color GetModeColor(double mode) => new Rgba32(ColorSpaceConverter.ToRgb(new Hsv((float)mode * 360f, 1, 1)).ToVector3());

        private class NodeInfo
        {
            public double First { get; set; }
            public int Count { get; set; }
            public double Sum { get; set; }
            public double Modify { get; set; } = 0;
            public double Min { get; private set; } = 1;
            public double Max { get; private set; } = 0;

            private int changeTry = 0;

            public double ModeValue { get; private set; } //Mode Mode { get; private set; } = Mode.NONE;

            public Color Color => GetModeColor(ModeValue); // { get; } = new Rgba32(ColorSpaceConverter.ToRgb(new Hsv(rnd.NextSingle() * 360, rnd.NextSingle() * 0.5f + 0.5f, 1.0f)).ToVector3());

            private double GetMode(double value)
            {
                double average = Sum / Count;

                double firstDelta = Math.Abs(average - First);
                double lastDelta = Math.Abs(average - value);
                //Mode currentMode;

                double absDelta = Math.Abs(firstDelta - lastDelta);

                /*if (absDelta < 0.05) currentMode = Mode.FLOOR;
                else if (absDelta < 0.1) currentMode = Mode.NORMAL;
                else if (firstDelta > lastDelta) currentMode = Mode.HALF;
                else currentMode = Mode.DEEP;*/
                if (absDelta < 0.1) return absDelta;
                else return firstDelta / lastDelta;/* if (firstDelta > lastDelta) currentMode = Mode.HALF;
                else currentMode = Mode.DEEP;

                return currentMode;*/
            }

            public bool IsNext(double value)
            {
                if (Count < 2 || ModeValue == 0) return true;

                var currentMode = GetMode(value);

                if (ModeValue != currentMode)
                {
                    if (changeTry < 2)
                    {
                        changeTry++;
                        return true;
                    }
                    return false;
                }
                changeTry = 0;

                return true;
            }

            public void Append(double value)
            {
                if (value < Min) Min = value;
                if (value > Max) Max = value;
                if (Count == 0) First = value;

                if (Count > 1 && ModeValue == 0) ModeValue = GetMode(value);

                if (Count > 0) Modify += value - (Sum / Count);
                Sum += value;
                Count++;
            }
        }

        private void analyzePointDelta(double dx, double dy)
        {
            if (imageBoxSrc == null) return;

            using Image<Rgba32> selected = imageBoxSrc.Clone();

            int width = selected.Width;
            int height = selected.Height;

            int x = (int)Math.Round((width - 1) * dx);
            int y = (int)Math.Round((height - 1) * dy);

            analyzePoint(x, y);
        }


        private void DrawLine(Image<Rgba32> image, int x, int fromY, int toY, Rgba32 color)
        {
            if (fromY == toY)
            {
                image[x, fromY] = color;
                return;
            }

            if (fromY > toY) (fromY, toY) = (toY, fromY);

            for (int y = fromY; y < toY; y++)
            {
                image[x, y] = color;
            }
        }


        private void tPoint(Image<Rgba32> selected, int x, int y)
        {
            if (extremumImage == null)
            {
                string md5 = GetImageMD5(selected) + ".ext";
                if (File.Exists(md5))
                {
                    using Stream fileStream = File.OpenRead(md5);
                    using BinaryReader reader = new BinaryReader(fileStream);
                    extremumImage = ExtremumImage.Read(reader);
                }
                else
                {
                    extremumImage = ExtremumImage.GetExtremum(selected);
                    using Stream fileStream = File.Open(md5, FileMode.Create);
                    using BinaryWriter writer = new BinaryWriter(fileStream);
                    extremumImage.Write(writer);
                }
            }

            extremumImage.DrawBorder(selected);

            selected.Mutate(v =>
            {
                double radius = 0;
                int count = 0;
                (int x, int y) center = (x, y);
                List<(double value, (double x, double y) a, (double x, double y) o)> radiuses = new List<(double value, (double x, double y) a, (double x, double y) o)>();

                foreach (((int x, int y) a, (double x, double y) o) in extremumImage.GetPointsWithOffsets(x, y))
                {
                    count++;
                    double rValue = Math2D.Distance(a, center);
                    radiuses.Add((rValue, a, o));
                    radius += rValue;
                    v.DrawLines(Color.Aqua, 1, new Point(a.x, a.y), new Point(x, y));
                }

                /*List<(int x, int y)> pointsA = new List<(int x, int y)>();
                List<(int x, int y)> pointsB = new List<(int x, int y)>();

                foreach (((int x, int y) a, (int x, int y) b) in extremumImage.FindPointsCircle(x, y))
                {
                    pointsA.Add(a);
                    pointsB.Add(b);

                    count++;
                    double rValue = Math2D.Distance(a, b);
                    distances.Add(rValue);

                    radius += rValue;
                    center = (center.x + a.x + b.x, center.y + a.y + b.y);
                    v.DrawLines(Color.Aqua, 1, new Point(a.x, a.y), new Point(b.x, b.y));
                }
                if (pointsA.Count == 0) return;

                ((int x, int y) last, double delta) modify = (pointsA[0], 0);

                foreach ((int x, int y) point in pointsA.Concat(pointsB).Skip(1))
                {
                    (int x, int y) dPoint = ((int)(point.x * 0.25 + modify.last.x * 0.75), (int)(point.y * 0.25 + modify.last.y * 0.75)); 
                    //v.DrawLines(Color.DarkOliveGreen, 1, new Point(modify.last.x, modify.last.y), new Point(point.x, point.y));
                    v.DrawLines(Color.Orange, 1, new Point(modify.last.x, modify.last.y), new Point(dPoint.x, dPoint.y));
                    modify = (dPoint, modify.delta + Math2D.Distance(dPoint, modify.last));
                }*/
                if (count == 0) return;
                radius /= count;
                if (radius == 0) return;

                double maxRadius = radiuses.Max(v => v.value);
                double minRadius = radiuses.Min(v => v.value);
                double deltaRadius = maxRadius - minRadius;

                double sqrUP = 0;
                _ = radiuses.Aggregate((a, b) =>
                {
                    sqrUP += Math2D.TriangleSqr(
                        a.value <= radius ? (a.o.x * radius + x, a.o.y * radius + y) : a.a,
                        b.value <= radius ? (b.o.x * radius + x, b.o.y * radius + y) : b.a,
                        center
                    );
                    return b;
                });
                double sqrDOWN = 0;
                _ = radiuses.Aggregate((a, b) =>
                {
                    sqrDOWN += Math2D.TriangleSqr(
                        a.value >= radius ? (a.o.x * radius + x, a.o.y * radius + y) : a.a,
                        b.value >= radius ? (b.o.x * radius + x, b.o.y * radius + y) : b.a,
                        center
                    );
                    return b;
                });
                double sqr = 2 * Math.PI * radius * radius;

                /*foreach ((double value, (double x, double y) o) in radiuses)
                {
                    double dRadius = (value - minRadius);// / deltaRadius;
                    v.DrawLines(new Rgba32(1, 0, 0, 1f), 3, new Point((int)(o.x * dRadius + x), (int)(o.y * dRadius + y)), new Point(x, y));
                }*/
                //radius *= 1.25;

                EllipsePolygon polygon = new EllipsePolygon(center.x, center.y, (float)radius);
                v.Draw(new Pen(Color.Yellow, 1), polygon);
                string[] lines = new string[]
                {
                    $"Min: {radiuses.Min(v => v.value)}",
                    $"Avg: {radiuses.Average(v => v.value)}",
                    $"Max: {radiuses.Max(v => v.value)}",
                    $"Radius: {radius}",
                    $"Min L: {2 * Math.PI * radius}",
                    $"UP: {sqrUP / sqr}",
                    $"DOWN: {sqrDOWN / sqr}",
                    /*$"Delta R: {modify.delta / (2 * Math.PI * radius)}",
                    $"Delta A: {modify.delta / (2 * Math.PI * distances.Average())}",
                    "",
                    $"Modify: {modify.delta}"*/
                };
                v.DrawText(string.Join("\n", lines), new Font(SystemFonts.Get("Consolas"), 12), (sqrUP / sqr) > 0.4 || (sqrDOWN / sqr) > 0.4 ? Color.Red : Color.Yellow, new Point(center.x, center.y));
            });
        }

        private void analyzePoint(int x, int y)
        {
            if (imageBoxSrc == null) return;

            using Image<Rgba32> selected = imageBoxSrc.Clone();
            int width = selected.Width;
            int height = selected.Height;

            x = Math.Max(Math.Min(x, width - 1), 0);
            y = Math.Max(Math.Min(y, height - 1), 0);

            tPoint(selected, x, y);
            this.imageBox.Image = selected.ToSystem();

            return;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            selected.Mutate(v => v.MedianBlur(5, true));

            /*double __x = x / (double)width;
            double __y = y / (double)height;

            int variable = (int)Math.Round(__y * 100);

            switch ((int)Math.Round(__x * 3))
            {
                case 0: selected.Mutate(v => v.BokehBlur(variable, 2, 1f)); break;
                case 1: selected.Mutate(v => v.BoxBlur(variable)); break;
                case 2: selected.Mutate(v => v.GaussianBlur(variable)); break;
                case 3: selected.Mutate(v => v.MedianBlur(variable, true)); break;
            }*/

            Point minX = new Point(x, 0);
            Point maxX = new Point(x, height);

            Point minY = new Point(0, y);
            Point maxY = new Point(width, y);

            using Image<Rgba32> graphX = new Image<Rgba32>(50, height);

            graphX.Mutate(v =>
            {
                for (int _y = 0; _y < height; _y++)
                {
                    double gray = 1 - (selected[x, _y].R / (double)byte.MaxValue);
                    v.DrawLines(Color.Red, 1, new Point((int)Math.Round(gray * 49), _y), new Point(0, _y));
                }
            });

            using Image<Rgba32> graphY = new Image<Rgba32>(width, 50);

            ResetRandom();

            IEnumerable<double> points = Enumerable.Range(0, width)
                //.Select(_x => (int)((1 - (selected[_x, y].R / (double)byte.MaxValue)) * 16) / 16.0);
                .Select(_x => 1 - (selected[_x, y].R / (double)byte.MaxValue));

            int _x = 0;
            foreach (IExtremumInfo info in Math2D.GetExtremums(points, 50))
            {
                double minimum = info.Minimum;
                double maximum = info.Maximum;
                double delta = Math.Max(Math.Abs(minimum), Math.Abs(maximum));
                foreach ((double point, bool isMinimum) in info.Points)
                {
                    if (maximum - minimum == 0)
                    {
                        _x++;
                        continue;
                    }
                    double normalize = point;
                    normalize -= minimum;
                    normalize /= maximum - minimum;
                    //normalize /= delta;
                    /*double normalize = point;
                    normalize -= minimum;
                    normalize /= maximum - minimum;*/

                    Color color;
                    if (isMinimum) color = Color.Yellow;
                    else color = Color.Blue;

                    int minPoint = 0;
                    int maxPoint = 49;
                    int centerPoint = 24;
                    int offset = (int)Math.Round(normalize * centerPoint);
                    if (info is ExtremumInfo extremum)
                    {
                        int dot = (int)Math.Round(normalize * maxPoint);
                        if (isMinimum)
                        {
                            DrawLine(graphY, _x, minPoint, maxPoint,
                                extremum.Positive
                                ? Color.White
                                : Color.Blue
                            );
                        }
                        else
                        {
                            DrawLine(graphY, _x, dot, dot,
                                extremum.Positive
                                ? isMinimum
                                    ? Color.White
                                    : Color.Yellow
                                : isMinimum
                                    ? Color.Blue
                                    : Color.Aqua
                            );
                        }
                        /*if (extremum.Positive)
                        {
                            //int centerPoint = (int)Math.Round(normalize * maxPoint);
                            DrawLine(graphY, _x, centerPoint, centerPoint - offset, color);
                        }*/
                        //DrawLine(graphY, _x, centerPoint, extremum.Positive ? minPoint : maxPoint, Color.White);
                        //DrawLines(color, 1, centerPoint, extremum.Positive ? maxPoint : minPoint);
                    }
                    else
                    {
                        //DrawLine(graphY, _x, centerPoint, centerPoint + offset, Color.White);
                    }

                    //if (point > 0) v.DrawLines(color, new Point(_x, (int)Math.Round(normalize2 * 49)), new Point(_x, 1));
                    //else v.DrawLines(color, new Point(_x, (int)Math.Round(normalize2 * 49)), new Point(_x, 1));
                    _x++;
                }
                //if (sign == 0) v.DrawLines(new Pen(Color.White, 0.1f), new Point(_x, 49), new Point(_x, 0));
                //else if (sign > 0) v.DrawLines(new Pen(Color.Yellow, 0.1f), new Point(_x, (int)Math.Round(normalize2 * 49)), new Point(_x, 0));
                //else v.DrawLines(new Pen(Color.Blue, 0.1f), new Point(_x, (int)Math.Round(normalize2 * 49)), new Point(_x, 1));
            }

            /*int DELTA = 30;
    
            (double max, double min) border = (double.NegativeInfinity, double.PositiveInfinity);
            (double max, double min) border2 = (double.NegativeInfinity, double.PositiveInfinity);
            (double a, double b, double c)[] values = new (double a, double b, double c)[width];
            for (int _x = 0; _x < width; _x++)
            {
                double gray = 0;

                double GetGrayOffset(int offset_x)
                {
                    int __x = _x + offset_x;
                    if (__x < 0) return gray;
                    else if (__x >= width) return gray;
                    return (int)((1 - (selected[__x, y].R / (double)byte.MaxValue)) * 16) / 16.0;
                }

                gray = GetGrayOffset(0);

                double p1 = GetGrayOffset(-DELTA);
                double p2 = GetGrayOffset(DELTA);

                //double value = Math2D.DistanceWithSign((_x, gray), ((_x - DELTA, p1), (_x + DELTA, p2)));
                double value = Math2D.DistanceWithSign((_x, gray), ((_x - DELTA, p1), (_x + DELTA, p2)));
                double value2 = Math2D.Distance((_x - DELTA, p1), (_x + DELTA, p2));

                border = (Math.Max(value, border.max), Math.Min(value, border.min));
                border2 = (Math.Max(value2, border2.max), Math.Min(value2, border2.min));

                values[_x] = (gray, value, value2);
            }
            for (int _x = 0; _x < width; _x++)
            {
                double normalize2 = values[_x].c;
                normalize2 -= border2.min;
                normalize2 /= border2.max - border2.min;

                int sign = Math.Sign(values[_x].b);

                normalize2 *= sign;
                normalize2 += 1;
                normalize2 /= 2;

                if (sign == 0) v.DrawLines(new Pen(Color.White, 0.1f), new Point(_x, 49), new Point(_x, 0));
                else if (sign > 0) v.DrawLines(new Pen(Color.Yellow, 0.1f), new Point(_x, (int)Math.Round(normalize2 * 49)), new Point(_x, 0));
                else v.DrawLines(new Pen(Color.Blue, 0.1f), new Point(_x, (int)Math.Round(normalize2 * 49)), new Point(_x, 1));
            }*/

            selected.Mutate(v => v.DrawLines(Color.Red, 1, minX, maxX).DrawLines(Color.Blue, 1, minY, maxY));

            (int width, int height) current = GetRealScale(this.imageBox);

            //graphX.Mutate(v => v.Resize(50, current.height));
            //graphY.Mutate(v => v.Resize(current.width, 50));

            stopwatch.Stop();

            double generateSec = stopwatch.ElapsedMilliseconds / 1000.0;

            stopwatch.Start();
            this.imageBox.Image = selected.ToSystem();
            this.graphX.Image = graphX.ToSystem();
            this.graphY.Image = graphY.ToSystem();
            stopwatch.Stop();

            double showSec = stopwatch.ElapsedMilliseconds / 1000.0;

            Console.WriteLine($"Generate: {generateSec:N2} sec");
            Console.WriteLine($"Show: {showSec:N2} sec");
        }
        private static Rgba32 GetColor(int value, int total)
        {
            return new Rgba32((byte)((value * byte.MaxValue) / (double)total), 0, 0);
            /*
            Random rnd = new Random(value);
            return GetModeColor(rnd.NextDouble() * rnd.NextDouble());
            */
        }

        private static string GetImageMD5(Image<Rgba32> image)
        {
            int width = image.Width;
            int height = image.Height;

            using HashAlgorithm hasher = MD5.Create();
            hasher.Initialize();

            byte[] versionHash = new byte[] { 0, 0, 3 };

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

        private ExtremumImage? extremumImage = null;
        private void analyzeImage()
        {
            if (imageBoxSrc == null) return;


            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            using Image<Rgba32> selected = imageBoxSrc.Clone();
            int width = selected.Width;
            int height = selected.Height;
            if (extremumImage == null)
            {
                string md5 = GetImageMD5(selected) + ".ext";
                if (File.Exists(md5))
                {
                    using Stream fileStream = File.OpenRead(md5);
                    using BinaryReader reader = new BinaryReader(fileStream);
                    extremumImage = ExtremumImage.Read(reader);
                }
                else
                {
                    extremumImage = ExtremumImage.GetExtremum(selected);
                    using Stream fileStream = File.Open(md5, FileMode.Create);
                    using BinaryWriter writer = new BinaryWriter(fileStream);
                    extremumImage.Write(writer);
                }
            }

            var dct = extremumImage.ExtractElements(selected);
            double maxSize = dct.Select(v => v.Value).Max(v => v.size);

            int depCount = 50;

            int[] values = new int[depCount]; 

            foreach ((double size, int count) in dct.Values)
            {
                double deltaSize = size / maxSize;
                values[(int)Math.Round(deltaSize * (depCount - 1))] += count;
            }

            int maxValue = values.Max();

            using Image<Rgba32> graphY = new Image<Rgba32>(width, 50);
            graphY.Mutate(v =>
            {
                (int x, int y) lastPoint = (0, 49);
                for (int _x = 0; _x < depCount; _x++)
                {
                    int y = 49 - values[_x] * 49 / maxValue;
                    int x = _x * width / depCount;
                    v.DrawLines(Color.White, 1, new Point(lastPoint.x, lastPoint.y), new Point(x, y));
                    lastPoint = (x, y);
                    //graphY[x, y];
                }
            });

            graphY.Mutate(v => v.Resize(width, 50));
            this.graphY.Image = graphY.ToSystem();

            //extremumImage.Test(selected);


            //double[,] modes = new double[width, height];

            /*double maxMode = 0;
            double minMode = 1;

            for (int _y = 0; _y < height; _y++)
            {
                for (int _x = 0; _x < width; _x++)
                {
                    double gray = 1 - (selected[_x, _y].R / (double)byte.MaxValue);
                    double value = gray * 2 - 1;
                    modes[_x, _y] = value;

                    maxMode = Math.Max(value, maxMode);
                    minMode = Math.Min(value, minMode);
                }
            }
            for (int _y = 0; _y < height; _y++)
            {
                for (int _x = 0; _x < width; _x++)
                {
                    double value = (modes[_x, _y] - minMode) / (maxMode - minMode);
                    //value = (int)(value * 5) / 5.0;
                    selected[_x, _y] = GetColor((int)(value * 8), 8);//GetModeColor(value);
                }
            }*/



            /*
                 int DELTA = 30;

                (double max, double min) border = (double.NegativeInfinity, double.PositiveInfinity);
                (double max, double min) border2 = (double.NegativeInfinity, double.PositiveInfinity);
                (double a, double b, double c)[] values = new (double a, double b, double c)[width];
                for (int _x = 0; _x < width; _x++)
                {
                    double gray = 0;

                    double GetGrayOffset(int offset_x)
                    {
                        int __x = _x + offset_x;
                        if (__x < 0) return gray;
                        else if (__x >= width) return gray;
                        return (int)((1 - (selected[__x, y].R / (double)byte.MaxValue)) * 16) / 16.0;
                    }

                    gray = GetGrayOffset(0);

                    double p1 = GetGrayOffset(-DELTA);
                    double p2 = GetGrayOffset(DELTA);

                    //double value = Math2D.DistanceWithSign((_x, gray), ((_x - DELTA, p1), (_x + DELTA, p2)));
                    double value = Math2D.DistanceWithSign((_x, gray), ((_x - DELTA, p1), (_x + DELTA, p2)));
                    double value2 = Math2D.Distance((_x - DELTA, p1), (_x + DELTA, p2));

                    border = (Math.Max(value, border.max), Math.Min(value, border.min));
                    border2 = (Math.Max(value2, border2.max), Math.Min(value2, border2.min));

                    values[_x] = (gray, value, value2);
                }
                for (int _x = 0; _x < width; _x++)
                {
                    double normalize2 = values[_x].c;
                    normalize2 -= border2.min;
                    normalize2 /= border2.max - border2.min;

                    int sign = Math.Sign(values[_x].b);

                    normalize2 *= sign;
                    normalize2 += 1;
                    normalize2 /= 2;

                    if (sign == 0) v.DrawLines(new Pen(Color.White, 0.1f), new Point(_x, 49), new Point(_x, 0));
                    else if (sign > 0) v.DrawLines(new Pen(Color.Yellow, 0.1f), new Point(_x, (int)Math.Round(normalize2 * 49)), new Point(_x, 0));
                    else v.DrawLines(new Pen(Color.Blue, 0.1f), new Point(_x, (int)Math.Round(normalize2 * 49)), new Point(_x, 1));
                }
            */

            //NodeInfo node;

            /*node = new NodeInfo();
            for (int _y = 0; _y < height; _y++)
            {
                double gray = 1 - (selected[x, _y].R / (double)byte.MaxValue);
                double value = gray * 2 - 1;
                if (node.IsNext(value))
                {
                    node.Append(value);
                }
                else
                {
                    node = new NodeInfo();
                    node.Append(value);
                }
                modes[];
                v.DrawLines(Color.Red, 1, new Point((int)Math.Round(gray * 49), _y), new Point(0, _y));
            }*/

            /*for (int _y = 0; _y < height; _y++)
            {
                node = new NodeInfo();
                for (int _x = 0; _x < width; _x++)
                {
                    double gray = 1 - (selected[_x, _y].R / (double)byte.MaxValue);
                    double value = gray * 2 - 1;
                    if (node.IsNext(value))
                    {
                        node.Append(value);
                    }
                    else
                    {
                        node = new NodeInfo();
                        node.Append(value);
                    }
                    modes[_x, _y] = node.ModeValue;
                }
            }

            double maxMode = 0;
            double minMode = 1;

            for (int _x = 0; _x < width; _x++)
            {
                node = new NodeInfo();
                for (int _y = 0; _y < height; _y++)
                {
                    double gray = 1 - (selected[_x, _y].R / (double)byte.MaxValue);
                    double value = gray * 2 - 1;
                    if (node.IsNext(value))
                    {
                        node.Append(value);
                    }
                    else
                    {
                        node = new NodeInfo();
                        node.Append(value);
                    }
                    double mode;
                    modes[_x, _y] = mode = (node.ModeValue + modes[_x, _y]) / 2;//(Mode)Math.Max((int)node.Mode, (int)modes[_x, _y]);
                    maxMode = Math.Max(mode, maxMode);
                    minMode = Math.Min(mode, minMode);
                }
            }

            for (int _y = 0; _y < height; _y++)
            {
                for (int _x = 0; _x < width; _x++)
                {
                    selected[_x, _y] = GetModeColor((modes[_x, _y] - minMode) / (maxMode - minMode));
                }
            }*/

            stopwatch.Stop();

            double generateSec = stopwatch.ElapsedMilliseconds / 1000.0;

            stopwatch.Start();
            this.imageBox.Image = selected.ToSystem();
            stopwatch.Stop();

            double showSec = stopwatch.ElapsedMilliseconds / 1000.0;

            Console.WriteLine($"Generate: {generateSec:N2} sec");
            Console.WriteLine($"Show: {showSec:N2} sec");
        }

        private (int x, int y)? analyzeThis = null;

        private void tickUpdate_Tick(object sender, EventArgs e)
        {
            if (analyzeThis is (int x, int y)) analyzeThis = null;
            else return;
            analyzePoint(x, y);
        }

        private void imageBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && sender is PictureBox picture)
            {
                Point point = ImageCoord(picture, e.X, e.Y);
                analyzeThis = (point.X, point.Y);
            }
        }

        private static (int x, int y) GetRealScale(PictureBox sender)
        {
            int realW = sender.Image.Width;
            int realH = sender.Image.Height;
            int currentW = sender.ClientRectangle.Width;
            int currentH = sender.ClientRectangle.Height;
            double zoomW = currentW / (double)realW;
            double zoomH = currentH / (double)realH;
            double zoomActual = Math.Min(zoomW, zoomH);

            return ((int)(realW * zoomActual), (int)(realH * zoomActual));
        }

        private static Point ImageCoord(PictureBox sender, int mouseX, int mouseY)
        {
            int realW = sender.Image.Width;
            int realH = sender.Image.Height;
            int currentW = sender.ClientRectangle.Width;
            int currentH = sender.ClientRectangle.Height;
            double zoomW = (currentW / (double)realW);
            double zoomH = (currentH / (double)realH);
            double zoomActual = Math.Min(zoomW, zoomH);
            double padX = zoomActual == zoomW ? 0 : (currentW - (zoomActual * realW)) / 2;
            double padY = zoomActual == zoomH ? 0 : (currentH - (zoomActual * realH)) / 2;

            int realX = (int)((mouseX - padX) / zoomActual);
            int realY = (int)((mouseY - padY) / zoomActual);
            return new Point(realX < 0 || realX > realW ? 0 : realX, realY < 0 || realY > realH ? 0 : realY);
        }

        private void imageBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && sender is PictureBox picture)
            {
                Point point = ImageCoord(picture, e.X, e.Y);
                analyzeThis = (point.X, point.Y);
            }
        }

        private void genButton_Click(object sender, EventArgs e)
        {
            analyzeImage();
        }
    }
}
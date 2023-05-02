using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Linq;
using System;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.IO;
using SixLabors.ImageSharp.Drawing;
using static NanoAnalyzer.ExtremumImage;

namespace NanoAnalyzer;

public class ExtremumImage
{
    private readonly struct HValue
    {
        public readonly double value;
        public readonly bool minimum;

        public HValue(double value, bool minimum)
        {
            this.value = value;
            this.minimum = minimum;
        }

        public static HValue Read(BinaryReader reader)
        {
            return new HValue(reader.ReadDouble(), reader.ReadBoolean());
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write(value);
            writer.Write(minimum);
        }
    }

    private readonly int width;
    private readonly int height;
    private readonly (HValue hX, HValue hY)[,] hXY;
    private readonly (double rValue, double gValue)[,] rgValues;
    private readonly (double min, double max) cValue;
    private readonly bool[,] borderMap;

    private readonly (int x, int y)[] POINT_ARRAY;

    public static ExtremumImage Read(BinaryReader reader)
    {
        int width = reader.ReadInt32();
        int height = reader.ReadInt32();

        (HValue hX, HValue hY)[,] hXY = new (HValue hX, HValue hY)[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                hXY[x, y] = (HValue.Read(reader), HValue.Read(reader));
            }
        }




        return new ExtremumImage(hXY);
    }
    public void Write(BinaryWriter writer)
    {
        writer.Write(width);
        writer.Write(height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                (HValue hX, HValue hY) = hXY[x, y];
                hX.Write(writer);
                hY.Write(writer);
            }
        }
    }

    private ExtremumImage((HValue hX, HValue hY)[,] hXY)
    {
        this.hXY = hXY;
        this.width = hXY.GetLength(0);
        this.height = hXY.GetLength(1);
        POINT_ARRAY = new (int x, int y)[width * height];

        cValue = (double.PositiveInfinity, double.NegativeInfinity);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                POINT_ARRAY[y * width + x] = (x, y);

                double hXValue = hXY[x, y].hX.value;
                double hYValue = hXY[x, y].hY.value;

                cValue.max = Math.Max(Math.Max(Math.Abs(hXValue), Math.Abs(hYValue)), cValue.max);
                cValue.min = Math.Min(Math.Min(Math.Abs(hXValue), Math.Abs(hYValue)), cValue.min);
            }
        }
        borderMap = new bool[width, height];
        rgValues = new (double rValue, double gValue)[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                double hXValue = hXY[x, y].hX.value;
                double hYValue = hXY[x, y].hY.value;

                float rValue = (float)((hXValue - cValue.min) / (cValue.max - cValue.min));
                float gValue = (float)((hYValue - cValue.min) / (cValue.max - cValue.min));

                rgValues[x, y] = (rValue, gValue);

                bool isBorder;
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    isBorder = false;
                }
                else
                {
                    (double crValue, double cgValue) = (double.NegativeInfinity, double.NegativeInfinity);
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            (double drValue, double dgValue) = rgValues[x + dx, y + dy];

                            crValue = Math.Max(crValue, Math.Abs(rValue - drValue));
                            cgValue = Math.Max(cgValue, Math.Abs(gValue - dgValue));
                        }
                    }
                    isBorder = Math.Max((float)crValue, (float)cgValue) > 0.05 || Math.Max((float)rValue, (float)gValue) > 0.05;
                }

                borderMap[x, y] = isBorder;

            }
        }


    }
    public static ExtremumImage GetExtremum(Image<Rgba32> selected)
    {
        selected.Mutate(v => v.MedianBlur(5, true));

        int width = selected.Width;
        int height = selected.Height;

        (HValue hX, HValue hY)[,] hXY = new (HValue hX, HValue hY)[width, height];

        double OptimizeValue(double value)
        {
            return ((int)(value * 10)) / 10.0;
        }

        int radius = 10;
        int modifyOfRadius = radius * 2;

        for (int y = 0; y < height; y++)
        {
            IEnumerable<double> points = Enumerable.Range(0, width)
                .Select(_x => OptimizeValue(1 - (selected[_x, y].R / (double)byte.MaxValue)));

            int _x = 0;
            foreach (IExtremumInfo info in Math2D.GetExtremums(points, radius))
            {
                foreach ((double point, bool isMinimum) in info.Points)
                {
                    hXY[_x, y] = (new HValue(point - Math.Sign(point) * modifyOfRadius, isMinimum), default);
                    _x++;
                }
            }
        }


        for (int x = 0; x < width; x++)
        {
            IEnumerable<double> points = Enumerable.Range(0, height)
                .Select(_y => OptimizeValue(1 - (selected[x, _y].R / (double)byte.MaxValue)));

            int _y = 0;
            foreach (IExtremumInfo info in Math2D.GetExtremums(points, radius))
            {
                foreach ((double point, bool isMinimum) in info.Points)
                {
                    hXY[x, _y] = (hXY[x, _y].hX, new HValue(point - Math.Sign(point) * modifyOfRadius, isMinimum));
                    _y++;
                }
            }
        }

        return new ExtremumImage(hXY);
    }

    public void DrawExtremum(Image<Rgba32> selected)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                HValue hXData = hXY[x, y].hX;
                HValue hYData = hXY[x, y].hY;

                int sign = Math.Sign(hXData.value);

                float rValue = (float)((hXData.value - cValue.min) / (cValue.max - cValue.min));
                float gValue = (float)((hYData.value - cValue.min) / (cValue.max - cValue.min));

                rValue = (float)Math.Pow(rValue, 1.0 / 3.0);
                gValue = (float)Math.Pow(gValue, 1.0 / 3.0);

                selected[x, y] = new Rgba32(rValue, gValue, hXData.minimum ? hYData.minimum ? 1f : 0.75f : hYData.minimum ? 0.5f : 0.25f);

                /*if (rValue < 0.05 && gValue < 0.01)
                {
                    selected[x, y] = new Rgba32(rValue / 0.005f, gValue / 0.005f, 0f);
                }
                /*else if (rValue > 0.02 && gValue > 0.02)
                {
                    selected[x, y] = new Rgba32(0f, 0f, 1f);
                }*/
                /*else
                {
                    selected[x, y] = new Rgba32(0f, 0f, 0f);
                }

                /*if (hXData.minimum && hYData.minimum)
                {
                    selected[x, y] = Color.Yellow;
                }
                else
                {
                    selected[x, y] = rValue < 0.01 && gValue < 0.01 ? new Rgba32(0.3f, 0.3f, 0.3f) : new Rgba32(0f, 0f, 0f);//new Rgba32(rValue, gValue, rValue < 0.01 && gValue < 0.01 ? 1 : 0);
                }
                /*gValue,
                rValue < 0.5 && gValue < 0.5 ? 1 : 0*/

                /*if ((hXData.minimum || hYData.minimum) && sign == Math.Sign(hYData.value))
                {
                    selected[x, y] = sign == 1 ? Color.Blue : Color.Red;
                }*/

                /*double wValue;
                if (hXData.value >= 0 && hYData.value >= 0)
                    wValue = hXData.value + hYData.value;
                else
                    wValue = cValue.max;

                double normalize = wValue;
                normalize -= cValue.min;
                normalize /= cValue.max - cValue.min;
                Rgba32 c = selected[x, y];
                selected[x, y] = new Rgba32((float)normalize, 0, c.G / (float)byte.MaxValue);*/
            }
        }
    }
    public void Test(Image<Rgba32> selected)
    {
        SortZone(selected);
    }
    public void FillZone(Image<Rgba32> selected)
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                (double rValue, double gValue) = rgValues[x, y];
                (double crValue, double cgValue) = (double.NegativeInfinity, double.NegativeInfinity);
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        (double drValue, double dgValue) = rgValues[x + dx, y + dy];

                        crValue = Math.Max(crValue, Math.Abs(rValue - drValue));
                        cgValue = Math.Max(cgValue, Math.Abs(gValue - dgValue));
                    }
                }

                selected[x, y] = new Rgba32((float)crValue, (float)cgValue, 0f);

                /*
                int sign = Math.Sign(hXData.value);

                float rValue = (float)((hXData.value - cValue.min) / (cValue.max - cValue.min));
                float gValue = (float)((hYData.value - cValue.min) / (cValue.max - cValue.min));

                rValue = (float)Math.Pow(rValue, 1.0 / 3.0);
                gValue = (float)Math.Pow(gValue, 1.0 / 3.0);

                selected[x, y] = new Rgba32(rValue, gValue, hXData.minimum ? hYData.minimum ? 1f : 0.75f : hYData.minimum ? 0.5f : 0.25f);
                */
            }
        }
    }
    public List<(int px, int py)>[,] GroupPoints(Image<Rgba32> selected)
    {
        Random rnd = new Random(156);

        List<(int px, int py)>[,] moveCount = new List<(int px, int py)>[width,height];


        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                moveCount[x, y] = new List<(int px, int py)>();
            }
        }
        int maxValue = 0;

        for (int i = 0; i < 5000; i++)
        {
            int px = rnd.Next(0, width);
            int py = rnd.Next(0, height);

            foreach ((int x, int y) in FindBetterCircle2(rnd, px, py))
            {
                moveCount[x, y].Add((px, py));
                maxValue = Math.Max(moveCount[x, y].Count, maxValue);
            }
        }

        for (int i = 0; i < 5; i++)
        {
            MoveBox(rnd, moveCount, 5);
        }

        //MoveBox(rnd, moveCount, 20);

        return moveCount;
    }
    public void SortZone(Image<Rgba32> selected)
    {
        List<(int px, int py)>[,] moveCount = GroupPoints(selected);

        int maxValue = moveCount.Cast<List<(int px, int py)>>().Max(v => v.Count);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                List<(int px, int py)> list = moveCount[x, y];
                int count = list.Count;
                //if (count / (float)maxValue < 0.1 && count / (float)maxValue > 0.8) continue;
                selected.Mutate(v =>
                {
                    foreach ((int px, int py) in list)
                    {
                        v.DrawLines(new Rgba32(0, byte.MaxValue, byte.MaxValue, 2), 1, new Point(px, py), new Point(x, y));
                        selected[x, y] = new Rgba32(count / (float)maxValue, 0, 0);
                        //selected[px, py] = Color.Red;
                    }
                });
            }
        }
    }

    public SortedDictionary<int, int> ExtractSizes(Image<Rgba32> selected)
    {
        List<(int px, int py)>[,] moveCount = GroupPoints(selected);
        return CalculateSizes(moveCount);
    }
    public SortedDictionary<(int x, int y), (double size, int count)> ExtractElements(Image<Rgba32> selected)
    {
        List<(int px, int py)>[,] moveCount = GroupPoints(selected);
        var elements = CalculateElements(moveCount);
        if (elements.Count == 0) return elements;
        int maxCount = elements.Max(v => v.Value.count);
        selected.Mutate(v =>
        {
            foreach ((var point, var data) in elements)//.Select(kv => new EllipsePolygon(kv.Key.x, kv.Key.y, (float)kv.Value.)))
            {
                float value = data.count / (float)maxCount;
                if (value < 0.05) continue;
                EllipsePolygon polygon = new EllipsePolygon(point.x, point.y, (float)data.size);
                v.Draw(new Pen(new Rgba32(0, value, value), 1), polygon);
            }
        });
        return elements;
    }
    public void DrawBorder(Image<Rgba32> selected)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                selected[x, y] = borderMap[x, y] ? new Rgba32(0.1f, 0.1f, 0.1f) : new Rgba32(0, 0, 0);
            }
        }
    }

    private const double RAD_TO_DEG = 180 / Math.PI;
    private const double DEG_TO_RAD = Math.PI / 180;

    private IEnumerable<(int x, int y)> FindBetterCircle(Random rnd, int px, int py)
    {
        (int x, int y) BetterO(int x, int y, double ox, double oy)
        {
            (int x, int y) min = (x, y);
            for ((double _x, double _y) = (x, y); _x >= 0 && _y >= 0; _x -= ox, _y -= oy)
            {
                int __x = (int)_x;
                int __y = (int)_y;

                if (borderMap[__x, __y])
                {
                    min = (__x, __y);
                    break;
                }
            }

            (int x, int y) max = (x, y);
            for ((double _x, double _y) = (x, y); _x < width && _y < height; _x += ox, _y += oy)
            {
                int __x = (int)_x;
                int __y = (int)_y;

                if (borderMap[__x, __y])
                {
                    max = (__x, __y);
                    break;
                }
            }

            return ((min.x + max.x) / 2, (min.y + max.y) / 2);
        }

        for (int i = 0; i < 36; i++)
        {
            int a = rnd.Next(0, 36);

            double ox = Math.Abs(Math.Cos(a * 10 * DEG_TO_RAD));
            double oy = Math.Abs(Math.Sin(a * 10 * DEG_TO_RAD));

            yield return BetterO(px, py, ox, oy);
        }
    }
    private IEnumerable<(int x, int y)> FindBetterCircle2(Random rnd, int px, int py)
    {
        (int x, int y) BetterO(int x, int y, double ox, double oy)
        {
            (int x, int y) min = (x, y);
            for ((double _x, double _y) = (x, y); _x >= 0 && _y >= 0; _x -= ox, _y -= oy)
            {
                int __x = (int)_x;
                int __y = (int)_y;

                if (borderMap[__x, __y])
                {
                    min = (__x, __y);
                    break;
                }
            }

            (int x, int y) max = (x, y);
            for ((double _x, double _y) = (x, y); _x < width && _y < height; _x += ox, _y += oy)
            {
                int __x = (int)_x;
                int __y = (int)_y;

                if (borderMap[__x, __y])
                {
                    max = (__x, __y);
                    break;
                }
            }

            return ((min.x + max.x) / 2, (min.y + max.y) / 2);
        }

        int x = px;
        int y = py;

        for (int i = 0; i < 36; i++)
        {
            int a = rnd.Next(0, 36);

            double ox = Math.Abs(Math.Cos(a * 10 * DEG_TO_RAD));
            double oy = Math.Abs(Math.Sin(a * 10 * DEG_TO_RAD));

            (int _x, int _y) = BetterO(x, y, ox, oy);
            x = (_x + x) / 2;
            y = (_y + y) / 2;
        }

        yield return (x, y);
    }
    private IEnumerable<(int x, int y)> FindBetterBox(Random rnd, int px, int py)
    {
        (int point, int size) BetterX(int x, int y)
        {
            int minx = x;
            for (int _x = x; _x >= 0; _x--)
            {
                if (borderMap[_x, y])
                {
                    minx = _x;
                    break;
                }
            }
            int maxx = x;
            for (int _x = x; _x < width; _x++)
            {
                if (borderMap[_x, y])
                {
                    maxx = _x;
                    break;
                }
            }
            return ((minx + maxx) / 2, Math.Abs(minx - maxx));
        }

        int miny = py;
        (int point, int size) bx = BetterX(px, py);
        for (int _y = py; _y >= 0; _y--)
        {
            if (borderMap[bx.point, _y])
            {
                miny = _y;
                break;
            }
            (int point, int size) _bx = BetterX(bx.point, py);
            if (_bx.size < bx.size) break;
            bx = _bx;
            miny = _y;
        }
        int maxy = py;
        for (int _y = py; _y < height; _y++)
        {
            if (borderMap[bx.point, _y])
            {
                maxy = _y;
                break;
            }
            (int point, int size) _bx = BetterX(bx.point, py);
            if (_bx.size < bx.size) break;
            bx = _bx;
            maxy = _y;
        }

        yield return (bx.point, (miny + maxy) / 2);
    }
    private IEnumerable<(int x, int y)> FindBetterOutBox(Random rnd, int px, int py)
    {
        (int point, int size) BetterX(int x, int y)
        {
            int minx = x;
            bool wait = false;
            for (int _x = x; _x >= 0; _x--)
            {
                if (borderMap[_x, y])
                {
                    wait = true;
                    minx = _x;
                }
                else if (wait)
                {
                    minx = _x;
                    break;
                }
            }
            wait = false;
            int maxx = x;
            for (int _x = x; _x < width; _x++)
            {
                if (borderMap[_x, y])
                {
                    wait = true;
                    maxx = _x;
                }
                else if (wait)
                {
                    maxx = _x;
                    break;
                }
            }
            return ((minx + maxx) / 2, Math.Abs(minx - maxx));
        }

        int miny = py;
        (int point, int size) bx = BetterX(px, py);

        bool wait = false;
        for (int _y = py; _y >= 0; _y--)
        {
            if (borderMap[bx.point, _y])
            {
                wait = true;
                miny = _y;
            }
            else if (wait)
            {
                miny = _y;
                break;
            }
            (int point, int size) _bx = BetterX(bx.point, py);
            if (_bx.size < bx.size) break;
            bx = _bx;
            miny = _y;
        }
        wait = false;
        int maxy = py;
        for (int _y = py; _y < height; _y++)
        {
            if (borderMap[bx.point, _y])
            {
                wait = true;
                maxy = _y;
            }
            else if (wait)
            {
                maxy = _y;
                break;
            }
            (int point, int size) _bx = BetterX(bx.point, py);
            if (_bx.size < bx.size) break;
            bx = _bx;
            maxy = _y;
        }

        yield return (bx.point, (miny + maxy) / 2);
    }


    public IEnumerable<double> FindRadiusCircle(int px, int py)
    {
        double BetterR(int x, int y, double ox, double oy)
        {
            (int x, int y) min = (x, y);
            for ((double _x, double _y) = (x, y); _x >= 0 && _y >= 0; _x -= ox, _y -= oy)
            {
                int __x = (int)_x;
                int __y = (int)_y;

                if (borderMap[__x, __y])
                {
                    min = (__x, __y);
                    break;
                }
            }

            (int x, int y) max = (x, y);
            for ((double _x, double _y) = (x, y); _x < width && _y < height; _x += ox, _y += oy)
            {
                int __x = (int)_x;
                int __y = (int)_y;

                if (borderMap[__x, __y])
                {
                    max = (__x, __y);
                    break;
                }
            }

            return Math2D.Distance(min, max);
        }

        int x = px;
        int y = py;

        for (int i = 0; i < 18; i++)
        {
            double ox = Math.Abs(Math.Cos(i * 10 * DEG_TO_RAD));
            double oy = Math.Abs(Math.Sin(i * 10 * DEG_TO_RAD));

            yield return BetterR(x, y, ox, oy);
        }
    }
    public (int x, int y, double radius) FindDataCircle(int px, int py)
    {
        double radius = 0;
        int count = 0;
        (int x, int y) center = (px, py);

        List<(double value, (double x, double y) a, (double x, double y) o)> radiuses = new List<(double value, (double x, double y) a, (double x, double y) o)>();

        foreach (((int x, int y) a, (double x, double y) o) in GetPointsWithOffsets(px, py))
        {
            count++;
            double rValue = Math2D.Distance(a, center);
            radiuses.Add((rValue, a, o));
            radius += rValue;
        }
        
        if (count == 0) return (px, py, 0);
        radius /= count;
        if (radius == 0) return (px, py, 0);

        double sqrUP = 0;
        _ = radiuses.Aggregate((a, b) =>
        {
            sqrUP += Math2D.TriangleSqr(
                a.value <= radius ? (a.o.x * radius + px, a.o.y * radius + py) : a.a,
                b.value <= radius ? (b.o.x * radius + px, b.o.y * radius + py) : b.a,
                center
            );
            return b;
        });
        double sqrDOWN = 0;
        _ = radiuses.Aggregate((a, b) =>
        {
            sqrDOWN += Math2D.TriangleSqr(
                a.value >= radius ? (a.o.x * radius + px, a.o.y * radius + py) : a.a,
                b.value >= radius ? (b.o.x * radius + px, b.o.y * radius + py) : b.a,
                center
            );
            return b;
        });
        double sqr = 2 * Math.PI * radius * radius;
        if ((sqrUP / sqr) > 0.2 || (sqrDOWN / sqr) > 0.4) return (px, py, 0);
        return (center.x, center.y, radius);

        /*foreach (((int x, int y) a, (int x, int y) b) in FindPointsCircle(px, py))
        {
            count++;
            double rValue = Math2D.Distance(a, b);
            radius += rValue;
            center = (center.x + a.x + b.x, center.y + a.y + b.y);

            if (double.IsNaN(modify.last))
                modify = (rValue, modify.delta);
            else
                modify = (rValue, modify.delta + Math.Abs(rValue - modify.last));
        }
        if (count == 0) return (px, py, 0);
        double distance = radius / count;
        if (modify.delta > distance * 3.5) return (px, py, 0);
        radius /= count * 2;
        center = (center.x / (count * 2), center.y / (count * 2));
        if (radius == 0) return (px, py, 0);
        //radius *= 1.25;
        return (center.x, center.y, radius);*/
    }

    private ((int x, int y) a, (int x, int y) b) BetterR(int x, int y, double ox, double oy)
    {
        bool wait = false;
        (int x, int y) min = (x, y);

        for ((double _x, double _y) = (x, y); _x >= 0 && _y >= 0 && _x < width && _y < height; _x -= ox, _y -= oy)
        {
            int __x = (int)_x;
            int __y = (int)_y;

            if (borderMap[__x, __y])
            {
                wait = true;
                min = (__x, __y);
            }
            else if (wait)
            {
                min = (__x, __y);
                break;
            }
        }


        wait = false;
        (int x, int y) max = (x, y);

        for ((double _x, double _y) = (x, y); _x >= 0 && _y >= 0 && _x < width && _y < height; _x += ox, _y += oy)
        {
            int __x = (int)_x;
            int __y = (int)_y;

            if (borderMap[__x, __y])
            {
                wait = true;
                max = (__x, __y);
            }
            else if (wait)
            {
                max = (__x, __y);
                break;
            }
        }

        return (min, max);
    }

    public IEnumerable<((int x, int y) a, (int x, int y) b)> FindPointsCircle(int px, int py)
    {
        int x = px;
        int y = py;

        for (int i = 0; i < 18; i++)
        {
            double ox = Math.Cos(i * 10 * DEG_TO_RAD);
            double oy = Math.Sin(i * 10 * DEG_TO_RAD);

            yield return BetterR(x, y, ox, oy);
        }
    }

    public IEnumerable<((int x, int y) border, (double ox, double oy) circle)> GetPointsWithOffsets(int px, int py)
    {
        int x = px;
        int y = py;

        List<((int x, int y) border, (double ox, double oy) circle)> aValues = new List<((int x, int y) border, (double ox, double oy) circle)>();
        List<((int x, int y) border, (double ox, double oy) circle)> bValues = new List<((int x, int y) border, (double ox, double oy) circle)>();

        (int x, int y) center = (x, y);

        for (int i = 0; i < 18; i++)
        {
            double ox = Math.Cos(i * 10 * DEG_TO_RAD);
            double oy = Math.Sin(i * 10 * DEG_TO_RAD);

            ((int x, int y) a, (int x, int y) b) = BetterR(x, y, ox, oy);

            if (a == center || b == center) return Enumerable.Empty<((int x, int y) border, (double ox, double oy) circle)>();

            aValues.Add((a, (ox, oy)));
            bValues.Add((b, (-ox, -oy)));
        }
        return aValues.Concat(bValues);
    }

    private static void Shuffle<T>(Random rnd, T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = rnd.Next(n--);
            (array[k], array[n]) = (array[n], array[k]);
        }
    }

    private int MoveBox(Random rnd, List<(int px, int py)>[,] values, int radius)
    {
        int maxCount = 0;

        (int mx, int my)[,] moveValues = new (int mx, int my)[width,height];

        Shuffle(rnd, POINT_ARRAY);

        foreach ((int x, int y) in POINT_ARRAY)
        {
            moveValues[x, y] = (x, y);

            if (x <= radius || y <= radius || x >= width - 1 - radius || y >= height - 1 - radius) continue;

            int currCount;

            (int x, int y, int count) max = (x, y, currCount = values[x, y].Count);
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    List<(int px, int py)> _list = values[dx + x, dy + y];
                    int _count = _list.Count;

                    if (max.count < _count)
                        max = (dx + x, dy + y, _count);
                }
            }

            if (max.count == 0) continue;

            float value = currCount / (float)max.count;
            float uvalue = 1 - value;

            int vx = (int)Math.Round(x * value + max.x * uvalue);
            int vy = (int)Math.Round(y * value + max.y * uvalue);

            moveValues[x, y] = (vx, vy);
        }

        Shuffle(rnd, POINT_ARRAY);

        foreach ((int x, int y) in POINT_ARRAY)
        {
            (int mx, int my) = moveValues[x, y];

            List<(int x, int y)> list = values[x, y];
            if (list.Count == 0 || mx == x && my == y) continue;

            values[mx, my].AddRange(list);
            list.Clear();
        }

        return maxCount;
    }

    private SortedDictionary<int, int> CalculateSizes(List<(int px, int py)>[,] values)
    {
        SortedDictionary<int, int> sizes = new SortedDictionary<int, int>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                List<(int px, int py)> list = values[x, y];
                if (list.Count == 0) continue;

                (int x, int y) center = (x, y);
                foreach (var point in list)
                {
                    int size = (int)Math2D.Distance(point, center);
                    sizes.TryGetValue(size, out int count);
                    count++;
                    sizes[size] = count;
                }
            }
        }
        return sizes;
    }
    private SortedDictionary<(int x, int y), (double size, int count)> CalculateElements(List<(int px, int py)>[,] values)
    {
        SortedDictionary<(int x, int y), (double size, int count)> elements = new SortedDictionary<(int x, int y), (double size, int count)>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                List<(int px, int py)> list = values[x, y];
                int count = list.Count;
                if (count == 0) continue;

                /*(int x, int y) center = (x, y);
                double minD = double.PositiveInfinity;
                double maxD = double.NegativeInfinity;
                double avgD = 0;*/

                (int x, int y, double radius) circle = FindDataCircle(x, y);

                if (circle.radius == 0) continue;

                /*foreach (var radius in FindRadiusCircle(x, y))
                {
                    minD = Math.Min(minD, radius);
                    maxD = Math.Max(maxD, radius);
                    avgD += radius;
                }
                avgD /= count;
                double size = avgD * 1.3;
                //if (size < maxD * 0.75 || size * 0.25 > minD) continue;
                //double size = list.Average(v => Math2D.Distance(v, center));
                if (size <= 0) continue;*/
                elements[(circle.x, circle.y)] = (circle.radius, count);
            }
        }
        return elements;
    }
}

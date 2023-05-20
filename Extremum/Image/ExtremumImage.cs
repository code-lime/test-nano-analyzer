using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Linq;
using System;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.IO;
using NanoAnalyzer.Extremum.Find.Better;
using NanoAnalyzer.Extremum.Find;

namespace NanoAnalyzer.Extremum.Image;

public readonly struct ExtremumImage
{
    private static readonly IBetter findBetter = new BetterCircleAngle();

    public readonly int width;
    public readonly int height;
    public readonly EValue[,] data;
    public readonly (double min, double max) cValue;
    public readonly bool[,] borderMap;

    public readonly (int x, int y)[] POINT_ARRAY;

    public ExtremumImage(EValue[,] data)
    {
        this.data = data;
        width = data.GetLength(0);
        height = data.GetLength(1);
        borderMap = new bool[width, height];
        POINT_ARRAY = new (int x, int y)[width * height];

        cValue = (double.PositiveInfinity, double.NegativeInfinity);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                POINT_ARRAY[y * width + x] = (x, y);

                double hXValue = data[x, y].x.value;
                double hYValue = data[x, y].y.value;

                cValue.max = Math.Max(Math.Max(Math.Abs(hXValue), Math.Abs(hYValue)), cValue.max);
                cValue.min = Math.Min(Math.Min(Math.Abs(hXValue), Math.Abs(hYValue)), cValue.min);
            }
        }

        InitBorders();
    }

    private void InitBorders()
    {
        (double rValue, double gValue)[,] rgValues = new (double rValue, double gValue)[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                double hXValue = data[x, y].x.value;
                double hYValue = data[x, y].y.value;

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

    public List<(int px, int py)>[,] GroupPoints(int pointCount, Action<Frame>? progress)
    {
        Random rnd = new Random(156);

        List<(int px, int py)>[,] moveCount = new List<(int px, int py)>[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                moveCount[x, y] = new List<(int px, int py)>();
            }
        }
        int maxValue = 0;

        int moveBoxCount = 5;

        Frame pointCounter = Frame.Of(0, 1 + moveBoxCount);

        progress?.Invoke(pointCounter);

        for (int i = 0; i < pointCount; i++)
        {
            int px = rnd.Next(0, width);
            int py = rnd.Next(0, height);

            foreach ((int x, int y) in findBetter.FindBetter(rnd, px, py, borderMap))
            {
                moveCount[x, y].Add((px, py));
                maxValue = Math.Max(moveCount[x, y].Count, maxValue);
            }
            progress?.Invoke(pointCounter.Child(i, pointCount));
        }

        for (int i = 0; i < moveBoxCount; i++)
        {
            Frame moveBoxFrame = Frame.Of(1 + i, 1 + moveBoxCount);
            MoveBox(rnd, moveCount, 5, frame => progress?.Invoke(frame.GlobalParent(moveBoxFrame)));
        }

        //MoveBox(rnd, moveCount, 20);

        return moveCount;
    }
    public SortedDictionary<int, int> ExtractSizes(int pointCount, Action<Frame>? progress)
    {
        Frame groupFrame = Frame.Of(0, 2, "Group points");
        progress?.Invoke(groupFrame);
        List<(int px, int py)>[,] moveCount = GroupPoints(pointCount, frame => progress?.Invoke(frame.GlobalParent(groupFrame)));
        Frame sizesFrame = Frame.Of(1, 2, "Calculate sizes");
        return CalculateSizes(moveCount, frame => progress?.Invoke(frame.GlobalParent(sizesFrame)));
    }
    public IEnumerable<ElementData> ExtractElements(int pointCount, Action<Frame>? progress)
    {
        Frame groupFrame = Frame.Of(0, 3, "Group points");
        progress?.Invoke(groupFrame);
        List<(int px, int py)>[,] points = GroupPoints(pointCount, frame => progress?.Invoke(frame.GlobalParent(groupFrame)));
        Frame elementsFrame = Frame.Of(1, 3, "Calculate elements");
        progress?.Invoke(elementsFrame);
        List<ElementData> elements = CalculateElements(points, frame => progress?.Invoke(frame.GlobalParent(elementsFrame))).ToList();
        if (elements.Count == 0) yield break;
        int maxWeight = elements.Max(v => v.weight);
        progress?.Invoke(Frame.Of(2, 3, "Filter elements"));

        foreach (ElementData element in elements)
        {
            float value = element.weight / (float)maxWeight;
            if (value < 0.3) continue;
            yield return element;
        }
    }
    public IEnumerable<ElementData> ExtractElementsWithDraw(Image<Rgba32> selected, float width, int pointCount, Action<Frame>? progress)
    {
        List<ElementData> elements = ExtractElements(pointCount, progress).ToList();
        float maxWeight = elements.Max(v => v.weight);
        foreach (ElementData element in elements)
        {
            float value = element.weight / maxWeight;
            selected.Mutate(v => element.Draw(new Pen(new Rgba32(0.0f, value, value, value), width), v));
        }
        return elements;
    }
    public void DrawBorder(Image<Rgba32> selected, bool white = false)
    {
        Rgba32 BORDER_COLOR = white ? new Rgba32(0.9f, 0.9f, 0.9f) : new Rgba32(0.1f, 0.1f, 0.1f);
        Rgba32 NORMAL_COLOR = white ? new Rgba32(1f, 1f, 1f) : new Rgba32(0f, 0f, 0f);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                selected[x, y] = borderMap[x, y] ? BORDER_COLOR : NORMAL_COLOR;
            }
        }
    }
    public void DrawExtremum(Image<Rgba32> selected)
    {
        (double min, double max) minMaxX = (double.PositiveInfinity, double.NegativeInfinity);
        (double min, double max) minMaxY = (double.PositiveInfinity, double.NegativeInfinity);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                double hXValue = data[x, y].x.value;
                double hYValue = data[x, y].y.value;

                minMaxX.max = Math.Max(hXValue, minMaxX.max);
                minMaxX.min = Math.Min(hXValue, minMaxX.min);

                minMaxY.max = Math.Max(hYValue, minMaxY.max);
                minMaxY.min = Math.Min(hYValue, minMaxY.min);
            }
        }

        (double min, double delta) minDeltaX = (minMaxX.min, minMaxX.max - minMaxX.min);
        (double min, double delta) minDeltaY = (minMaxY.min, minMaxY.max - minMaxY.min);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                selected[x, y] = data[x, y].ToPixel(minDeltaX, minDeltaY);
            }
        }
    }

    private void MoveBox(Random rnd, List<(int px, int py)>[,] values, int radius, Action<Frame>? progress)
    {
        (int mx, int my)[,] moveValues = new (int mx, int my)[width, height];

        int length = POINT_ARRAY.Length;

        progress?.Invoke(Frame.Of(0, 3));

        POINT_ARRAY.Shuffle(rnd);

        Frame pointArray;
        pointArray = Frame.Of(1, 3);

        progress?.Invoke(pointArray);
        int i = 0;

        foreach ((int x, int y) in POINT_ARRAY)
        {
            progress?.Invoke(pointArray.Child(i, length));
            i++;
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

        progress?.Invoke(Frame.Of(2, 3));
        POINT_ARRAY.Shuffle(rnd);
        pointArray = Frame.Of(3, 3);

        progress?.Invoke(pointArray);
        i = 0;

        foreach ((int x, int y) in POINT_ARRAY)
        {
            progress?.Invoke(pointArray.Child(i, length));
            i++;
            (int mx, int my) = moveValues[x, y];

            List<(int x, int y)> list = values[x, y];
            if (list.Count == 0 || mx == x && my == y) continue;

            values[mx, my].AddRange(list);
            list.Clear();
        }
    }

    private SortedDictionary<int, int> CalculateSizes(List<(int px, int py)>[,] values, Action<Frame>? progress)
    {
        SortedDictionary<int, int> sizes = new SortedDictionary<int, int>();
        int i = 0;
        int length = width * height;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                progress?.Invoke(Frame.Of(i, length));
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
    private IEnumerable<ElementData> CalculateElements(List<(int px, int py)>[,] values, Action<Frame>? progress)
    {
        int i = 0;
        int length = width * height;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                progress?.Invoke(Frame.Of(i, length));
                List<(int px, int py)> list = values[x, y];
                int count = list.Count;
                if (count == 0) continue;

                (int x, int y, double radius) circle = DataCircle.FindDataCircle(x, y, borderMap);

                if (circle.radius == 0) continue;

                yield return new ElementData(circle.x, circle.y, circle.radius, count);
            }
        }
    }

    public static ExtremumImage Read(BinaryReader reader)
    {
        int width = reader.ReadInt32();
        int height = reader.ReadInt32();

        EValue[,] data = new EValue[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                data[x, y] = EValue.Read(reader);
            }
        }
        return new ExtremumImage(data);
    }
    public void Write(BinaryWriter writer)
    {
        writer.Write(width);
        writer.Write(height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                data[x, y].Write(writer);
            }
        }
    }
}

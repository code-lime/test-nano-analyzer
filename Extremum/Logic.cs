using NanoAnalyzer.Extremum.Image;
using NanoAnalyzer.Extremum.Part;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp.Processing;

namespace NanoAnalyzer.Extremum;

public static class Logic
{
    private static IEnumerable<double> GetExtremumValues(IEnumerable<double> points, int radius)
    {
        LinkedList<double?> around = new LinkedList<double?>(new double?[radius * 2 + 1]);
        foreach (double? point in points.OfType<double?>().Concat(Enumerable.Repeat<double?>(null, radius)))
        {
            around.RemoveFirst();
            around.AddLast(point);

            double? _first = null;
            double? _current = null;
            double? _last = null;

            int firstCount = 0;
            int lastCount = 0;

            int i = 0;
            foreach (double? _dat in around)
            {
                if (_dat is double dat)
                {
                    if (i <= radius)
                    {
                        if (_first is null) _first = dat;
                        else _first += dat;

                        firstCount++;
                    }
                    if (i == radius) _current = dat;
                    if (i >= radius)
                    {
                        if (_last is null) _last = dat;
                        else _last += dat;

                        lastCount++;
                    }
                }
                i++;
            }

            if (_current is not double current) continue;

            if (_first is double first) first /= firstCount;
            else first = current;
            if (_last is double last) last /= lastCount;
            else last = current;

            (double x, double y) a = (-radius, first);
            (double x, double y) b = (radius, last);
            (double x, double y) c = (0, current);

            yield return Math2D.Distance(a, b) * Math.Sign(Math2D.DistanceWithSign(c, (a, b)));
        }
    }

    public static IEnumerable<IPartInfo> GetExtremums(IEnumerable<double> points, int radius)
    {
        PartType infoType = PartType.None;
        List<double>? info = null;
        foreach (double value in GetExtremumValues(points, radius))
        {
            PartType type = value switch
            {
                double v when v > 0 => PartType.Positive,
                double v when v < 0 => PartType.Negative,
                _ => PartType.None,
            };
            if (info is null)
            {
                info = new List<double>() { value };
                infoType = type;
                continue;
            }
            if (infoType != type)
            {
                yield return infoType switch
                {
                    PartType.Positive => new PartInfo(true, info),
                    PartType.Negative => new PartInfo(false, info),
                    _ => new SimplePartInfo(info),
                };
                info = new List<double>();
                infoType = type;
            }
            info.Add(value);
        }
        if (info is null) yield break;
        yield return infoType switch
        {
            PartType.Positive => new PartInfo(true, info),
            PartType.Negative => new PartInfo(false, info),
            _ => new SimplePartInfo(info),
        };
    }

    public static ExtremumImage GetExtremumImage(Image<Rgba32> selected, Action<int, int>? progress = null)
    {
        selected.Mutate(v => v.MedianBlur(5, true));

        int width = selected.Width;
        int height = selected.Height;

        EValue[,] hXY = new EValue[width, height];

        int radius = 10;
        int modifyOfRadius = radius * 2;


        int frameIndex = 0;
        int frameTotal = height + width;

        progress?.Invoke(frameIndex, frameTotal);

        for (int y = 0; y < height; y++)
        {
            IEnumerable<double> points = Enumerable.Range(0, width)
                .Select(_x => 1 - selected[_x, y].R / (double)byte.MaxValue)
                .Select(_v => _v.Optimize(10));

            int _x = 0;
            foreach (IPartInfo info in GetExtremums(points, radius))
            {
                foreach ((double point, bool isMinimum) in info.Points)
                {
                    hXY[_x, y] = new EValue(new HValue(point - Math.Sign(point) * modifyOfRadius, isMinimum), default);
                    _x++;
                }
            }

            frameIndex++;
            progress?.Invoke(frameIndex, frameTotal);
        }


        for (int x = 0; x < width; x++)
        {
            IEnumerable<double> points = Enumerable.Range(0, height)
                .Select(_y => 1 - selected[x, _y].R / (double)byte.MaxValue)
                .Select(_v => _v.Optimize(10));

            int _y = 0;
            foreach (IPartInfo info in GetExtremums(points, radius))
            {
                foreach ((double point, bool isMinimum) in info.Points)
                {
                    hXY[x, _y] = hXY[x, _y].ChangeY(new HValue(point - Math.Sign(point) * modifyOfRadius, isMinimum));
                    _y++;
                }
            }

            frameIndex++;
            progress?.Invoke(frameIndex, frameTotal);
        }

        return new ExtremumImage(hXY);
    }
}

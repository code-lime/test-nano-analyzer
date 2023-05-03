using System;
using System.Collections.Generic;

namespace NanoAnalyzer.Extremum.Find.Better;

public class BetterBox : IBetter
{
    public IEnumerable<(int x, int y)> FindBetter(Random rnd, int px, int py, bool[,] border)
    {
        int width = border.GetLength(0);
        int height = border.GetLength(1);
        (int point, int size) BetterX(int x, int y)
        {
            int minx = x;
            for (int _x = x; _x >= 0; _x--)
            {
                if (border[_x, y])
                {
                    minx = _x;
                    break;
                }
            }
            int maxx = x;
            for (int _x = x; _x < width; _x++)
            {
                if (border[_x, y])
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
            if (border[bx.point, _y])
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
            if (border[bx.point, _y])
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
}

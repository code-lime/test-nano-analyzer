using System;
using System.Collections.Generic;

namespace NanoAnalyzer.Extremum.Find.Better;

public class BetterBoxOut : IBetter
{
    public IEnumerable<(int x, int y)> FindBetter(Random rnd, int px, int py, bool[,] border)
    {
        int width = border.GetLength(0);
        int height = border.GetLength(1);
        (int point, int size) BetterX(int x, int y)
        {
            int minx = x;
            bool wait = false;
            for (int _x = x; _x >= 0; _x--)
            {
                if (border[_x, y])
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
                if (border[_x, y])
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
            if (border[bx.point, _y])
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
            if (border[bx.point, _y])
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
}

using Avalonia;
using Avalonia.VisualTree;

namespace PixiDocks.Avalonia.Utils;

public static class CoordinatesUtil
{
    public static Point ToRelativePoint(Visual visual, int x, int y)
    {
        PixelPoint point = new PixelPoint(x, y);
        if(visual.GetVisualRoot() is null)
        {
            return new Point(-1, -1);
        }

        Point pos = visual.PointToClient(point);

        pos += visual.Bounds.Position;
        return pos;
    }


}
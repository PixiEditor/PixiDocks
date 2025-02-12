using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace PixiDocks.Avalonia.Helpers;

public static class WindowUtility
{
    public static StandardCursorType SetResizeCursor(PointerEventArgs e, Control relativeTo, Thickness borderThickness)
    {
        var direction = GetResizeDirection(e.GetPosition(relativeTo), relativeTo, borderThickness);
        if (direction is WindowEdge.West)
        {
            return StandardCursorType.LeftSide;
        }

        if (direction is WindowEdge.East)
        {
            return StandardCursorType.RightSide;
        }

        if (direction is WindowEdge.South)
        {
            return StandardCursorType.BottomSide;
        }

        if (direction is WindowEdge.North)
        {
            return (StandardCursorType.TopSide);
        }

        if (direction is WindowEdge.NorthWest)
        {
            return (StandardCursorType.TopLeftCorner);
        }

        if (direction is WindowEdge.NorthEast)
        {
            return (StandardCursorType.TopRightCorner);
        }

        if (direction is WindowEdge.SouthWest)
        {
            return (StandardCursorType.BottomLeftCorner);
        }

        if (direction is WindowEdge.SouthEast)
        {
            return (StandardCursorType.BottomRightCorner);
        }

        return (StandardCursorType.Arrow);
    }

    public static WindowEdge? GetResizeDirection(Point pt, Control resizeBorder, Thickness thickness)
    {
        if (pt.X < thickness.Left)
        {
            if (pt.Y < thickness.Top)
            {
                return WindowEdge.NorthWest;
            }

            if (pt.Y > resizeBorder.Bounds.Height - thickness.Bottom)
            {
                return WindowEdge.SouthWest;
            }

            return WindowEdge.West;
        }

        if (pt.X > resizeBorder.Bounds.Width - thickness.Right)
        {
            if (pt.Y < thickness.Top)
            {
                return WindowEdge.NorthEast;
            }

            if (pt.Y > resizeBorder.Bounds.Height - thickness.Bottom)
            {
                return WindowEdge.SouthEast;
            }

            return WindowEdge.East;
        }

        if (pt.Y < thickness.Top)
        {
            return WindowEdge.North;
        }

        if (pt.Y > resizeBorder.Bounds.Height - thickness.Bottom)
        {
            return WindowEdge.South;
        }

        return null;
    }
}

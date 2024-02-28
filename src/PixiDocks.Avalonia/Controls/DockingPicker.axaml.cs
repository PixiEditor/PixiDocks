using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace PixiDocks.Avalonia.Controls;

[TemplatePart("PART_Up", typeof(Image))]
[TemplatePart("PART_Down", typeof(Image))]
[TemplatePart("PART_Left", typeof(Image))]
[TemplatePart("PART_Right", typeof(Image))]
[TemplatePart("PART_Center", typeof(Image))]
public class DockingPicker : TemplatedControl
{
    private Image _up;
    private Image _down;
    private Image _left;
    private Image _right;
    private Image _center;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _up = e.NameScope.Get<Image>("PART_Top");
        _down = e.NameScope.Get<Image>("PART_Bottom");
        _left = e.NameScope.Get<Image>("PART_Left");
        _right = e.NameScope.Get<Image>("PART_Right");
        _center = e.NameScope.Get<Image>("PART_Center");
    }

    public DockingDirection? GetDockingDirection(Point? relativePoint)
    {
        if (relativePoint == null)
        {
            return null;
        }

        if (_up.Bounds.Contains(relativePoint.Value))
        {
            return DockingDirection.Top;
        }
        if (_down.Bounds.Contains(relativePoint.Value))
        {
            return DockingDirection.Bottom;
        }
        if (_left.Bounds.Contains(relativePoint.Value))
        {
            return DockingDirection.Left;
        }
        if (_right.Bounds.Contains(relativePoint.Value))
        {
            return DockingDirection.Right;
        }
        if (_center.Bounds.Contains(relativePoint.Value))
        {
            return DockingDirection.Center;
        }

        return null;
    }
}

public enum DockingDirection
{
    Top,
    Bottom,
    Left,
    Right,
    Center
}
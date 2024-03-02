using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using PixiDocks.Core;

namespace PixiDocks.Avalonia.Controls;

[TemplatePart("PART_Up", typeof(Image))]
[TemplatePart("PART_Down", typeof(Image))]
[TemplatePart("PART_Left", typeof(Image))]
[TemplatePart("PART_Right", typeof(Image))]
[TemplatePart("PART_Center", typeof(Image))]
public class DockingPicker : TemplatedControl
{
    public static readonly StyledProperty<ObservableCollection<IDockable>> DockablesProperty = AvaloniaProperty.Register<DockingPicker, ObservableCollection<IDockable>>(
        nameof(Dockables));

    public ObservableCollection<IDockable> Dockables
    {
        get => GetValue(DockablesProperty);
        set => SetValue(DockablesProperty, value);
    }
    private Image _up;
    private Image _down;
    private Image _left;
    private Image _right;
    private Image _center;
    private bool _initialized;

    static DockingPicker()
    {
        DockablesProperty.Changed.AddClassHandler<DockingPicker>((sender, args) =>
        {
            if (args.NewValue is ObservableCollection<IDockable> dockables)
            {
                dockables.CollectionChanged += (_, _) => DockablesOnCollectionChanged(sender);
            }
        });
    }

    private static void DockablesOnCollectionChanged(DockingPicker picker)
    {
        if (!picker._initialized)
        {
            return;
        }

        bool onlyCenter = picker.ShowOnlyCenter();

        picker._left.IsVisible = !onlyCenter;
        picker._right.IsVisible = !onlyCenter;
        picker._up.IsVisible = !onlyCenter;
        picker._down.IsVisible = !onlyCenter;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _up = e.NameScope.Get<Image>("PART_Top");
        _down = e.NameScope.Get<Image>("PART_Bottom");
        _left = e.NameScope.Get<Image>("PART_Left");
        _right = e.NameScope.Get<Image>("PART_Right");
        _center = e.NameScope.Get<Image>("PART_Center");
        _initialized = true;
    }

    public DockingDirection? GetDockingDirection(Point? relativePoint)
    {
        if (relativePoint == null || !_initialized)
        {
            return null;
        }

        if (_center.Bounds.Contains(relativePoint.Value))
        {
            return DockingDirection.Center;
        }

        if (ShowOnlyCenter())
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

        return null;
    }

    private bool ShowOnlyCenter()
    {
        return Dockables != null && Dockables.Count == 0;
    }
}
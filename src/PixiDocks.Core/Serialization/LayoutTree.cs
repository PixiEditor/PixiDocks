using System.Text.Json.Serialization;
using PixiDocks.Core.Docking;

namespace PixiDocks.Core.Serialization;

[JsonConverter(typeof(LayoutTreeConverter))]
public struct LayoutTree
{
    public static Dictionary<Type, Type> TypeMap = new Dictionary<Type, Type>();
    public static Dictionary<string, Type> TypeResolver = new Dictionary<string, Type>();

    public IDockableTree Root { get; set; }

    public bool IsFloating { get; set; } = false;
    public int? FloatingPositionX { get; set; }
    public int? FloatingPositionY { get; set; }
    public int? FloatingWidth { get; set; }
    public int? FloatingHeight { get; set; }

    public LayoutTree()
    {
        Root = null;
        FloatingPositionX = null;
        FloatingPositionY = null;
        FloatingWidth = null;
        FloatingHeight = null;
    }

    public void ApplyDockables(List<IDockable?> dockables)
    {
        foreach(var element in Root)
        {
            if (element is IDockableHost host)
            {
                foreach (var dockable in dockables)
                {
                    var found = host.Dockables.FirstOrDefault(d => d.Id == dockable.Id);
                    if (found != null)
                    {
                        dockable.Host?.RemoveDockable(dockable);
                        host.AddDockable(dockable);
                        host.RemoveDockable(found);
                    }
                }
            }
        };
    }

    public void SetContext(IDockContext dockContext)
    {
        foreach(var element in Root)
        {
            if (element is IDockableTarget target)
            {
                target.Context = dockContext;
            }
        };
    }
}
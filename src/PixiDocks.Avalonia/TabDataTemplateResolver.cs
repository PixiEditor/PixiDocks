using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Platform;
using PixiDocks.Core.Docking;

namespace PixiDocks.Avalonia;

public class TabDataTemplateResolver : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (Application.Current.TryGetResource(param.GetType(), out object? template))
        {
            return ((IDataTemplate)template).Build(param);
        }

        return new TextBlock { Text = param?.GetType().Name };
    }

    public bool Match(object? data)
    {
        return data is TabCustomizationSettings;
    }
}
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PixiDocks.Core.Docking;

public class TabCustomizationSettings : INotifyPropertyChanged
{
    private object? _icon;

    public object? Icon
    {
        get => _icon;
        set => SetField(ref _icon, value);
    }

    private bool _showCloseButton;

    public bool ShowCloseButton
    {
        get => _showCloseButton;
        set => SetField(ref _showCloseButton, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public TabCustomizationSettings()
    {
        Icon = null;
        ShowCloseButton = false;
    }

    public TabCustomizationSettings(object? icon = null, bool showCloseButton = false)
    {
        Icon = icon;
        ShowCloseButton = showCloseButton;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
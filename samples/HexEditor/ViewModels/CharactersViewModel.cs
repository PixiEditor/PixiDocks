using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiDocks.Core.Docking;

namespace HexEditor.ViewModels;

public class CharactersViewModel : ObservableObject, IDockableContent
{
    public string Id => "Inspector";
    public string Title => "Inspector";
    public bool CanFloat => true;
    public bool CanClose => true;
    public object? Icon { get; }

    private byte[]? _rawData;
    public byte[]? RawData
    {
        get => _rawData;
        set
        {
            SetProperty(ref _rawData, value);
            OnPropertyChanged(nameof(Characters));
        }
    }

    public string Characters => RawData != null ? Encoding.ASCII.GetString(RawData) : "Select a document to view characters";
}
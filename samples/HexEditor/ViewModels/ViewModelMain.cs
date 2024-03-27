using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HexEditor.Models;

namespace HexEditor.ViewModels;

public partial class ViewModelMain : ObservableObject
{
    public IStorageProvider StorageProvider { get; }

    public ObservableCollection<DocumentViewModel> Documents { get; } = new();

    public LayoutManager LayoutManager { get; } = new();

    public ViewModelMain()
    {

    }

    public ViewModelMain(IStorageProvider storageProvider)
    {
        StorageProvider = storageProvider;
    }

    [RelayCommand]
    public async Task OpenFile()
    {
        var file = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions());
        if(file.Count == 0)
        {
            return;
        }

        var document = new DocumentViewModel(new Document(file[0].Path.AbsolutePath));
        document.Load();
        Documents.Add(document);

        var documentArea = LayoutManager.DockContext.AllHosts.FirstOrDefault(x => x.Id == "DocumentArea");
        if(documentArea == null)
        {
            throw new InvalidOperationException("Document area not found");
        }

        LayoutManager.DockContext.Dock(LayoutManager.DockContext.CreateDockable(document), documentArea);
    }
}
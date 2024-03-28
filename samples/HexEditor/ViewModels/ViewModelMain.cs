using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HexEditor.Models;
using HexEditor.Views;

namespace HexEditor.ViewModels;

public partial class ViewModelMain : ObservableObject
{
    private DocumentViewModel? _selectedDocument;
    public IStorageProvider StorageProvider { get; }

    public ObservableCollection<DocumentViewModel> Documents { get; } = new();

    public DocumentViewModel? SelectedDocument
    {
        get => _selectedDocument;
        set => SetProperty(ref _selectedDocument, value);
    }
    public LayoutManager LayoutManager { get; } = new();

    private CharactersViewModel _charactersViewModel = new();

    public ViewModelMain()
    {

    }

    public ViewModelMain(IStorageProvider storageProvider)
    {
        StorageProvider = storageProvider;
        var inspectorArea = LayoutManager.DockContext.AllHosts.FirstOrDefault(x => x.Id == "InspectorArea");
        LayoutManager.DockContext.Dock(LayoutManager.DockContext.CreateDockable(_charactersViewModel), inspectorArea);
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
        document.Selected += () => SelectDocument(document);
        Documents.Add(document);

        var documentArea = LayoutManager.DockContext.AllHosts.FirstOrDefault(x => x.Id == "DocumentArea");
        if(documentArea == null)
        {
            throw new InvalidOperationException("Document area not found");
        }

        LayoutManager.DockContext.Dock(LayoutManager.DockContext.CreateDockable(document), documentArea);
        SelectDocument(document);
    }

    [RelayCommand]
    public void SelectDocument(DocumentViewModel document)
    {
        SelectedDocument = document;
        _charactersViewModel.RawData = document.Data;
    }
}
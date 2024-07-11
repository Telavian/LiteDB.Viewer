using LiteDB.Viewer.Client.Models;
using LiteDB.Viewer.Client.Pages.Shared;
using Microsoft.AspNetCore.Components.Forms;

namespace LiteDB.Viewer.Client.Pages.Dialogs;

public class ImportCollectionDialogViewModel : DialogViewModelBase
{
    public string Filename { get; set; } = "";
    public IBrowserFile? SelectedFile { get; set; }
    public string CollectionName { get; set; } = "";

    private Func<IBrowserFile, Task>? _loadImportFileAsyncCommand;
    public Func<IBrowserFile, Task> LoadImportFileAsyncCommand => _loadImportFileAsyncCommand ??= CreateEventCallbackAsyncCommand<IBrowserFile>(HandleLoadImportFileAsync, "Unable to load import file");

    private Func<Task>? _importAsyncCommand;
    public Func<Task> ImportAsyncCommand => _importAsyncCommand ??= CreateEventCallbackAsyncCommand(HandleImportAsync, "Unable to import");

    private async Task HandleLoadImportFileAsync(IBrowserFile browserFile)
    {
        await Task.Yield();
        Filename = browserFile.Name;
        SelectedFile = browserFile;
    }

    private async Task HandleImportAsync()
    {
        if (SelectedFile is null)
        {
            await ShowAlertAsync("No file is selected");
            return;
        }

        var results = new CollectionImportResults
        {
            Name = CollectionName,
            FileStream = SelectedFile.OpenReadStream(),                        
        };

        Dialog?.Close(results);
    }
}

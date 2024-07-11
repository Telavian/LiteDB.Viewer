using LiteDB.Viewer.Client.Models;
using LiteDB.Viewer.Client.Pages.Shared;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace LiteDB.Viewer.Client.Pages.Dialogs;

public class ConnectDatabaseDialogViewModel : DialogViewModelBase
{
    public string Filename { get; set; } = "";
    public IBrowserFile? SelectedFile { get; set; }
    public string? Password { get; set; }
    public bool IsReadOnly { get; set; } = false;
    public bool IsUpgrade { get; set; } = false;

    public InputType PasswordInputType { get; set; } = InputType.Password;
    public string PasswordInputIcon { get; set; } = Icons.Material.Filled.VisibilityOff;

    private Func<IBrowserFile, Task>? _loadDatabaseFileAsyncCommand;
    public Func<IBrowserFile, Task> LoadDatabaseFileAsyncCommand => _loadDatabaseFileAsyncCommand ??= CreateEventCallbackAsyncCommand<IBrowserFile>(HandleLoadDatabaseFileAsync, "Unable to load database file");

    private Func<Task>? _connectAsyncCommand;
    public Func<Task> ConnectAsyncCommand => _connectAsyncCommand ??= CreateEventCallbackAsyncCommand(HandleConnectAsync, "Unable to connect");

    private async Task HandleLoadDatabaseFileAsync(IBrowserFile browserFile)
    {
        await Task.Yield();
        Filename = browserFile.Name;
        SelectedFile = browserFile;
    }

    private async Task HandleConnectAsync()
    {
        if (SelectedFile is null)
        {
            await ShowAlertAsync("No file is selected");
            return;
        }

        var results = new DatabaseConnectResults
        {
            Name = Filename,
            FileStream = SelectedFile.OpenReadStream(),
            IsReadOnly = IsReadOnly,
            IsUpgrade = IsUpgrade,
        };

        Dialog?.Close(results);
    }
}

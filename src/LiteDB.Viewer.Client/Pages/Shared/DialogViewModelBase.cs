using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LiteDB.Viewer.Client.Pages.Shared;

public class DialogViewModelBase : ViewModelBase
{
    [CascadingParameter]
    public MudDialogInstance? Dialog { get; set; } = null!;

    private Func<Task>? _closeDialogAsyncCommand;
    public Func<Task> CloseDialogAsyncCommand => _closeDialogAsyncCommand ??= CreateEventCallbackAsyncCommand(() => HandleCloseDialogAsync(), "Unable to close dialog");

    private async Task HandleCloseDialogAsync()
    {
        await Task.Yield();
        Dialog?.Close();
    }
}

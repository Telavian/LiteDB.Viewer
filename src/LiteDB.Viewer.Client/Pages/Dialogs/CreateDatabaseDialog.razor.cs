using LiteDB.Viewer.Client.Models;
using LiteDB.Viewer.Client.Pages.Shared;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.Globalization;

namespace LiteDB.Viewer.Client.Pages.Dialogs;

public class CreateDatabaseDialogViewModel : DialogViewModelBase
{
    public string Name { get; set; } = "";    
    public int InitialSize { get; set; }
    public string SelectedCulture { get; set; } = "";
    public string SelectedSort { get; set; } = "";

    public string[] AllCultures { get; } = CultureInfo.GetCultures(CultureTypes.AllCultures)
                                                .Select(x => x.LCID)
                                                .Distinct()
                                                .Where(x => x != 4096)
                                                .Select(x => CultureInfo.GetCultureInfo(x).Name)
                                                .ToArray();

    public string[] AllSorts { get; } = Enum.GetValues<CompareOptions>()
                                            .Select(x => x.ToString())
                                            .ToArray();

    private Func<Task>? _createAsyncCommand;
    public Func<Task> CreateAsyncCommand => _createAsyncCommand ??= CreateEventCallbackAsyncCommand(HandleCreateAsync, "Unable to create");

    private async Task HandleCreateAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await ShowAlertAsync("No name entered");
            return;
        }

        var results = new DatabaseCreateResults
        {
            Name = Name,
            InitialSize = InitialSize,
            Culture = SelectedCulture,
            Sort = SelectedSort,
        };

        Dialog?.Close(results);
    }
}

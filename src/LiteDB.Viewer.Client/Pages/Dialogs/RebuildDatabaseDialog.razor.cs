using LiteDB.Viewer.Client.Models;
using LiteDB.Viewer.Client.Pages.Shared;
using System.Globalization;

namespace LiteDB.Viewer.Client.Pages.Dialogs;

public class RebuildDatabaseDialogViewModel : DialogViewModelBase
{
    public string Name { get; set; } = "";    
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

    private Func<Task>? _rebuildAsyncCommand;
    public Func<Task> RebuildAsyncCommand => _rebuildAsyncCommand ??= CreateEventCallbackAsyncCommand(HandleRebuildAsync, "Unable to rebuild");

    private async Task HandleRebuildAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await ShowAlertAsync("No name entered");
            return;
        }

        var results = new DatabaseRebuildResults
        {
            Name = Name,
            Culture = SelectedCulture,
            Sort = SelectedSort,
        };

        Dialog?.Close(results);
    }
}

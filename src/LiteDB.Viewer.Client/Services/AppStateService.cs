using LiteDB.Viewer.Client.Services.Interfaces;

namespace LiteDB.Viewer.Client.Services;

public class AppStateService : IAppStateService
{
    public bool IsDBOpen { get; set; }
    public LiteDatabase? Database { get; set; }
    public string? DatabaseName { get; set; }
    public Stream? DatabaseStream { get; set; }
}

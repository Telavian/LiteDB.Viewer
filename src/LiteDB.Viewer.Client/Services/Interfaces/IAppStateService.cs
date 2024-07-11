namespace LiteDB.Viewer.Client.Services.Interfaces
{
    public interface IAppStateService
    {
        public bool IsDBOpen { get; set; }
        public LiteDatabase? Database { get; set; }
        public string? DatabaseName { get; set; }
        public Stream? DatabaseStream { get; set; }
    }
}

namespace LiteDB.Viewer.Client.Models
{
    public class DBQuery
    {
        public Guid QueryId { get; } = Guid.NewGuid();
        public required string Header { get; init; }

        public string Query { get; set; } = "";

        public string JsonResult { get; set; } = "";
    }
}

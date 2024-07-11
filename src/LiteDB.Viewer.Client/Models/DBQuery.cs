namespace LiteDB.Viewer.Client.Models;

public class DBQuery
{
    public static DBQuery Null { get; } = new DBQuery { Header = "" };

    public Guid QueryId { get; } = Guid.NewGuid();
    public required string Header { get; init; }

    public string Query { get; set; } = "";

    public string JsonResult { get; set; } = "";
    public DBQueryGridRecords GridRecords { get; set; } = new DBQueryGridRecords();
}

namespace LiteDB.Viewer.Client.Models;

public class DBQueryGridRecords
{
    public string[] AllColumns { get; set; } = [];
    public DBQueryGridRecord[] Records { get; set; } = [];
}

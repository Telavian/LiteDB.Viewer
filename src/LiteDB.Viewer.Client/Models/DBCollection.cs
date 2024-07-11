namespace LiteDB.Viewer.Client.Models;

public class DBCollection
{
    public required string Name { get; init; } = "";
    public required string Icon { get; init; } = "";
    public required bool IsRoot { get; set; }
    public DBCollection[] Children { get; set; } = [];
}

namespace LiteDB.Viewer.Client.Models;

public class DatabaseConnectResults
{
    public string Name { get; set; } = "";
    public Stream? FileStream { get; set; }
    public bool IsReadOnly { get; set; } = false;
    public bool IsUpgrade { get; set; } = false;
}

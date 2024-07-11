namespace LiteDB.Viewer.Client.Models;

public class DBQueryGridRecord
{
    private Dictionary<string, string> _data = new Dictionary<string, string>();
    
    public string[] AllColumns => _data.Keys.ToArray();

    public bool HasData()
    {
        return _data.Count > 0;
    }

    public void AddData(string key, string value)
    {
        _data[key] = value;
    }

    public string? GetRowData(string column)
    {
        var isFound = _data.TryGetValue(column, out var data);

        return isFound
            ? data
            : null;
    }
}

namespace D2SLib.Model.Data;

public abstract class DataFile
{
    public Dictionary<string, int> Columns { get; } = new();
    public List<DataRow> Rows { get; } = new();

    protected void ReadData(Stream data)
    {
        Columns.Clear();
        Rows.Clear();

        using var reader = new StreamReader(data);

        //skip header
        int idx = 0;
        var columns = reader.ReadLine()?.Split('\t') ?? Array.Empty<string>();
        foreach (var col in columns)
        {
            Columns.TryAdd(col, idx++);
        }
        while (reader.Peek() >= 0)
        {
            Rows.Add(new DataRow(Columns, reader.ReadLine()?.Split('\t') ?? Array.Empty<string>()));
        }
    }

    public DataRow? GetByColumnAndValue(string name, ReadOnlySpan<char> value)
    {
        //Console.WriteLine(name);
        //Console.WriteLine(value.ToString());
        foreach (var row in Rows)
        {
            if (row[name].Value.AsSpan().Trim().Equals(value.Trim(), StringComparison.Ordinal))
            {
                return row;
            }
        }
        return null;
    }
}
public class DataRow
{
    public Dictionary<string, int> Columns { get; set; }
    public DataCell[] Data { get; set; }

    public DataCell this[int i] => GetByIndex(i);
    public DataCell this[string i] => GetByColumn(i);

    public DataRow(Dictionary<string, int> columns, string[] data)
    {
        Columns = columns;
        Data = data.Select(e => new DataCell(e)).ToArray();
    }

    public DataCell GetByIndex(int idx) => Data[idx];

    public DataCell GetByColumn(string col) => GetByIndex(Columns[col]);
}

public class DataCell
{
    public string Value { get; set; }

    public int ToInt32()
    {
        int.TryParse(Value, out int ret);
        return ret;
    }

    public uint ToUInt32()
    {
        uint.TryParse(Value, out uint ret);
        return ret;
    }

    public ushort ToUInt16()
    {
        ushort.TryParse(Value, out ushort ret);
        return ret;
    }

    public short ToInt16()
    {
        short.TryParse(Value, out short ret);
        return ret;
    }

    public bool ToBool() => ToInt32() != 0;

    public DataCell(string value)
    {
        Value = value;
    }
}

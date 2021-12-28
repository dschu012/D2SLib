using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

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
        foreach (var col in reader.ReadLine()!.Tokenize('\t'))
        {
            Columns.TryAdd(StringPool.Shared.GetOrAdd(col), idx++);
        }

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            Rows.Add(new DataRow(Columns, line.AsSpan()));
        }
    }

    public DataRow? GetByColumnAndValue(string name, ReadOnlySpan<char> value)
    {
        if (Columns.TryGetValue(name, out var colIdx))
        {
            foreach (var row in Rows)
            {
                if (row[colIdx].Value.AsSpan().Trim().Equals(value.Trim(), StringComparison.Ordinal))
                {
                    return row;
                }
            }
        }
        return null;
    }

    public DataRow? GetByColumnAndValue(string name, int value)
    {
        if (Columns.TryGetValue(name, out var colIdx))
        {
            foreach (var row in Rows)
            {
                if (row[colIdx].ToInt32() == value)
                {
                    return row;
                }
            }
        }
        return null;
    }
}

public sealed class DataRow
{
    public DataRow(IReadOnlyDictionary<string, int> columns, ReadOnlySpan<char> data)
    {
        Columns = columns;
        var cells = new List<DataCell>(columns.Count);
        foreach (var value in data.Tokenize('\t'))
        {
            cells.Add(DataCell.Create(value));
        }
        Data = cells.ToArray();
    }

    public IReadOnlyDictionary<string, int> Columns { get; }
    public DataCell[] Data { get; }

    public DataCell this[int i] => Data[i];
    public DataCell this[string colName] => Data[Columns[colName]];
}

public abstract class DataCell
{
    public abstract string Value { get; }
    public abstract int ToInt32();
    public abstract ushort ToUInt16();
    public abstract bool ToBool();

    public static DataCell Create(ReadOnlySpan<char> value)
    {
        if (int.TryParse(value, out int intVal))
        {
            return new Int32DataCell(intVal);
        }

        return new StringDataCell(value.IsEmpty ? string.Empty : StringPool.Shared.GetOrAdd(value));
    }
}

public sealed class StringDataCell : DataCell
{
    public StringDataCell(string value) => Value = value;

    public override string Value { get; }

    public override int ToInt32() => 0;
    public override ushort ToUInt16() => 0;
    public override bool ToBool() => false;
}

public sealed class Int32DataCell : DataCell
{
    private readonly int _value;
    public Int32DataCell(int value) => _value = value;

    public override string Value => _value.ToString();
    public override int ToInt32() => _value;
    public override ushort ToUInt16() => (ushort)_value;
    public override bool ToBool() => _value != 0;
}


using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;
using System.Diagnostics;

namespace D2SLib.Model.Data;

public abstract class DataFile
{
    private DataColumn[] _columns = Array.Empty<DataColumn>();
    private readonly Dictionary<string, int> _columnsLookup = new();

    public IReadOnlyDictionary<string, int> ColumnNames => _columnsLookup;
    public ReadOnlySpan<DataColumn> Columns => _columns;
    public int Count => _columns.Length == 0 ? 0 : _columns[0].Count;

    public DataRow this[int rowIndex]
    {
        get
        {
            if ((uint)rowIndex >= (uint)(_columns.Length == 0 ? 0 : _columns[0].Count))
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            return new DataRow(this, rowIndex);
        }
    }

    public IEnumerable<DataRow> GetRows()
    {
        if (_columns.Length > 0)
        {
            for (int i = 0, len = _columns[0].Count; i < len; i++)
            {
                yield return new DataRow(this, i);
            }
        }
    }

    protected void ReadData(Stream data)
    {
        _columns = Array.Empty<DataColumn>();
        _columnsLookup.Clear();

        using var reader = new StreamReader(data);

        var colNames = new List<string>();

        int colIndex = 0;
        int rowIndex = 0;
        string? line = null;
        while ((line = reader.ReadLine()) is not null)
        {
            var curLine = line.AsSpan();
            colIndex = 0;

            if (_columns.Length == 0)
            {
                // parse column names from header line
                foreach (var colName in curLine.Tokenize('\t'))
                {
                    colNames.Add(StringPool.Shared.GetOrAdd(colName));
                    _columnsLookup.TryAdd(colNames[colIndex], colIndex);
                    colIndex++;
                }

                _columns = new DataColumn[colIndex];
                continue;
            }

            // add data to existing columns
            foreach (var value in curLine.Tokenize('\t'))
            {
                var col = _columns[colIndex] ??= new StringDataColumn(colNames[colIndex]);

                if (value.IsEmpty)
                {
                    col.AddEmptyValue();
                }
                else if (ushort.TryParse(value, out var ushortVal))
                {
                    switch (col)
                    {
                        case UInt16DataColumn ushortCol:
                            ushortCol.AddValue(ushortVal);
                            break;
                        case Int32DataColumn intCol:
                            intCol.AddValue(ushortVal);
                            break;
                        case StringDataColumn stringCol:
                            {
                                // we need to upgrade this column to a UInt16DataColum (if previous rows were blank)
                                var newCol = new UInt16DataColumn(stringCol.Name);
                                foreach (var v in stringCol.Values)
                                {
                                    if (string.IsNullOrEmpty(v))
                                        newCol.AddEmptyValue();
                                    else
                                        throw new InvalidOperationException($"Trying to add the number {ushortVal} to a string column with previous values (row {rowIndex + 1}, col {colIndex + 1})");
                                }
                                newCol.AddValue(ushortVal);
                                _columns[colIndex] = newCol;
                            }
                            break;
                        default:
                            throw new InvalidOperationException("Unsupported column type.");
                    }
                }
                else if (int.TryParse(value, out var intVal))
                {
                    switch (col)
                    {
                        case Int32DataColumn intCol:
                            intCol.AddValue(intVal);
                            break;
                        case UInt16DataColumn ushortCol:
                            {
                                // we need to upgrade this column to an Int32DataColumn to hold this data
                                var newCol = new Int32DataColumn(ushortCol.Name);
                                foreach (var v in ushortCol.Values)
                                {
                                    newCol.AddValue(v);
                                }
                                newCol.AddValue(intVal);
                                _columns[colIndex] = newCol;
                            }
                            break;
                        case StringDataColumn stringCol:
                            {
                                // we need to upgrade this column to an Int32DataColumn (if previous rows were blank)
                                var newCol = new Int32DataColumn(stringCol.Name);
                                foreach (var v in stringCol.Values)
                                {
                                    if (string.IsNullOrEmpty(v))
                                        newCol.AddEmptyValue();
                                    else
                                        throw new InvalidOperationException($"Trying to add the number {intVal} to a string column with previous values (row {rowIndex + 1}, col {colIndex + 1})");
                                }
                                newCol.AddValue(intVal);
                                _columns[colIndex] = newCol;
                            }
                            break;
                        default:
                            throw new InvalidOperationException("Unsupported column type.");
                    }
                }
                else // string value
                {
                    if (col is StringDataColumn stringCol)
                    {
                        stringCol.AddValue(StringPool.Shared.GetOrAdd(value));
                    }
                    else
                    {
                        if (value.Trim().IsEmpty)
                        {
                            col.AddEmptyValue();
                        }
                        else
                        {
                            throw new InvalidOperationException($"Non-empty string '{value.ToString()}' being added to a {col.GetType().Name} column (row {rowIndex + 1}, col {colIndex + 1})");
                        }
                    }
                }

                colIndex++;
            }

            // if any columns didn't have a row added, add empty values
            while (colIndex < _columns.Length)
            {
                _columns[colIndex++].AddEmptyValue();
            }

            rowIndex++;
        }
    }

    public DataRow? GetByColumnAndValue(string name, ReadOnlySpan<char> value)
    {
        if (int.TryParse(value, out int parsed))
        {
            return GetByColumnAndValue(name, parsed);
        }

        if (ColumnNames.TryGetValue(name, out var colIdx))
        {
            var col = _columns[colIdx];
            value = value.Trim();

            for (int i = 0; i < col.Count; i++)
            {
                if (value.Equals(col.GetString(i).AsSpan().Trim(), StringComparison.Ordinal))
                {
                    return new DataRow(this, i);
                }
            }
        }
        return null;
    }

    public DataRow? GetByColumnAndValue(string name, int value)
    {
        if (ColumnNames.TryGetValue(name, out var colIdx))
        {
            var col = _columns[colIdx];

            for (int i = 0; i < col.Count; i++)
            {
                if (col.GetInt32(i) == value)
                {
                    return new DataRow(this, i);
                }
            }
        }
        return null;
    }
}

[DebuggerDisplay("Row {RowIndex}")]
public sealed class DataRow
{
    private readonly DataFile _data;

    public DataRow(DataFile data, int rowIndex)
    {
        _data = data;
        RowIndex = rowIndex;
    }

    public int RowIndex { get; }

    public DataCell this[int colIndex] => new(_data, colIndex, RowIndex);
    public DataCell this[string colName] => new(_data, _data.ColumnNames[colName], RowIndex);
}

[DebuggerDisplay("Cell (row {RowIndex}, col {ColIndex})")]
public sealed class DataCell
{
    private readonly DataFile _data;

    public DataCell(DataFile data, int colIndex, int rowIndex)
    {
        _data = data;
        ColIndex = colIndex;
        RowIndex = rowIndex;
    }

    public int RowIndex { get; }
    public int ColIndex { get; }

    public string Value => _data.Columns[ColIndex].GetString(RowIndex);
    public int ToInt32() => _data.Columns[ColIndex].GetInt32(RowIndex);
    public ushort ToUInt16() => _data.Columns[ColIndex].GetUInt16(RowIndex);
    public bool ToBool() => _data.Columns[ColIndex].GetBoolean(RowIndex);
}


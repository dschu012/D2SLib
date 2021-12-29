using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace D2SLib.Model.Data;

[DebuggerDisplay("{Name} ({Count} rows)")]
public abstract class DataColumn
{
    protected DataColumn(string name)
    {
        Name = name;
    }

    public string Name { get; }
    public abstract int Count { get; }

    public abstract void AddEmptyValue();
    public abstract string GetString(int rowIndex);
    public abstract int GetInt32(int rowIndex);
    public abstract ushort GetUInt16(int rowIndex);
    public abstract bool GetBoolean(int rowIndex);
    public override string ToString() => Name;
}

public abstract class DataColumn<T> : DataColumn
{
    private readonly List<T> _data;

    public DataColumn(string name) : base(name)
    {
        _data = new();
    }

    public T this[int i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _data[i];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data[i] = value;
    }

    public override sealed int Count => _data.Count;
    public int IndexOf(T value) => _data.IndexOf(value);
    public void AddValue(T value) => _data.Add(value);

    public ReadOnlySpan<T> Values
    {
        get
        {
#if NET6_0_OR_GREATER
            return CollectionsMarshal.AsSpan(_data);
#else
            return _data.ToArray();
#endif
        }
    }
}

public sealed class StringDataColumn : DataColumn<string>
{
    public StringDataColumn(string name) : base(name) { }
    public override int GetInt32(int rowIndex) => 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string GetString(int rowIndex) => this[rowIndex];

    public override ushort GetUInt16(int rowIndex) => 0;
    public override bool GetBoolean(int rowIndex) => false;
    public override void AddEmptyValue() => AddValue(string.Empty);
}

public sealed class Int32DataColumn : DataColumn<int>
{
    public Int32DataColumn(string name) : base(name) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetInt32(int rowIndex) => this[rowIndex];

    public override string GetString(int rowIndex) => this[rowIndex].ToString();
    public override ushort GetUInt16(int rowIndex) => (ushort)this[rowIndex];
    public override bool GetBoolean(int rowIndex) => this[rowIndex] != 0;
    public override void AddEmptyValue() => AddValue(0);
}

public sealed class UInt16DataColumn : DataColumn<ushort>
{
    public UInt16DataColumn(string name) : base(name) { }
    public override int GetInt32(int rowIndex) => this[rowIndex];
    public override string GetString(int rowIndex) => this[rowIndex].ToString();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override ushort GetUInt16(int rowIndex) => this[rowIndex];
    public override bool GetBoolean(int rowIndex) => this[rowIndex] != 0;
    public override void AddEmptyValue() => AddValue(0);
}
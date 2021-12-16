namespace D2SLib.Model.TXT
{
    public abstract class TXTFile
    {
        public Dictionary<string, int> Columns { get; } = new();
        public List<TXTRow> Rows { get; } = new();

        protected void ReadTXTData(Stream data)
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
                Rows.Add(new TXTRow(Columns, reader.ReadLine()?.Split('\t') ?? Array.Empty<string>()));
            }
        }

        public TXTRow? GetByColumnAndValue(string name, ReadOnlySpan<char> value)
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
    public class TXTRow
    {
        public Dictionary<string, int> Columns { get; set; }
        public TXTCell[] Data { get; set; }

        public TXTCell this[int i] => GetByIndex(i);
        public TXTCell this[string i] => GetByColumn(i);

        public TXTRow(Dictionary<string, int> columns, string[] data)
        {
            Columns = columns;
            Data = data.Select(e => new TXTCell(e)).ToArray();
        }

        public TXTCell GetByIndex(int idx) => Data[idx];

        public TXTCell GetByColumn(string col) => GetByIndex(Columns[col]);
    }

    public class TXTCell
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
        public TXTCell(string value)
        {
            Value = value;
        }
    }
}

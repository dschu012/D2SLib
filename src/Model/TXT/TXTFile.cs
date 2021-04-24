using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace D2SLib.Model.TXT
{
    public abstract class TXTFile
    {
        public Dictionary<string, int> Columns { get; set; }
        public List<TXTRow> Rows { get; set; }

        protected void ReadTXTData(Stream data)
        {
            
            Columns = new Dictionary<string, int>();
            Rows = new List<TXTRow>();
            using (StreamReader reader = new StreamReader(data))
            {
                //skip header
                int idx = 0;
                var columns = reader.ReadLine().Split('\t');
                foreach (var col in columns)
                {
                    if (Columns.ContainsKey(col)) continue;
                    Columns.Add(col, idx++);
                }
                while (reader.Peek() >= 0)
                {
                    Rows.Add(new TXTRow(Columns, reader.ReadLine().Split('\t')));
                }
            }
        }

        public TXTRow GetByColumnAndValue(string name, string value)
        {
            Console.WriteLine(name);
            Console.WriteLine(value);
            foreach(var row in Rows)
            {
                if (row[name].Value.Trim() == value.Trim())
                    return row;
            }
            return null;
        }
    }
    public class TXTRow
    {
        public Dictionary<string, int> Columns { get; set; }
        public TXTCell[] Data { get; set; }

        public TXTCell this[int i] => this.GetByIndex(i);
        public TXTCell this[string i] => this.GetByColumn(i);

        public TXTRow(Dictionary<string, int> columns, string[] data)
        {
            Columns = columns;
            Data = data.Select(e => new TXTCell(e)).ToArray(); 
        }

        public TXTCell GetByIndex(int idx)
        {
            return Data[idx];
        }

        public TXTCell GetByColumn(string col)
        {
            return GetByIndex(Columns[col]);
        }
    }

    public class TXTCell
    {
        public string Value { get; set; }

        public Int32 ToInt32()
        {
            Int32 ret = 0;
            Int32.TryParse(Value, out ret);
            return ret;
        }

        public UInt32 ToUInt32()
        {
            UInt32 ret = 0;
            UInt32.TryParse(Value, out ret);
            return ret;
        }

        public UInt16 ToUInt16()
        {
            UInt16 ret = 0;
            UInt16.TryParse(Value, out ret);
            return ret;
        }

        public Int16 ToInt16()
        {
            Int16 ret = 0;
            Int16.TryParse(Value, out ret);
            return ret;
        }

        public bool ToBool()
        {
            return ToInt32() == 1;
        }
        public TXTCell(string value)
        {
            Value = value;
        }
    }
}

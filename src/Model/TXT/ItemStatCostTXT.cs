using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace D2SLib.Model.TXT
{
    public class ItemStatCostTXT : TXTFile
    {
        public TXTRow this[int i] => this.GetByColumnAndValue("ID", i.ToString());
        public TXTRow this[string i] => this.GetByColumnAndValue("Stat", i);

        public static ItemStatCostTXT Read(Stream data)
        {
            ItemStatCostTXT itemStatCostTXT = new ItemStatCostTXT();
            itemStatCostTXT.ReadTXTData(data);
            return itemStatCostTXT;
        }
        public static ItemStatCostTXT Read(string file)
        {
            using (Stream stream = File.OpenRead(file))
            {
                return Read(stream);
            }
        }
    }
}

using D2SLib.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace D2SLib.Model.Save
{
    public class D2I
    {

        public ItemList ItemList { get; set; }

        public static D2I Read(byte[] bytes, UInt32 version)
        {
            D2I d2i = new D2I();
            using (BitReader reader = new BitReader(bytes))
            {
                d2i.ItemList = ItemList.Read(reader, version);
                return d2i;
            }
        }

        public static byte[] Write(D2I d2i, UInt32 version)
        {
            using (BitWriter writer = new BitWriter())
            {
                writer.WriteBytes(ItemList.Write(d2i.ItemList, version));
                return writer.ToArray();
            }
        }

    }
}

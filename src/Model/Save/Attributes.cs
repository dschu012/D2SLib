using D2SLib.IO;
using D2SLib.Model.TXT;
using System;
using System.Collections.Generic;
using System.Text;

namespace D2SLib.Model.Save
{
    //variable size. depends on # of attributes
    public class Attributes
    {
        public UInt16? Header { get; set; }
        public Dictionary<string, Int32> Stats { get; set; } = new Dictionary<string, Int32>();

        public static Attributes Read(BitReader reader)
        {
            ItemStatCostTXT itemStatCost = Core.TXT.ItemStatCostTXT;
            Attributes attributes = new Attributes();
            attributes.Header = reader.ReadUInt16();
            UInt16 id = reader.ReadUInt16(9);
            while(id != 0x1ff)
            {
                var property = itemStatCost[id];
                var attribute = reader.ReadInt32(property["CSvBits"].ToInt32());
                if(property["ValShift"].ToInt32() > 0)
                {
                    attribute >>= property["ValShift"].ToInt32();
                }
                attributes.Stats.Add(property["Stat"].Value, attribute);
                id = reader.ReadUInt16(9);
            }
            reader.Align();
            return attributes;
        }

        public static byte[] Write(Attributes attributes)
        {
            using (BitWriter writer = new BitWriter())
            {
                ItemStatCostTXT itemStatCost = Core.TXT.ItemStatCostTXT;
                writer.WriteUInt16(attributes.Header ?? (UInt16)0x6667);
                foreach (var entry in attributes.Stats)
                {
                    var property = itemStatCost[entry.Key];
                    writer.WriteUInt16(property["ID"].ToUInt16(), 9);
                    Int32 attribute = entry.Value;
                    if(property["ValShift"].ToInt32() > 0)
                    {
                        attribute <<= property["ValShift"].ToInt32();
                    }
                    writer.WriteInt32(attribute, property["CSvBits"].ToInt32());
                }
                writer.WriteUInt16(0x1ff, 9);
                writer.Align();
                return writer.ToArray();
            }
        }

    }
}

using D2SLib.IO;

namespace D2SLib.Model.Save
{
    //variable size. depends on # of attributes
    public class Attributes
    {
        public ushort? Header { get; set; }
        public Dictionary<string, int> Stats { get; set; } = new Dictionary<string, int>();

        public static Attributes Read(BitReader reader)
        {
            var itemStatCost = Core.TXT.ItemStatCostTXT;
            var attributes = new Attributes
            {
                Header = reader.ReadUInt16()
            };
            ushort id = reader.ReadUInt16(9);
            while (id != 0x1ff)
            {
                var property = itemStatCost[id];
                int attribute = reader.ReadInt32(property["CSvBits"].ToInt32());
                if (property["ValShift"].ToInt32() > 0)
                {
                    attribute >>= property["ValShift"].ToInt32(); // let's not parse this string twice
                }
                attributes.Stats.Add(property["Stat"].Value, attribute);
                id = reader.ReadUInt16(9);
            }
            reader.Align();
            return attributes;
        }

        public static byte[] Write(Attributes attributes)
        {
            using (var writer = new BitWriter())
            {
                var itemStatCost = Core.TXT.ItemStatCostTXT;
                writer.WriteUInt16(attributes.Header ?? 0x6667);
                foreach (var entry in attributes.Stats)
                {
                    var property = itemStatCost[entry.Key];
                    writer.WriteUInt16(property["ID"].ToUInt16(), 9);
                    int attribute = entry.Value;
                    if (property["ValShift"].ToInt32() > 0)
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

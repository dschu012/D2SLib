using D2SLib.IO;

namespace D2SLib.Model.Save;

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
            int valShift = property["ValShift"].ToInt32();
            if (valShift > 0)
            {
                attribute >>= valShift;
            }
            attributes.Stats.Add(property["Stat"].Value, attribute);
            id = reader.ReadUInt16(9);
        }
        reader.Align();
        return attributes;
    }

    public void Write(BitWriter writer)
    {
        var itemStatCost = Core.TXT.ItemStatCostTXT;
        writer.WriteUInt16(Header ?? 0x6667);
        foreach (var entry in Stats)
        {
            var property = itemStatCost[entry.Key];
            writer.WriteUInt16(property["ID"].ToUInt16(), 9);
            int attribute = entry.Value;
            int valShift = property["ValShift"].ToInt32();
            if (valShift > 0)
            {
                attribute <<= valShift;
            }
            writer.WriteInt32(attribute, property["CSvBits"].ToInt32());
        }
        writer.WriteUInt16(0x1ff, 9);
        writer.Align();
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(Attributes attributes)
    {
        using var writer = new BitWriter();
        attributes.Write(writer);
        return writer.ToArray();
    }
}

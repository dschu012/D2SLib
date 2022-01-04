using D2SLib.IO;

namespace D2SLib.Model.Save;

public class Golem
{
    private Golem(IBitReader reader, uint version)
    {
        Header = reader.ReadUInt16();
        Exists = reader.ReadByte() == 1;
        if (Exists)
        {
            Item = Item.Read(reader, version);
        }
    }

    public ushort? Header { get; set; }
    public bool Exists { get; set; }
    public Item? Item { get; set; }

    public void Write(IBitWriter writer, uint version)
    {
        writer.WriteUInt16(Header ?? 0x666B);
        writer.WriteByte((byte)(Exists ? 1 : 0));
        if (Exists)
        {
            Item?.Write(writer, version);
        }
    }

    public static Golem Read(IBitReader reader, uint version)
    {
        var golem = new Golem(reader, version);
        return golem;
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(Golem golem, uint version)
    {
        using var writer = new BitWriter();
        golem.Write(writer, version);
        return writer.ToArray();
    }
}

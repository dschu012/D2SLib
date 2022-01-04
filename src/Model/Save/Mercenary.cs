using D2SLib.IO;

namespace D2SLib.Model.Save;

public sealed class Mercenary
{
    //is this right?
    public ushort IsDead { get; set; }
    public uint Id { get; set; }
    public ushort NameId { get; set; }
    public ushort TypeId { get; set; }
    public uint Experience { get; set; }

    public void Write(IBitWriter writer)
    {
        writer.WriteUInt16(IsDead);
        writer.WriteUInt32(Id);
        writer.WriteUInt16(NameId);
        writer.WriteUInt16(TypeId);
        writer.WriteUInt32(Experience);
    }

    public static Mercenary Read(IBitReader reader)
    {
        var mercenary = new Mercenary
        {
            IsDead = reader.ReadUInt16(),
            Id = reader.ReadUInt32(),
            NameId = reader.ReadUInt16(),
            TypeId = reader.ReadUInt16(),
            Experience = reader.ReadUInt32()
        };
        return mercenary;
    }

    [Obsolete("Try the direct-read overload!")]
    public static Mercenary Read(ReadOnlySpan<byte> bytes)
    {
        using var reader = new BitReader(bytes);
        return Read(reader);
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(Mercenary mercenary)
    {
        using var writer = new BitWriter();
        mercenary.Write(writer);
        return writer.ToArray();
    }
}

public sealed class MercenaryItemList : IDisposable
{
    public ushort? Header { get; set; }
    public ItemList? ItemList { get; private set; }

    public void Write(IBitWriter writer, Mercenary mercenary, uint version)
    {
        writer.WriteUInt16(Header ?? 0x666A);
        if (mercenary.Id != 0)
        {
            ItemList?.Write(writer, version);
        }
    }

    public static MercenaryItemList Read(IBitReader reader, Mercenary mercenary, uint version)
    {
        var mercenaryItemList = new MercenaryItemList
        {
            Header = reader.ReadUInt16()
        };
        if (mercenary.Id != 0)
        {
            mercenaryItemList.ItemList = ItemList.Read(reader, version);
        }
        return mercenaryItemList;
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(MercenaryItemList mercenaryItemList, Mercenary mercenary, uint version)
    {
        using var writer = new BitWriter();
        mercenaryItemList.Write(writer, mercenary, version);
        return writer.ToArray();
    }

    public void Dispose() => ItemList?.Dispose();
}

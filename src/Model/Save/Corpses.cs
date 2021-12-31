using D2SLib.IO;

namespace D2SLib.Model.Save;

public sealed class CorpseList : IDisposable
{
    public CorpseList(ushort? header, ushort count)
    {
        Header = header;
        Count = count;
        Corpses = new List<Corpse>(count);
    }

    public ushort? Header { get; set; }
    public ushort Count { get; set; }
    public List<Corpse> Corpses { get; }

    public void Write(IBitWriter writer, uint version)
    {
        writer.WriteUInt16(Header ?? 0x4D4A);
        writer.WriteUInt16(Count);
        for (int i = 0; i < Count; i++)
        {
            Corpses[i].Write(writer, version);
        }
    }

    public static CorpseList Read(IBitReader reader, uint version)
    {
        var corpseList = new CorpseList(
            header: reader.ReadUInt16(),
            count: reader.ReadUInt16()
        );
        for (int i = 0; i < corpseList.Count; i++)
        {
            corpseList.Corpses.Add(Corpse.Read(reader, version));
        }
        return corpseList;
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(CorpseList corpseList, uint version)
    {
        using var writer = new BitWriter();
        corpseList.Write(writer, version);
        return writer.ToArray();
    }

    public void Dispose()
    {
        foreach (var corpse in Corpses)
        {
            corpse?.Dispose();
        }
        Corpses.Clear();
    }
}

public sealed class Corpse : IDisposable
{
    private Corpse(IBitReader reader, uint version)
    {
        Unk0x0 = reader.ReadUInt32();
        X = reader.ReadUInt32();
        Y = reader.ReadUInt32();
        ItemList = ItemList.Read(reader, version);
    }

    public uint? Unk0x0 { get; set; }
    public uint X { get; set; }
    public uint Y { get; set; }
    public ItemList ItemList { get; }

    public void Write(IBitWriter writer, uint version)
    {
        writer.WriteUInt32(Unk0x0 ?? 0x0);
        writer.WriteUInt32(X);
        writer.WriteUInt32(Y);
        ItemList.Write(writer, version);
    }

    public static Corpse Read(IBitReader reader, uint version)
    {
        var corpse = new Corpse(reader, version);
        return corpse;
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(Corpse corpse, uint version)
    {
        using var writer = new BitWriter();
        corpse.Write(writer, version);
        return writer.ToArray();
    }

    public void Dispose() => ItemList.Dispose();
}


using D2SLib.IO;

namespace D2SLib.Model.Save;

public sealed class D2I : IDisposable
{
    private D2I(IBitReader reader, uint version)
    {
        ItemList = ItemList.Read(reader, version);
    }

    public ItemList ItemList { get; }

    public void Write(IBitWriter writer, uint version)
    {
        ItemList.Write(writer, version);
    }

    public static D2I Read(IBitReader reader, uint version) => new(reader, version);

    public static D2I Read(ReadOnlySpan<byte> bytes, uint version)
    {
        using var reader = new BitReader(bytes);
        return new D2I(reader, version);
    }

    public static byte[] Write(D2I d2i, uint version)
    {
        using var writer = new BitWriter();
        d2i.Write(writer, version);
        return writer.ToArray();
    }

    public void Dispose() => ItemList?.Dispose();
}

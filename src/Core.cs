using D2SLib.IO;
using D2SLib.Model.Save;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace D2SLib;

public class Core
{
    private static MetaData? _metaData = null;
    public static MetaData MetaData
    {
        get => _metaData ?? ResourceFilesData.Instance.MetaData;
        set => _metaData = value;
    }

    public static D2S ReadD2S(string path) => D2S.Read(File.ReadAllBytes(path));

    public static async Task<D2S> ReadD2SAsync(string path, CancellationToken ct = default)
    {
        var bytes = await File.ReadAllBytesAsync(path, ct).ConfigureAwait(false);
        return D2S.Read(bytes);
    }

    public static D2S ReadD2S(ReadOnlySpan<byte> bytes) => D2S.Read(bytes);

    public static Item ReadItem(string path, uint version) => ReadItem(File.ReadAllBytes(path), version);

    public static async Task<Item> ReadItemAsync(string path, uint version, CancellationToken ct = default)
    {
        var bytes = await File.ReadAllBytesAsync(path, ct).ConfigureAwait(false);
        return Item.Read(bytes, version);
    }

    public static Item ReadItem(ReadOnlySpan<byte> bytes, uint version) => Item.Read(bytes, version);

    public static D2I ReadD2I(string path, uint version) => D2I.Read(File.ReadAllBytes(path), version);

    public static async Task<D2I> ReadD2IAsync(string path, uint version, CancellationToken ct = default)
    {
        var bytes = await File.ReadAllBytesAsync(path, ct).ConfigureAwait(false);
        return D2I.Read(bytes, version);
    }

    public static D2I ReadD2I(ReadOnlySpan<byte> bytes, uint version) => D2I.Read(bytes, version);

    public static MemoryOwner<byte> WriteD2SPooled(D2S d2s) => D2S.WritePooled(d2s);

    public static byte[] WriteD2S(D2S d2s) => D2S.Write(d2s);

    public static MemoryOwner<byte> WriteItemPooled(Item item, uint version)
    {
        using var writer = new BitWriter();
        item.Write(writer, version);
        return writer.ToPooledArray();
    }

    public static byte[] WriteItem(Item item, uint version) => Item.Write(item, version);

    public static MemoryOwner<byte> WriteD2IPooled(D2I d2i, uint version)
    {
        using var writer = new BitWriter();
        d2i.Write(writer, version);
        return writer.ToPooledArray();
    }

    public static byte[] WriteD2I(D2I d2i, uint version) => D2I.Write(d2i, version);

}

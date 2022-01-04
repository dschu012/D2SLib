using D2SLib.IO;
using System.Buffers.Binary;

namespace D2SLib.Model.Save;

public class Header
{
    //0x0000
    public uint? Magic { get; set; }
    //0x0004
    public uint Version { get; set; }
    //0x0008
    public uint Filesize { get; set; }
    //0x000c
    public uint Checksum { get; set; }

    public void Write(IBitWriter writer)
    {
        writer.WriteUInt32(Magic ?? 0xAA55AA55);
        writer.WriteUInt32(Version);
        writer.WriteUInt32(Filesize);
        writer.WriteUInt32(Checksum);
    }

    public static Header Read(IBitReader reader)
    {
        var header = new Header
        {
            Magic = reader.ReadUInt32(),
            Version = reader.ReadUInt32(),
            Filesize = reader.ReadUInt32(),
            Checksum = reader.ReadUInt32()
        };
        return header;
    }

    [Obsolete("Try the direct-read overload!")]
    public static Header Read(ReadOnlySpan<byte> bytes)
    {
        using var reader = new BitReader(bytes);
        return Read(reader);
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(Header header)
    {
        using var writer = new BitWriter();
        header.Write(writer);
        return writer.ToArray();
    }

    public static void Fix(Span<byte> bytes)
    {
        FixSize(bytes);
        FixChecksum(bytes);
    }

    public static void FixSize(Span<byte> bytes)
    {
        Span<byte> length = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32LittleEndian(length, (uint)bytes.Length);
        length.CopyTo(bytes[0x8..]);
    }

    public static void FixChecksum(Span<byte> bytes)
    {
        bytes[0xc..].Clear();
        int checksum = 0;
        for (int i = 0; i < bytes.Length; i++)
        {
            checksum = bytes[i] + (checksum * 2) + (checksum < 0 ? 1 : 0);
        }
        Span<byte> csb = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(csb, checksum);
        csb.CopyTo(bytes[0xc..]);
    }
}

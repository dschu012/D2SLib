using System.Runtime.CompilerServices;
using System.Text;

namespace D2SLib.IO;

public sealed class BitReader : IBitReader, IDisposable
{
    private const int STACK_MAX = 100;
    private BitField _bits;

    public int Position { get; private set; }

    public BitReader(ReadOnlySpan<byte> bytes)
    {
        Position = 0;
        _bits = new BitField(bytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetBytesForBits(int numberOfBits) => (numberOfBits - 1) / 8 + 1;

    public bool ReadBit() => _bits[Position++];

    public byte[] ReadBits(int numberOfBits)
    {
        byte[] bytes = new byte[GetBytesForBits(numberOfBits)];
        ReadBits(numberOfBits, bytes);
        return bytes;
    }

    public int ReadBits(int numberOfBits, Span<byte> output)
    {
        int byteCount = _bits.GetBytes(Position, numberOfBits, output);
        Position += numberOfBits;
        return byteCount;
    }

    public byte[] ReadBytes(int numberOfBytes) => ReadBits(numberOfBytes * 8);

    public int ReadBytes(int numberOfBytes, Span<byte> output) 
        => ReadBits(numberOfBytes * 8, output);

    public byte ReadByte(int bits)
    {
        int byteCount = GetBytesForBits(bits);
        Span<byte> bytes = byteCount > STACK_MAX ? new byte[byteCount] : stackalloc byte[byteCount];
        int bytesRead = ReadBits(bits, bytes);
        return bytes[0];
    }

    public byte ReadByte() => ReadByte(8);

    public ushort ReadUInt16(int bits)
    {
        var result = _bits.GetUInt16(Position, bits);
        Position += bits;
        return result;
    }

    public ushort ReadUInt16() => ReadUInt16(sizeof(ushort) * 8);

    public uint ReadUInt32(int bits)
    {
        var result = _bits.GetUInt32(Position, bits);
        Position += bits;
        return result;
    }

    public uint ReadUInt32() => ReadUInt32(sizeof(uint) * 8);

    public int ReadInt32(int bits)
    {
        var result = _bits.GetInt32(Position, bits);
        Position += bits;
        return result;
    }

    public int ReadInt32() => ReadInt32(sizeof(int) * 8);

    public string ReadString(int byteCount)
    {
        Span<byte> bytes = byteCount > STACK_MAX ? new byte[byteCount] : stackalloc byte[byteCount];
        int readBytes = ReadBytes(byteCount, bytes);
        bytes = bytes[..readBytes];
        return Encoding.ASCII.GetString(bytes.Trim((byte)0));
    }

    public void SeekBits(int bitPosition) => Position = bitPosition;

    public void Seek(int bytePostion) => SeekBits(bytePostion * 8);

    public void Align() => Position = (Position + 7) & ~7;

    public void Dispose()
    {
        Interlocked.Exchange(ref _bits!, null)?.Dispose();
    }
}

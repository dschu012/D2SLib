using Microsoft.Toolkit.HighPerformance.Buffers;
using System.Buffers;
using System.Text;
using static System.Buffers.Binary.BinaryPrimitives;
using static D2SLib.IO.InternalBitArray;

namespace D2SLib.IO;

public sealed class BitReader : IBitReader, IDisposable
{
    private const int STACK_MAX = 0xff;

    private InternalBitArray _bits;
    public int Position { get; private set; }

    public BitReader(ReadOnlySpan<byte> bytes)
    {
        Position = 0;
        _bits = new InternalBitArray(bytes);
    }

    public bool ReadBit() => _bits[Position++];

    public byte[] ReadBits(int numberOfBits)
    {
        byte[] bytes = new byte[GetByteArrayLengthFromBitLength(numberOfBits)];
        ReadBits(numberOfBits, bytes);
        return bytes;
    }

    public MemoryOwner<byte> ReadBitsPooled(int numberOfBits)
    {
        var bytes = MemoryOwner<byte>.Allocate(GetByteArrayLengthFromBitLength(numberOfBits));
        ReadBits(numberOfBits, bytes.Span);
        return bytes;
    }

    IMemoryOwner<byte> IBitReader.ReadBitsPooled(int numberOfBits)
        => ReadBitsPooled(numberOfBits);

    public int ReadBits(int numberOfBits, Span<byte> output)
    {
        int byteCount = GetByteArrayLengthFromBitLength(numberOfBits);

        if (output.Length < byteCount)
            throw new ArgumentOutOfRangeException(nameof(output));

        int byteIndex = 0;
        int bitIndex = 0;
        for (int i = 0; i < numberOfBits; i++)
        {
            if (_bits[Position + i])
            {
                output[byteIndex] |= (byte)(1 << bitIndex);
            }
            bitIndex++;
            if (bitIndex == 8)
            {
                byteIndex++;
                bitIndex = 0;
            }
        }
        Position += numberOfBits;

        return byteCount;
    }


    public int ReadBytes(int numberOfBytes, Span<byte> output) 
        => ReadBits(numberOfBytes * 8, output);

    public int ReadBytes(Span<byte> output)
        => ReadBits(output.Length * 8, output);

    public byte[] ReadBytes(int numberOfBytes) 
        => ReadBits(numberOfBytes * 8);

    public MemoryOwner<byte> ReadBytesPooled(int numberOfBytes)
        => ReadBitsPooled(numberOfBytes * 8);

    IMemoryOwner<byte> IBitReader.ReadBytesPooled(int numberOfBytes)
        => ReadBitsPooled(numberOfBytes * 8);

    public byte ReadByte(int bits)
    {
        if ((uint)bits > 8) throw new ArgumentOutOfRangeException(nameof(bits));
        Span<byte> bytes = stackalloc byte[1];
        bytes.Clear();
        int bytesRead = ReadBits(bits, bytes);
        return bytes[0];
    }

    public byte ReadByte() => ReadByte(8);

    public ushort ReadUInt16(int bits)
    {
        if ((uint)bits > sizeof(ushort) * 8) throw new ArgumentOutOfRangeException(nameof(bits));
        Span<byte> bytes = stackalloc byte[sizeof(ushort)];
        bytes.Clear();
        ReadBits(bits, bytes);
        return ReadUInt16LittleEndian(bytes);
    }

    public ushort ReadUInt16() => ReadUInt16(sizeof(ushort) * 8);

    public uint ReadUInt32(int bits)
    {
        if ((uint)bits > sizeof(uint) * 8) throw new ArgumentOutOfRangeException(nameof(bits));
        Span<byte> bytes = stackalloc byte[sizeof(uint)];
        bytes.Clear();
        ReadBits(bits, bytes);
        return ReadUInt32LittleEndian(bytes);
    }

    public uint ReadUInt32() => ReadUInt32(sizeof(uint) * 8);

    public int ReadInt32(int bits)
    {
        if ((uint)bits > sizeof(int) * 8) throw new ArgumentOutOfRangeException(nameof(bits));
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        bytes.Clear();
        ReadBits(bits, bytes);
        return ReadInt32LittleEndian(bytes);
    }

    public int ReadInt32() => ReadInt32(sizeof(int) * 8);

    public string ReadString(int byteCount)
    {
        using var pooledBytes = byteCount > STACK_MAX ? SpanOwner<byte>.Allocate(byteCount) : SpanOwner<byte>.Empty;
        Span<byte> bytes = byteCount > STACK_MAX ? pooledBytes.Span : stackalloc byte[byteCount];
        bytes.Clear();
        int readBytes = ReadBytes(bytes);
        bytes = bytes[..readBytes];
        return Encoding.ASCII.GetString(bytes.Trim((byte)0));
    }

    public void AdvanceBits(int bits) => Position += bits;
    public void SeekBits(int bitPosition) => Position = bitPosition;
    public void Seek(int bytePostion) => SeekBits(bytePostion * 8);

    public void Align() => Position = (Position + 7) & ~7;

    public void Dispose() => Interlocked.Exchange(ref _bits!, null)?.Dispose();
}

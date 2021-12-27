using Microsoft.Toolkit.HighPerformance.Buffers;
using System.Buffers;
using System.Runtime.CompilerServices;
using static D2SLib.IO.InternalBitArray;

namespace D2SLib.IO;

public sealed class BitWriter : IBitWriter, IDisposable
{
    private InternalBitArray _bits;

    private int _position = 0;
    public int Position
    {
        get => _position;
        private set
        {
            if (value > Length)
            {
                Length = value;
            }
            _position = value;
        }
    }

    public int Length { get; private set; }

    public BitWriter(int initialCapacity)
    {
        _bits = new InternalBitArray(initialCapacity);
        Position = 0;
    }

    public BitWriter() : this(1024)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBit(bool value)
    {
        if (_position >= _bits.Length)
        {
            Grow();
        }
        _bits[Position++] = value;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow()
    {
        while (_position >= _bits.Length)
        {
            if (_bits.Length == 0)
            {
                _bits.Length = 1024;
            }
            else
            {
                _bits.Length *= 2;
            }
        }
    }

    internal void WriteBits(InternalBitArray bits) => WriteBits(bits, bits.Length);

    internal void WriteBits(InternalBitArray bits, int numberOfBits)
    {
        for (int i = 0; i < numberOfBits; i++)
        {
            WriteBit(bits[i]);
        }
    }

    public void WriteBytes(ReadOnlySpan<byte> value)
    {
        var bits = new InternalBitArray(value);
        WriteBits(bits);
    }

    public void WriteBytes(ReadOnlySpan<byte> value, int numberOfBits)
    {
        var bits = new InternalBitArray(value) { Length = numberOfBits };
        WriteBits(bits, numberOfBits);
    }

    public void WriteByte(byte value, int size) => WriteBytes(stackalloc byte[] { value }, size);

    public void WriteByte(byte value) => WriteBytes(stackalloc byte[] { value }, 8);

    public void WriteUInt16(ushort value, int numberOfBits) => WriteBytes(BitConverter.GetBytes(value), numberOfBits);

    public void WriteUInt16(ushort value) => WriteBytes(BitConverter.GetBytes(value), sizeof(ushort) * 8);
    public void WriteUInt32(uint value, int numberOfBits) => WriteBytes(BitConverter.GetBytes(value), numberOfBits);
    public void WriteUInt32(uint value) => WriteBytes(BitConverter.GetBytes(value), sizeof(uint) * 8);
    public void WriteInt32(int value, int numberOfBits) => WriteBytes(BitConverter.GetBytes(value), numberOfBits);
    public void WriteInt32(int value) => WriteBytes(BitConverter.GetBytes(value), sizeof(int) * 8);
    public void WriteString(string s, int length) => WriteBytes(System.Text.Encoding.ASCII.GetBytes(s), length * 8);
    
    [Obsolete("Try the ToPooledArray or GetBytes methods for lower allocation.")]
    public byte[] ToArray()
    {
        byte[] bytes = new byte[GetByteArrayLengthFromBitLength(Length)];
        InternalGetBytes(bytes);
        return bytes;
    }

    public MemoryOwner<byte> ToPooledArray()
    {
        var bytes = MemoryOwner<byte>.Allocate(GetByteArrayLengthFromBitLength(Length));
        InternalGetBytes(bytes.Span);
        return bytes;
    }

    IMemoryOwner<byte> IBitWriter.ToPooledArray() => ToPooledArray();

    public int GetBytes(Span<byte> output)
    {
        int byteLength = GetByteArrayLengthFromBitLength(Length);

        if (byteLength > output.Length)
            throw new ArgumentOutOfRangeException(nameof(output));

        InternalGetBytes(output);

        return byteLength;
    }

    // assumes calling method has sized output correctly
    private void InternalGetBytes(Span<byte> output)
    {
        int byteIndex = 0;
        int bitIndex = 0;
        for (int i = 0; i < Length; ++i)
        {
            if (_bits[i])
            {
                output[byteIndex] |= (byte)(1 << bitIndex);
            }
            ++bitIndex;
            if (bitIndex >= 8)
            {
                ++byteIndex;
                bitIndex = 0;
            }
        }
    }

    public void SkipBits(int numberOfBits) => Position += numberOfBits;
    public void Skip(int bytes) => SkipBits(bytes * 8);
    public void SeekBits(int bitPosition) => Position = bitPosition;
    public void Seek(int bytePostion) => SeekBits(bytePostion * 8);
    public void Align() => Position = (Position + 7) & ~7;
    public void Dispose() => _bits = null!;
}

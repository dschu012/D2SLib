using Microsoft.Toolkit.HighPerformance.Buffers;
using System.Runtime.CompilerServices;
using static System.Buffers.Binary.BinaryPrimitives;

namespace D2SLib.IO;

// Based on code from daveaglick, https://stackoverflow.com/a/6769591/24380
// TODO: Make endian-ness configurable

public sealed class BitField : IDisposable
{
    private MemoryOwner<byte> _bytes;

    public BitField(int size)
    {
        _bytes = MemoryOwner<byte>.Allocate(size);
    }

    //fill == true = initially set all bits to 1
    public BitField(int size, bool fill) : this(size)
    {
        if (!fill)
        {
            return;
        }

        _bytes.Span.Fill(0xff);
    }

    public BitField(ReadOnlySpan<byte> bytes) : this(bytes.Length)
    {
        bytes.CopyTo(_bytes.Span);
    }

    public Span<byte> Bytes => _bytes.Span;

    public bool this[int bit]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => IsBitSet(bit);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (value)
                SetBit(bit);
            else
                ClearBit(bit);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBitSet(int bit) 
        => (_bytes.Span[bit / 8] & (1 << (7 - (bit % 8)))) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBit(int bit) 
        => _bytes.Span[bit / 8] |= unchecked((byte)(1 << (7 - (bit % 8))));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearBit(int bit) 
        => _bytes.Span[bit / 8] &= unchecked((byte)~(1 << (7 - (bit % 8))));

    /// <summary>
    /// Write a given number of bits to a byte span.
    /// </summary>
    /// <param name="index">The index of the source BitField at which to start getting bits.</param>
    /// <param name="length">The number of bits to get.</param>
    /// <param name="bytesToFill">The span to fill with bytes.</param>
    /// <param name="fill">True to set all padding bits to 1.</param>
    /// <returns>The number of bytes written to.</returns>
    public int GetBytes(int index, int length, Span<byte> bytesToFill, bool fill = false)
    {
        int size = Math.Min(bytesToFill.Length, (length + 7) / 8);
        using var bitField = new BitField(size, fill);
        for (int s = index, d = (size * 8) - length; s < index + length && d < (size * 8); s++, d++)
        {
            bitField[d] = IsBitSet(s);
        }
        bitField._bytes.Span[..size].CopyTo(bytesToFill);
        return size;
    }

    /// <summary>
    /// Set bits from the given <see cref="bytes"/> span starting at the given index.
    /// </summary>
    /// <param name="bytes">The bytes to set.</param>
    /// <param name="bytesIndex">The index (in bits) into the <see cref="bytes"/> span at which to start copying.</param>
    /// <param name="index">The index (in bits) in this BitField at which to put the value.</param>
    /// <param name="length">The number of bits to copy from the bytes array.</param>
    public void SetBytes(ReadOnlySpan<byte> bytes, int bytesIndex, int index, int length)
    {
        using var bitField = new BitField(bytes);
        for (int i = 0; i < length; i++)
        {
            this[index + i] = bitField[bytesIndex + i];
        }
    }

    /// <summary>
    /// Set bits from the given <see cref="bytes"/> span starting at the given index.
    /// </summary>
    /// <param name="bytes">The bytes to set.</param>
    /// <param name="index">The index (in bits) in this BitField at which to put the value.</param>
    /// <param name="length">The number of bits to copy from the bytes array.</param>
    public void SetBytes(ReadOnlySpan<byte> bytes, int index, int length)
        => SetBytes(bytes, 0, index, length);

    /// <summary>
    /// Set bits from the given <see cref="bytes"/> span starting at the given index.
    /// </summary>
    /// <param name="bytes">The bytes to set.</param>
    /// <param name="index">The index (in bits) in this BitField at which to put the value.</param>
    public void SetBytes(ReadOnlySpan<byte> bytes, int index)
        => SetBytes(bytes, 0, index, bytes.Length * 8);

    //UInt16

    /// <summary>
    /// Reads a <see cref="ushort"/> from the BitField.
    /// </summary>
    /// <param name="index">The index (in bits) at which to start getting the value.</param>
    /// <param name="length">The number of bits to use for the value, if less than required the value is padded with 0.</param>
    public ushort GetUInt16(int index, int length)
    {
        Span<byte> bytes = stackalloc byte[sizeof(ushort)];
        GetBytes(index, length, bytes);
        return ReadUInt16LittleEndian(bytes);
    }

    /// <summary>
    /// Reads a <see cref="ushort"/> from the BitField.
    /// </summary>
    /// <param name="index">The index (in bits) at which to start getting the value.</param>
    public ushort GetUInt16(int index)
        => GetUInt16(index, sizeof(ushort) * 8);

    /// <summary>
    /// Sets a <see cref="ushort"/> into the BitField.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="valueIndex">The index (in bits) of the value at which to start copying.</param>
    /// <param name="index">The index (in bits) in this BitField at which to put the value.</param>
    /// <param name="length">The number of bits to copy from the value.</param>
    public void Set(ushort value, int valueIndex, int index, int length)
    {
        Span<byte> bytes = stackalloc byte[sizeof(ushort)];
        WriteUInt16LittleEndian(bytes, value);
        SetBytes(bytes, valueIndex, index, length);
    }

    /// <summary>
    /// Sets a <see cref="ushort"/> into the BitField.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="index">The index (in bits) in this BitField at which to put the value.</param>
    public void Set(ushort value, int index)
        => Set(value, 0, index, sizeof(ushort) * 8);

    //UInt32

    /// <summary>
    /// Reads a <see cref="uint"/> from the BitField.
    /// </summary>
    /// <param name="index">The index (in bits) at which to start getting the value.</param>
    /// <param name="length">The number of bits to use for the value, if less than required the value is padded with 0.</param>
    public uint GetUInt32(int index, int length)
    {
        Span<byte> bytes = stackalloc byte[sizeof(uint)];
        GetBytes(index, length, bytes);
        return ReadUInt32LittleEndian(bytes);
    }

    /// <summary>
    /// Reads a <see cref="uint"/> from the BitField.
    /// </summary>
    /// <param name="index">The index (in bits) at which to start getting the value.</param>
    public uint GetUInt32(int index)
        => GetUInt32(index, sizeof(uint) * 8);

    /// <summary>
    /// Sets a <see cref="uint"/> into the BitField.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="valueIndex">The index (in bits) of the value at which to start copying.</param>
    /// <param name="index">The index (in bits) in this BitField at which to put the value.</param>
    /// <param name="length">The number of bits to copy from the value.</param>
    public void Set(uint value, int valueIndex, int index, int length)
    {
        Span<byte> bytes = stackalloc byte[sizeof(uint)];
        WriteUInt32LittleEndian(bytes, value);
        SetBytes(bytes, valueIndex, index, length);
    }

    /// <summary>
    /// Sets a <see cref="uint"/> into the BitField.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="index">The index (in bits) in this BitField at which to put the value.</param>
    public void Set(uint value, int index)
        => Set(value, 0, index, sizeof(uint) * 8);

    //UInt64

    /// <summary>
    /// Reads a <see cref="ulong"/> from the BitField.
    /// </summary>
    /// <param name="index">The index (in bits) at which to start getting the value.</param>
    /// <param name="length">The number of bits to use for the value, if less than required the value is padded with 0.</param>
    public ulong GetUInt64(int index, int length)
    {
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        GetBytes(index, length, bytes);
        return ReadUInt64LittleEndian(bytes);
    }

    /// <summary>
    /// Reads a <see cref="ulong"/> from the BitField.
    /// </summary>
    /// <param name="index">The index (in bits) at which to start getting the value.</param>
    public ulong GetUInt64(int index)
        => GetUInt64(index, sizeof(ulong) * 8);

    /// <summary>
    /// Sets a <see cref="ulong"/> into the BitField.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="valueIndex">The index (in bits) of the value at which to start copying.</param>
    /// <param name="index">The index (in bits) in this BitField at which to put the value.</param>
    /// <param name="length">The number of bits to copy from the value.</param>
    public void Set(ulong value, int valueIndex, int index, int length)
    {
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        WriteUInt64LittleEndian(bytes, value);
        SetBytes(bytes, valueIndex, index, length);
    }

    /// <summary>
    /// Sets a <see cref="ulong"/> into the BitField.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="valueIndex">The index (in bits) of the value at which to start copying.</param>
    /// <param name="index">The index (in bits) in this BitField at which to put the value.</param>
    public void Set(ulong value, int index)
        => Set(value, 0, index, sizeof(ulong) * 8);

    //Int16

    /// <summary>
    /// Reads a <see cref="short"/> from the BitField.
    /// </summary>
    /// <param name="index">The index (in bits) at which to start getting the value.</param>
    /// <param name="length">The number of bits to use for the value, if less than required the value is padded with 0.</param>
    public short GetInt16(int index, int length)
    {
        Span<byte> bytes = stackalloc byte[sizeof(short)];
        GetBytes(index, length, bytes, IsBitSet(index));
        return ReadInt16LittleEndian(bytes);
    }

    /// <summary>
    /// Reads a <see cref="short"/> from the BitField.
    /// </summary>
    /// <param name="index">The index (in bits) at which to start getting the value.</param>
    public short GetInt16(int index)
        => GetInt16(index, sizeof(short) * 8);

    /// <summary>
    /// Sets a <see cref="short"/> into the BitField.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="valueIndex">The index (in bits) of the value at which to start copying.</param>
    /// <param name="index">The index (in bits) in this BitField at which to put the value.</param>
    /// <param name="length">The number of bits to copy from the value.</param>
    public void Set(short value, int valueIndex, int index, int length)
    {
        Span<byte> bytes = stackalloc byte[sizeof(short)];
        WriteInt16LittleEndian(bytes, value);
        SetBytes(bytes, valueIndex, index, length);
    }

    /// <summary>
    /// Sets a <see cref="short"/> into the BitField.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="index">The index (in bits) in this BitField at which to put the value.</param>
    public void Set(short value, int index)
        => Set(value, 0, index, sizeof(short) * 8);

    //Int32

    /// <summary>
    /// Reads an <see cref="int"/> from the BitField.
    /// </summary>
    /// <param name="index">The index (in bits) at which to start getting the value.</param>
    /// <param name="length">The number of bits to use for the value, if less than required the value is padded with 0.</param>
    public int GetInt32(int index, int length)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        GetBytes(index, length, bytes, IsBitSet(index));
        return ReadInt32LittleEndian(bytes);
    }

    /// <summary>
    /// Reads an <see cref="int"/> from the BitField.
    /// </summary>
    /// <param name="index">The index (in bits) at which to start getting the value.</param>
    public int GetInt32(int index)
        => GetInt32(index, sizeof(int) * 8);

    /// <summary>
    /// Sets an <see cref="int"/> into the BitField.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="valueIndex">The index (in bits) of the value at which to start copying.</param>
    /// <param name="index">The index (in bits) in this BitField at which to put the value.</param>
    /// <param name="length">The number of bits to copy from the value.</param>
    public void Set(int value, int valueIndex, int index, int length)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        WriteInt32LittleEndian(bytes, value);
        SetBytes(bytes, valueIndex, index, length);
    }

    /// <summary>
    /// Sets an <see cref="int"/> into the BitField.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="index">The index (in bits) in this BitField at which to put the value.</param>
    public void Set(int value, int index)
        => Set(value, 0, index, sizeof(int) * 8);

    //Int64

    /// <summary>
    /// Reads a <see cref="long"/> from the BitField.
    /// </summary>
    /// <param name="index">The index (in bits) at which to start getting the value.</param>
    /// <param name="length">The number of bits to use for the value, if less than required the value is padded with 0.</param>
    public long GetInt64(int index, int length)
    {
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        GetBytes(index, length, bytes, IsBitSet(index));
        return ReadInt64LittleEndian(bytes);
    }

    /// <summary>
    /// Reads a <see cref="long"/> from the BitField.
    /// </summary>
    /// <param name="index">The index (in bits) at which to start getting the value.</param>
    public long GetInt64(int index)
        => GetInt64(index, sizeof(long) * 8);

    /// <summary>
    /// Sets a <see cref="long"/> into the BitField.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="valueIndex">The index (in bits) of the value at which to start copying.</param>
    /// <param name="index">The index (in bits) in this BitField at which to put the value.</param>
    /// <param name="length">The number of bits to copy from the value.</param>
    public void Set(long value, int valueIndex, int index, int length)
    {
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        WriteInt64LittleEndian(bytes, value);
        SetBytes(bytes, valueIndex, index, length);
    }

    /// <summary>
    /// Sets a <see cref="long"/> into the BitField.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="index">The index (in bits) in this BitField at which to put the value.</param>
    public void Set(long value, int index)
        => Set(value, 0, index, sizeof(long) * 8);

    //Char

    /// <summary>
    /// Reads a <see cref="char"/> from the BitField.
    /// </summary>
    /// <param name="index">The index (in bits) at which to start getting the value.</param>
    /// <param name="length">The number of bits to use for the value, if less than required the value is padded with 0.</param>
    public char GetChar(int index, int length)
    {
        Span<byte> bytes = stackalloc byte[sizeof(char)];
        GetBytes(index, length, bytes);
        return (char)ReadUInt16LittleEndian(bytes);
    }

    /// <summary>
    /// Reads a <see cref="char"/> from the BitField.
    /// </summary>
    /// <param name="index">The index (in bits) at which to start getting the value.</param>
    public char GetChar(int index)
        => GetChar(index, sizeof(char) * 8);

    /// <summary>
    /// Sets a <see cref="char"/> into the BitField.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="valueIndex">The index (in bits) of the value at which to start copying.</param>
    /// <param name="index">The index (in bits) in this BitField at which to put the value.</param>
    /// <param name="length">The number of bits to copy from the value.</param>
    public void Set(char value, int valueIndex, int index, int length)
    {
        Span<byte> bytes = stackalloc byte[sizeof(char)];
        WriteUInt16LittleEndian(bytes, value);
        SetBytes(bytes, valueIndex, index, length);
    }

    /// <summary>
    /// Sets a <see cref="char"/> into the BitField.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="index">The index (in bits) in this BitField at which to put the value.</param>
    public void Set(char value, int index)
        => Set(value, 0, index, sizeof(char) * 8);

    //Bool

    /// <summary>
    /// Reads a <see cref="bool"/> from the BitField.
    /// </summary>
    /// <param name="index">The index (in bits) at which to start getting the value.</param>
    /// <param name="length">The number of bits to use for the value, if less than required the value is padded with 0.</param>
    public bool GetBool(int index, int length)
    {
        Span<byte> bytes = stackalloc byte[sizeof(bool)];
        GetBytes(index, length, bytes);
        return Convert.ToBoolean(bytes[0]);
    }

    /// <summary>
    /// Reads a <see cref="bool"/> from the BitField.
    /// </summary>
    /// <param name="index">The index (in bits) at which to start getting the value.</param>
    public bool GetBool(int index)
        => GetBool(index, sizeof(bool) * 8);

    /// <summary>
    /// Sets a <see cref="char"/> into the BitField.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="index">The index (in bits) in this BitField at which to put the value.</param>
    public void Set(bool value, int valueIndex, int index, int length)
    {
        Span<byte> bytes = stackalloc byte[sizeof(bool)];
        bytes[0] = Convert.ToByte(value);
        SetBytes(bytes, valueIndex, index, length);
    }


    /// <summary>
    /// Sets a <see cref="char"/> into the BitField.
    /// </summary>
    /// <param name="value">The value to set.</param>
    public void Set(bool value, int index)
        => Set(value, 0, index, sizeof(bool) * 8);

    //Single and double precision floating point values must always use the correct number of bits

#if NET6_0

    public float GetSingle(int index)
    {
        Span<byte> bytes = stackalloc byte[sizeof(float)];
        GetBytes(index, sizeof(float) * 8, bytes);
        return ReadSingleLittleEndian(bytes);
    }

    public void SetSingle(float value, int index)
    {
        Span<byte> bytes = stackalloc byte[sizeof(float)];
        WriteSingleLittleEndian(bytes, value);
        SetBytes(bytes, 0, index, sizeof(float) * 8);
    }

    public double GetDouble(int index)
    {
        Span<byte> bytes = stackalloc byte[sizeof(double)];
        GetBytes(index, sizeof(double) * 8, bytes);
        return ReadDoubleLittleEndian(bytes);
    }

    public void SetDouble(double value, int index)
    {
        Span<byte> bytes = stackalloc byte[sizeof(double)];
        WriteDoubleLittleEndian(bytes, value);
        SetBytes(bytes, 0, index, sizeof(double) * 8);
    }

#endif

    public void Dispose()
    {
        Interlocked.Exchange(ref _bytes!, null)?.Dispose();
    }
}

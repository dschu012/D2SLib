using System.Collections;

namespace D2SLibTests;

public class BitWriter_Old : IDisposable
{
    private BitArray _bits;

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
    public BitWriter_Old(int initialCapacity)
    {
        _bits = new BitArray(initialCapacity);
        Position = 0;
    }

    public BitWriter_Old() : this(1024)
    {
    }

    public void WriteBit(bool value)
    {
        // grow if necessary
        while (Position >= _bits.Length)
        {
            if (_bits.Length == 0)
            {
                _bits.Length = 1024;
            }
            else
            {
                _bits.Length = _bits.Length * 2;
            }
        }
        _bits[Position++] = value;
    }

    public void WriteBits(BitArray bits, int numberOfBits)
    {
        for (int i = 0; i < numberOfBits; i++)
        {
            WriteBit(bits[i]);
        }
    }
    public void WriteBytes(byte[] value, int numberOfBits)
    {
        Array.Resize<byte>(ref value, (numberOfBits - 1) / 8 + 1);
        var bits = new BitArray(value);
        WriteBits(bits, numberOfBits);
    }
    public void WriteBytes(byte[] value)
    {
        var bits = new BitArray(value);
        WriteBits(bits, value.Length * 8);
    }

    public void WriteByte(byte value, int size) => WriteBytes(new byte[] { value }, size);

    public void WriteByte(byte value) => WriteBytes(new byte[] { value }, 8);

    public void WriteUInt16(ushort value, int numberOfBits) => WriteBytes(BitConverter.GetBytes(value), numberOfBits);

    public void WriteUInt16(ushort value) => WriteBytes(BitConverter.GetBytes(value), 16);
    public void WriteUInt32(uint value, int numberOfBits) => WriteBytes(BitConverter.GetBytes(value), numberOfBits);
    public void WriteUInt32(uint value) => WriteBytes(BitConverter.GetBytes(value), 32);
    public void WriteInt32(int value, int numberOfBits) => WriteBytes(BitConverter.GetBytes(value), numberOfBits);
    public void WriteInt32(int value) => WriteBytes(BitConverter.GetBytes(value), 32);
    public void WriteString(string s, int length) => WriteBytes(System.Text.Encoding.ASCII.GetBytes(s), length * 8);
    public byte[] ToArray()
    {
        byte[] bytes = new byte[((Length - 1) / 8) + 1];
        int byteIndex = 0;
        int bitIndex = 0;
        for (int i = 0; i < Length; ++i)
        {
            if (_bits[i])
            {
                bytes[byteIndex] |= (byte)(1 << bitIndex);
            }
            ++bitIndex;
            if (bitIndex >= 8)
            {
                ++byteIndex;
                bitIndex = 0;
            }
        }
        return bytes;
    }
    public void SkipBits(int numberOfBits) => Position += numberOfBits;
    public void Skip(int bytes) => SkipBits(bytes * 8);
    public void SeekBits(int bitPosition) => Position = bitPosition;
    public void Seek(int bytePostion) => SeekBits(bytePostion * 8);
    public void Align() => Position = (Position + 7) & ~7;
    public void Dispose() => _bits = null!;
}

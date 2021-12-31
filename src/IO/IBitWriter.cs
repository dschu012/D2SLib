using System.Buffers;

namespace D2SLib.IO;

public interface IBitWriter
{
    int Length { get; }
    int Position { get; }

    void Align();
    void Dispose();
    void Seek(int bytePostion);
    void SeekBits(int bitPosition);
    void Skip(int bytes);
    void SkipBits(int numberOfBits);
    byte[] ToArray();
    IMemoryOwner<byte> ToPooledArray();
    int GetBytes(Span<byte> output);
    void WriteBit(bool value);
    void WriteBits(IList<bool> bits);
    void WriteBits(IList<bool> bits, int numberOfBits);
    void WriteByte(byte value);
    void WriteByte(byte value, int size);
    void WriteBytes(ReadOnlySpan<byte> value);
    void WriteBytes(ReadOnlySpan<byte> value, int numberOfBits);
    void WriteInt32(int value);
    void WriteInt32(int value, int numberOfBits);
    void WriteString(ReadOnlySpan<char> s, int length);
    void WriteUInt16(ushort value);
    void WriteUInt16(ushort value, int numberOfBits);
    void WriteUInt32(uint value);
    void WriteUInt32(uint value, int numberOfBits);
}
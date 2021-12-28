using System.Buffers;

namespace D2SLib.IO;

public interface IBitReader
{
    int Position { get; }

    void Align();
    bool ReadBit();
    byte[] ReadBits(int numberOfBits);
    IMemoryOwner<byte> ReadBitsPooled(int numberOfBits);
    int ReadBits(int numberOfBits, Span<byte> output);
    byte ReadByte();
    byte ReadByte(int bits);
    byte[] ReadBytes(int numberOfBytes);
    IMemoryOwner<byte> ReadBytesPooled(int numberOfBytes);
    int ReadBytes(int numberOfBytes, Span<byte> output);
    int ReadBytes(Span<byte> output);
    int ReadInt32();
    int ReadInt32(int bits);
    string ReadString(int byteCount);
    ushort ReadUInt16();
    ushort ReadUInt16(int bits);
    uint ReadUInt32();
    uint ReadUInt32(int bits);
    void Seek(int bytePostion);
    void SeekBits(int bitPosition);
    void AdvanceBits(int bits);
}
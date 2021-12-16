using D2SLib.IO;
using System.Text;
using static System.Buffers.Binary.BinaryPrimitives;

namespace D2SLibTests;

[TestClass]
public sealed class BitReaderTests
{
    [TestMethod]
    public void CanReadBits()
    {
        byte[] bytes = new byte[25];
        new Random(1337).NextBytes(bytes);
        using var bro = new BitReader_Old(bytes);
        using var br = new BitReader(bytes);

        Assert.IsTrue(bro.ReadBits(73).AsSpan().SequenceEqual(br.ReadBits(73)));
    }

    [TestMethod]
    public void CanReadByte()
    {
        byte[] bytes = new byte[] { 137 };
        using var bro = new BitReader_Old(bytes);
        using var br = new BitReader(bytes);

        Assert.AreEqual(bro.ReadByte(), br.ReadByte());
    }

    [TestMethod]
    public void CanReadBytes()
    {
        byte[] bytes = new byte[10];
        new Random(1337).NextBytes(bytes);
        using var bro = new BitReader_Old(bytes);
        using var br = new BitReader(bytes);

        Assert.IsTrue(bro.ReadBytes(10).AsSpan().SequenceEqual(br.ReadBytes(10)));
    }

    [TestMethod]
    public void CanReadInt32()
    {
        byte[] bytes = new byte[sizeof(int)];
        WriteInt32LittleEndian(bytes, 1370048);
        using var bro = new BitReader_Old(bytes);
        using var br = new BitReader(bytes);

        Assert.AreEqual(bro.ReadInt32(), br.ReadInt32());
    }

    [TestMethod]
    public void CanReadUInt32()
    {
        byte[] bytes = new byte[sizeof(uint)];
        WriteUInt32LittleEndian(bytes, 1370048);
        using var bro = new BitReader_Old(bytes);
        using var br = new BitReader(bytes);

        Assert.AreEqual(bro.ReadUInt32(), br.ReadUInt32());
    }

    [TestMethod]
    public void CanReadUInt16()
    {
        byte[] bytes = new byte[sizeof(ushort)];
        WriteUInt16LittleEndian(bytes, 7401);
        using var bro = new BitReader_Old(bytes);
        using var br = new BitReader(bytes);

        Assert.AreEqual(bro.ReadUInt16(), br.ReadUInt16());
    }

    [TestMethod]
    public void CanReadString()
    {
        byte[] bytes = Encoding.ASCII.GetBytes("test");
        using var bro = new BitReader_Old(bytes);
        using var br = new BitReader(bytes);

        Assert.AreEqual(bro.ReadString(4), br.ReadString(4));
    }
}
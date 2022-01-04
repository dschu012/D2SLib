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
        byte[] bytes = new byte[32];
        new Random(1337).NextBytes(bytes);
        using var bro = new BitReader_Old(bytes);
        using var br = new BitReader(bytes);

        var oldBits = bro.ReadBits(17);
        var newBits = br.ReadBits(17);

        for (int i = 0; i < oldBits.Length; i++)
        {
            Console.Write(Convert.ToString(oldBits[i], 2).PadLeft(8, '0'));
            Console.Write(' ');
        }
        Console.WriteLine();

        for (int i = 0; i < newBits.Length; i++)
        {
            Console.Write(Convert.ToString(newBits[i], 2).PadLeft(8, '0'));
            Console.Write(' ');
        }
        Console.WriteLine();

        CollectionAssert.AreEqual(oldBits, newBits);
        Assert.AreEqual(bro.Position, br.Position);
    }

    [TestMethod]
    public void CanReadByte()
    {
        byte[] bytes = new byte[] { 137 };
        using var bro = new BitReader_Old(bytes);
        using var br = new BitReader(bytes);

        Assert.AreEqual(bro.ReadByte(), br.ReadByte());
        Assert.AreEqual(bro.Position, br.Position);
    }

    [TestMethod]
    public void CanReadBytes()
    {
        byte[] bytes = new byte[97];
        new Random(1337).NextBytes(bytes);
        using var bro = new BitReader_Old(bytes);
        using var br = new BitReader(bytes);

        bro.SeekBits(5);
        br.SeekBits(5);

        var oldBits = bro.ReadBytes(95);
        var newBits = br.ReadBytes(95);

        CollectionAssert.AreEqual(oldBits, newBits);
        Assert.AreEqual(bro.Position, br.Position);
    }

    [TestMethod]
    public void CanReadInt32()
    {
        byte[] bytes = new byte[sizeof(int)];
        WriteInt32LittleEndian(bytes, 1370048);
        using var bro = new BitReader_Old(bytes);
        using var br = new BitReader(bytes);

        Assert.AreEqual(bro.ReadInt32(), br.ReadInt32());
        Assert.AreEqual(bro.Position, br.Position);
    }

    [TestMethod]
    public void CanReadUInt32()
    {
        byte[] bytes = new byte[sizeof(uint)];
        WriteUInt32LittleEndian(bytes, 1370048);
        using var bro = new BitReader_Old(bytes);
        using var br = new BitReader(bytes);

        Assert.AreEqual(bro.ReadUInt32(), br.ReadUInt32());
        Assert.AreEqual(bro.Position, br.Position);
    }

    [TestMethod]
    public void CanReadUInt16()
    {
        byte[] bytes = new byte[sizeof(ushort)];
        WriteUInt16LittleEndian(bytes, 7401);
        using var bro = new BitReader_Old(bytes);
        using var br = new BitReader(bytes);

        Assert.AreEqual(bro.ReadUInt16(), br.ReadUInt16());
        Assert.AreEqual(bro.Position, br.Position);
    }

    [TestMethod]
    public void CanReadString()
    {
        byte[] bytes = Encoding.ASCII.GetBytes("test");
        using var bro = new BitReader_Old(bytes);
        using var br = new BitReader(bytes);

        Assert.AreEqual(bro.ReadString(4), br.ReadString(4));
        Assert.AreEqual(bro.Position, br.Position);
    }
}
using D2SLib.IO;
using System.Diagnostics.CodeAnalysis;

namespace D2SLib.Model.Save;

public class Appearances
{
    private readonly Appearance[] _parts = new Appearance[16];

    public Appearance Head { get => _parts[0]; set => _parts[0] = value; }
    public Appearance Torso { get => _parts[1]; set => _parts[1] = value; }
    public Appearance Legs { get => _parts[2]; set => _parts[2] = value; }
    public Appearance RightArm { get => _parts[3]; set => _parts[3] = value; }
    public Appearance LeftArm { get => _parts[4]; set => _parts[4] = value; }
    public Appearance RightHand { get => _parts[5]; set => _parts[5] = value; }
    public Appearance LeftHand { get => _parts[6]; set => _parts[6] = value; }
    public Appearance Shield { get => _parts[7]; set => _parts[7] = value; }
    public Appearance Special1 { get => _parts[8]; set => _parts[8] = value; }
    public Appearance Special2 { get => _parts[9]; set => _parts[9] = value; }
    public Appearance Special3 { get => _parts[10]; set => _parts[10] = value; }
    public Appearance Special4 { get => _parts[11]; set => _parts[11] = value; }
    public Appearance Special5 { get => _parts[12]; set => _parts[12] = value; }
    public Appearance Special6 { get => _parts[13]; set => _parts[13] = value; }
    public Appearance Special7 { get => _parts[14]; set => _parts[14] = value; }
    public Appearance Special8 { get => _parts[15]; set => _parts[15] = value; }

    public void Write(IBitWriter writer)
    {
        for (int i = 0; i < _parts.Length; i++)
        {
            _parts[i].Write(writer);
        }
    }

    public static Appearances Read(IBitReader reader)
    {
        var appearances = new Appearances();
        var parts = appearances._parts;

        for (int i = 0; i < parts.Length; i++)
        {
            parts[i] = new Appearance(reader);
        }

        return appearances;
    }

    [Obsolete("Try the direct-read overload!")]
    public static Appearances Read(ReadOnlySpan<byte> bytes)
    {
        using var reader = new BitReader(bytes);
        return Read(reader);
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(Appearances appearances)
    {
        using var writer = new BitWriter();
        appearances.Write(writer);
        return writer.ToArray();
    }

}

public readonly struct Appearance : IEquatable<Appearance>
{
    public Appearance(byte graphic, byte tint)
    {
        Graphic = graphic;
        Tint = tint;
    }

    public Appearance(IBitReader reader)
    {
        Graphic = reader.ReadByte();
        Tint = reader.ReadByte();
    }

    public readonly byte Graphic { get; }
    public readonly byte Tint { get; }

    public void Write(IBitWriter writer)
    {
        writer.WriteByte(Graphic);
        writer.WriteByte(Tint);
    }

    public bool Equals([AllowNull] Appearance other)
    {
        return Graphic == other.Graphic
            && Tint == other.Tint;
    }

    public override bool Equals(object? obj) => obj is Appearance app && Equals(app);

    public override int GetHashCode() => HashCode.Combine(Graphic, Tint);

    public static bool operator ==(Appearance left, Appearance right) => left.Equals(right);

    public static bool operator !=(Appearance left, Appearance right) => !left.Equals(right);
}

using D2SLib.IO;
using System.Diagnostics.CodeAnalysis;

namespace D2SLib.Model.Save;

public class Locations
{
    private readonly Location[] _locations = new Location[3];

    public Location Normal { get => _locations[0]; set => _locations[0] = value; }
    public Location Nightmare { get => _locations[1]; set => _locations[1] = value; }
    public Location Hell { get => _locations[2]; set => _locations[2] = value; }

    public void Write(IBitWriter writer)
    {
        for (int i = 0; i < _locations.Length; i++)
        {
            _locations[i].Write(writer);
        }
    }

    public static Locations Read(IBitReader reader)
    {
        var locations = new Locations();
        var places = locations._locations;
        for (int i = 0; i < places.Length; i++)
        {
            places[i] = Location.Read(reader);
        }
        return locations;
    }

    [Obsolete("Try the direct-read overload!")]
    public static Locations Read(ReadOnlySpan<byte> bytes)
    {
        using var reader = new BitReader(bytes);
        return Read(reader);
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(Locations locations)
    {
        using var writer = new BitWriter();
        locations.Write(writer);
        return writer.ToArray();
    }
}

public readonly struct Location : IEquatable<Location>
{
    public Location(bool active, byte act)
    {
        Active = active;
        Act = act;
    }

    public readonly bool Active { get; }
    public readonly byte Act { get; }

    public void Write(IBitWriter writer)
    {
        byte b = 0x0;
        if (Active)
        {
            b |= 0x7;
        }

        b |= (byte)(Act - 1);

        writer.WriteByte(b);
    }

    public static Location Read(IBitReader reader)
    {
        byte b = reader.ReadByte();
        return new Location(
            active: (b >> 7) == 1,
            act: (byte)((b & 0x5) + 1)
        );
    }

    public bool Equals(Location other)
    {
        return Active == other.Active
            && Act == other.Act;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Location other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Active, Act);

    public static bool operator ==(Location left, Location right) => left.Equals(right);

    public static bool operator !=(Location left, Location right) => !left.Equals(right);
}

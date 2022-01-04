using D2SLib.IO;

namespace D2SLib.Model.Save;

public sealed class WaypointsSection : IDisposable
{
    private readonly WaypointsDifficulty[] _difficulties = new WaypointsDifficulty[3];

    //0x0279 [waypoint data = 0x57, 0x53 "WS"]
    public ushort? Header { get; set; }
    //0x027b [waypoint header version = 0x1, 0x0, 0x0, 0x0]
    public uint? Version { get; set; }
    //0x027f [waypoint header length = 0x50, 0x0]
    public ushort? Length { get; set; }
    public WaypointsDifficulty Normal => _difficulties[0];
    public WaypointsDifficulty Nightmare => _difficulties[1];
    public WaypointsDifficulty Hell => _difficulties[2];

    public void Write(IBitWriter writer)
    {
        writer.WriteUInt16(Header ?? 0x5357);
        writer.WriteUInt32(Version ?? 0x1);
        writer.WriteUInt16(Length ?? 0x50);

        for (int i = 0; i < _difficulties.Length; i++)
        {
            _difficulties[i].Write(writer);
        }
    }

    public static WaypointsSection Read(IBitReader reader)
    {
        var waypointsSection = new WaypointsSection
        {
            Header = reader.ReadUInt16(),
            Version = reader.ReadUInt32(),
            Length = reader.ReadUInt16()
        };

        for (int i = 0; i < waypointsSection._difficulties.Length; i++)
        {
            waypointsSection._difficulties[i] = WaypointsDifficulty.Read(reader);
        }

        return waypointsSection;
    }

    [Obsolete("Try the direct-read overload!")]
    public static WaypointsSection Read(ReadOnlySpan<byte> bytes)
    {
        using var reader = new BitReader(bytes);
        return Read(reader);
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(WaypointsSection waypointsSection)
    {
        using var writer = new BitWriter();
        waypointsSection.Write(writer);
        return writer.ToArray();
    }

    public void Dispose()
    {
        for (int i = 0; i < _difficulties.Length; i++)
        {
            Interlocked.Exchange(ref _difficulties[i]!, null)?.Dispose();
        }
    }
}

public sealed class WaypointsDifficulty : IDisposable
{
    private WaypointsDifficulty(IBitReader reader)
    {
        Header = reader.ReadUInt16();
        ActI = ActIWaypoints.Read(reader);
        ActII = ActIIWaypoints.Read(reader);
        ActIII = ActIIIWaypoints.Read(reader);
        ActIV = ActIVWaypoints.Read(reader);
        ActV = ActVWaypoints.Read(reader);

        reader.Align();
        reader.AdvanceBits(17 * 8);
    }

    //[0x02, 0x01]
    public ushort? Header { get; set; }
    public ActIWaypoints ActI { get; set; }
    public ActIIWaypoints ActII { get; set; }
    public ActIIIWaypoints ActIII { get; set; }
    public ActIVWaypoints ActIV { get; set; }
    public ActVWaypoints ActV { get; set; }

    public void Write(IBitWriter writer)
    {
        writer.WriteUInt16(Header ?? 0x102);

        int startPos = writer.Position;
        ActI.Write(writer);
        ActII.Write(writer);
        ActIII.Write(writer);
        ActIV.Write(writer);
        ActV.Write(writer);
        int endPos = writer.Position;

        writer.Align();
        Span<byte> padding = stackalloc byte[13];
        padding.Clear();
        writer.WriteBytes(padding);
    }

    public static WaypointsDifficulty Read(IBitReader reader)
    {
        var waypointsDifficulty = new WaypointsDifficulty(reader);
        return waypointsDifficulty;
    }

    [Obsolete("Try the direct-read overload!")]
    public static WaypointsDifficulty Read(ReadOnlySpan<byte> bytes)
    {
        using var reader = new BitReader(bytes);
        return Read(reader);
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(WaypointsDifficulty waypointsDifficulty)
    {
        using var writer = new BitWriter();
        waypointsDifficulty.Write(writer);
        return writer.ToArray();
    }

    public void Dispose()
    {
        ActI.Dispose();
        ActII.Dispose();
        ActIII.Dispose();
        ActIV.Dispose();
        ActV.Dispose();
    }
}

public sealed class ActIWaypoints : IDisposable
{
    private InternalBitArray _flags;
    private ActIWaypoints(InternalBitArray flags) => _flags = flags;

    public bool RogueEncampement { get => _flags[0]; set => _flags[0] = value; }
    public bool ColdPlains { get => _flags[1]; set => _flags[1] = value; }
    public bool StonyField { get => _flags[2]; set => _flags[2] = value; }
    public bool DarkWoods { get => _flags[3]; set => _flags[3] = value; }
    public bool BlackMarsh { get => _flags[4]; set => _flags[4] = value; }
    public bool OuterCloister { get => _flags[5]; set => _flags[5] = value; }
    public bool JailLvl1 { get => _flags[6]; set => _flags[6] = value; }
    public bool InnerCloister { get => _flags[7]; set => _flags[7] = value; }
    public bool CatacombsLvl2 { get => _flags[8]; set => _flags[8] = value; }

    public void Write(IBitWriter writer)
    {
        foreach (var flag in _flags)
        {
            writer.WriteBit(flag);
        }
    }

    public static ActIWaypoints Read(IBitReader reader)
    {
        Span<byte> bytes = stackalloc byte[2];
        reader.ReadBits(9, bytes);
        var bits = new InternalBitArray(bytes);
        return new ActIWaypoints(bits);
    }

    public void Dispose() => Interlocked.Exchange(ref _flags!, null)?.Dispose();
}

public sealed class ActIIWaypoints : IDisposable
{
    private InternalBitArray _flags;
    private ActIIWaypoints(InternalBitArray flags) => _flags = flags;

    public bool LutGholein { get => _flags[0]; set => _flags[0] = value; }
    public bool SewersLvl2 { get => _flags[1]; set => _flags[1] = value; }
    public bool DryHills { get => _flags[2]; set => _flags[2] = value; }
    public bool HallsOfTheDeadLvl2 { get => _flags[3]; set => _flags[3] = value; }
    public bool FarOasis { get => _flags[4]; set => _flags[4] = value; }
    public bool LostCity { get => _flags[5]; set => _flags[5] = value; }
    public bool PalaceCellarLvl1 { get => _flags[6]; set => _flags[6] = value; }
    public bool ArcaneSanctuary { get => _flags[7]; set => _flags[7] = value; }
    public bool CanyonOfTheMagi { get => _flags[8]; set => _flags[8] = value; }

    public void Write(IBitWriter writer)
    {
        foreach (var flag in _flags)
        {
            writer.WriteBit(flag);
        }
    }

    public static ActIIWaypoints Read(IBitReader reader)
    {
        Span<byte> bytes = stackalloc byte[2];
        reader.ReadBits(9, bytes);
        var bits = new InternalBitArray(bytes);
        return new ActIIWaypoints(bits);
    }

    public void Dispose() => Interlocked.Exchange(ref _flags!, null)?.Dispose();
}

public sealed class ActIIIWaypoints : IDisposable
{
    private InternalBitArray _flags;
    private ActIIIWaypoints(InternalBitArray flags) => _flags = flags;

    public bool KurastDocks { get => _flags[0]; set => _flags[0] = value; }
    public bool SpiderForest { get => _flags[1]; set => _flags[1] = value; }
    public bool GreatMarsh { get => _flags[2]; set => _flags[2] = value; }
    public bool FlayerJungle { get => _flags[3]; set => _flags[3] = value; }
    public bool LowerKurast { get => _flags[4]; set => _flags[4] = value; }
    public bool KurastBazaar { get => _flags[5]; set => _flags[5] = value; }
    public bool UpperKurast { get => _flags[6]; set => _flags[6] = value; }
    public bool Travincal { get => _flags[7]; set => _flags[7] = value; }
    public bool DuranceOfHateLvl2 { get => _flags[8]; set => _flags[8] = value; }

    public void Write(IBitWriter writer)
    {
        foreach (var flag in _flags)
        {
            writer.WriteBit(flag);
        }
    }

    public static ActIIIWaypoints Read(IBitReader reader)
    {
        Span<byte> bytes = stackalloc byte[2];
        reader.ReadBits(9, bytes);
        var bits = new InternalBitArray(bytes);
        return new ActIIIWaypoints(bits);
    }

    public void Dispose() => Interlocked.Exchange(ref _flags!, null)?.Dispose();
}

public sealed class ActIVWaypoints : IDisposable
{
    private InternalBitArray _flags;
    private ActIVWaypoints(InternalBitArray flags) => _flags = flags;

    public bool ThePandemoniumFortress { get => _flags[0]; set => _flags[0] = value; }
    public bool CityOfTheDamned { get => _flags[1]; set => _flags[1] = value; }
    public bool RiverOfFlame { get => _flags[2]; set => _flags[2] = value; }

    public void Write(IBitWriter writer)
    {
        foreach (var flag in _flags)
        {
            writer.WriteBit(flag);
        }
    }

    public static ActIVWaypoints Read(IBitReader reader)
    {
        Span<byte> bytes = stackalloc byte[1];
        reader.ReadBits(3, bytes);
        var bits = new InternalBitArray(bytes);
        return new ActIVWaypoints(bits);
    }

    public void Dispose() => Interlocked.Exchange(ref _flags!, null)?.Dispose();
}

public sealed class ActVWaypoints : IDisposable
{
    private InternalBitArray _flags;
    private ActVWaypoints(InternalBitArray flags) => _flags = flags;

    public bool Harrogath { get => _flags[0]; set => _flags[0] = value; }
    public bool FrigidHighlands { get => _flags[1]; set => _flags[1] = value; }
    public bool ArreatPlateau { get => _flags[2]; set => _flags[2] = value; }
    public bool CrystallinePassage { get => _flags[3]; set => _flags[3] = value; }
    public bool HallsOfPain { get => _flags[4]; set => _flags[4] = value; }
    public bool GlacialTrail { get => _flags[5]; set => _flags[5] = value; }
    public bool FrozenTundra { get => _flags[6]; set => _flags[6] = value; }
    public bool TheAncientsWay { get => _flags[7]; set => _flags[7] = value; }
    public bool WorldstoneKeepLvl2 { get => _flags[8]; set => _flags[8] = value; }

    public void Write(IBitWriter writer)
    {
        foreach (var flag in _flags)
        {
            writer.WriteBit(flag);
        }
    }

    public static ActVWaypoints Read(IBitReader reader)
    {
        Span<byte> bytes = stackalloc byte[2];
        reader.ReadBits(9, bytes);
        var bits = new InternalBitArray(bytes);
        return new ActVWaypoints(bits);
    }

    public void Dispose() => Interlocked.Exchange(ref _flags!, null)?.Dispose();
}

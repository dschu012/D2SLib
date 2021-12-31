using D2SLib.IO;
using System.Diagnostics;

namespace D2SLib.Model.Save;

public sealed class QuestsSection : IDisposable
{
    private readonly QuestsDifficulty[] _difficulties = new QuestsDifficulty[3];

    //0x014b [unk = 0x1, 0x0, 0x0, 0x0]
    public uint? Magic { get; set; }
    //0x014f [quests header identifier = 0x57, 0x6f, 0x6f, 0x21 "Woo!"]
    public uint? Header { get; set; }
    //0x0153 [version = 0x6, 0x0, 0x0, 0x0]
    public uint? Version { get; set; }
    //0x0153 [quests header length = 0x2a, 0x1]
    public ushort? Length { get; set; }

    public QuestsDifficulty Normal => _difficulties[0];
    public QuestsDifficulty Nightmare => _difficulties[1];
    public QuestsDifficulty Hell => _difficulties[2];

    public void Write(IBitWriter writer)
    {
        writer.WriteUInt32(Magic ?? 0x1);
        writer.WriteUInt32(Header ?? 0x216F6F57);
        writer.WriteUInt32(Version ?? 0x6);
        writer.WriteUInt16(Length ?? 0x12A);

        for (int i = 0; i < _difficulties.Length; i++)
        {
            _difficulties[i].Write(writer);
        }
    }

    public static QuestsSection Read(IBitReader reader)
    {
        var questSection = new QuestsSection
        {
            Magic = reader.ReadUInt32(),
            Header = reader.ReadUInt32(),
            Version = reader.ReadUInt32(),
            Length = reader.ReadUInt16()
        };

        for (int i = 0; i < questSection._difficulties.Length; i++)
        {
            questSection._difficulties[i] = QuestsDifficulty.Read(reader);
        }

        return questSection;
    }

    [Obsolete("Try the direct-read overload!")]
    public static QuestsSection Read(byte[] bytes)
    {
        using var reader = new BitReader(bytes);
        return Read(reader);
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(QuestsSection questSection)
    {
        using var writer = new BitWriter();
        questSection.Write(writer);
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

public sealed class QuestsDifficulty : IDisposable
{
    private QuestsDifficulty(IBitReader reader)
    {
        ActI = ActIQuests.Read(reader);
        ActII = ActIIQuests.Read(reader);
        ActIII = ActIIIQuests.Read(reader);
        ActIV = ActIVQuests.Read(reader);
        ActV = ActVQuests.Read(reader);
    }

    public ActIQuests ActI { get; set; }
    public ActIIQuests ActII { get; set; }
    public ActIIIQuests ActIII { get; set; }
    public ActIVQuests ActIV { get; set; }
    public ActVQuests ActV { get; set; }

    public void Write(IBitWriter writer)
    {
        ActI.Write(writer);
        ActII.Write(writer);
        ActIII.Write(writer);
        ActIV.Write(writer);
        ActV.Write(writer);
    }

    public static QuestsDifficulty Read(IBitReader reader)
    {
        var qd = new QuestsDifficulty(reader);
        return qd;
    }

    [Obsolete("Try the direct-read overload!")]
    public static QuestsDifficulty Read(ReadOnlySpan<byte> bytes)
    {
        using var reader = new BitReader(bytes);
        return Read(reader);
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(QuestsDifficulty questsDifficulty)
    {
        using var writer = new BitWriter();
        questsDifficulty.Write(writer);
        Debug.Assert(writer.Position == 96 * 8);
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


public sealed class Quest : IDisposable
{
    private InternalBitArray _flags;

    private Quest(InternalBitArray flags) => _flags = flags;

    public bool RewardGranted { get => _flags[0]; set => _flags[0] = value; }
    public bool RewardPending { get => _flags[1]; set => _flags[1] = value; }
    public bool Started { get => _flags[2]; set => _flags[2] = value; }
    public bool LeftTown { get => _flags[3]; set => _flags[3] = value; }
    public bool EnterArea { get => _flags[4]; set => _flags[4] = value; }
    public bool Custom1 { get => _flags[5]; set => _flags[5] = value; }
    public bool Custom2 { get => _flags[6]; set => _flags[6] = value; }
    public bool Custom3 { get => _flags[7]; set => _flags[7] = value; }
    public bool Custom4 { get => _flags[8]; set => _flags[8] = value; }
    public bool Custom5 { get => _flags[9]; set => _flags[9] = value; }
    public bool Custom6 { get => _flags[10]; set => _flags[10] = value; }
    public bool Custom7 { get => _flags[11]; set => _flags[11] = value; }
    public bool QuestLog { get => _flags[12]; set => _flags[12] = value; }
    public bool PrimaryGoalAchieved { get => _flags[13]; set => _flags[13] = value; }
    public bool CompletedNow { get => _flags[14]; set => _flags[14] = value; }
    public bool CompletedBefore { get => _flags[15]; set => _flags[15] = value; }

    public void Write(IBitWriter writer)
    {
        ushort flags = 0x0;
        ushort i = 1;
        foreach (var flag in _flags)
        {
            if (flag)
            {
                flags |= i;
            }
            i <<= 1;
        }
        writer.WriteUInt16(flags);
    }

    public static Quest Read(IBitReader reader)
    {
        Span<byte> bytes = stackalloc byte[2];
        reader.ReadBytes(bytes);
        var bits = new InternalBitArray(bytes);
        return new Quest(bits);
    }

    [Obsolete("Try the direct-read overload!")]
    public static Quest Read(ReadOnlySpan<byte> bytes)
    {
        var bits = new InternalBitArray(bytes);
        return new Quest(bits);
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(Quest quest)
    {
        using var writer = new BitWriter();
        quest.Write(writer);
        return writer.ToArray();
    }

    public void Dispose() => Interlocked.Exchange(ref _flags!, null)?.Dispose();
}

public sealed class ActIQuests : IDisposable
{
    private readonly Quest[] _quests = new Quest[8];

    public Quest Introduction => _quests[0];
    public Quest DenOfEvil => _quests[1];
    public Quest SistersBurialGrounds => _quests[2];
    public Quest ToolsOfTheTrade => _quests[3];
    public Quest TheSearchForCain => _quests[4];
    public Quest TheForgottenTower => _quests[5];
    public Quest SistersToTheSlaughter => _quests[6];
    public Quest Completion => _quests[7];

    public void Write(IBitWriter writer)
    {
        for (int i = 0; i < _quests.Length; i++)
        {
            _quests[i].Write(writer);
        }
    }

    public static ActIQuests Read(IBitReader reader)
    {
        var quests = new ActIQuests();
        for (int i = 0; i < quests._quests.Length; i++)
        {
            quests._quests[i] = Quest.Read(reader);
        }
        return quests;
    }

    public void Dispose()
    {
        for (int i = 0; i < _quests.Length; i++)
        {
            Interlocked.Exchange(ref _quests[i]!, null)?.Dispose();
        }
    }
}

public sealed class ActIIQuests : IDisposable
{
    private readonly Quest[] _quests = new Quest[8];

    public Quest Introduction => _quests[0];
    public Quest RadamentsLair => _quests[1];
    public Quest TheHoradricStaff => _quests[2];
    public Quest TaintedSun => _quests[3];
    public Quest ArcaneSanctuary => _quests[4];
    public Quest TheSummoner => _quests[5];
    public Quest TheSevenTombs => _quests[6];
    public Quest Completion => _quests[7];

    public void Write(IBitWriter writer)
    {
        for (int i = 0; i < _quests.Length; i++)
        {
            _quests[i].Write(writer);
        }
    }

    public static ActIIQuests Read(IBitReader reader)
    {
        var quests = new ActIIQuests();
        for (int i = 0; i < quests._quests.Length; i++)
        {
            quests._quests[i] = Quest.Read(reader);
        }
        return quests;
    }

    public void Dispose()
    {
        for (int i = 0; i < _quests.Length; i++)
        {
            Interlocked.Exchange(ref _quests[i]!, null)?.Dispose();
        }
    }
}

public sealed class ActIIIQuests : IDisposable
{
    private readonly Quest[] _quests = new Quest[8];

    public Quest Introduction => _quests[0];
    public Quest LamEsensTome => _quests[1];
    public Quest KhalimsWill => _quests[2];
    public Quest BladeOfTheOldReligion => _quests[3];
    public Quest TheGoldenBird => _quests[4];
    public Quest TheBlackenedTemple => _quests[5];
    public Quest TheGuardian => _quests[6];
    public Quest Completion => _quests[7];

    public void Write(IBitWriter writer)
    {
        for (int i = 0; i < _quests.Length; i++)
        {
            _quests[i].Write(writer);
        }
    }

    public static ActIIIQuests Read(IBitReader reader)
    {
        var quests = new ActIIIQuests();
        for (int i = 0; i < quests._quests.Length; i++)
        {
            quests._quests[i] = Quest.Read(reader);
        }
        return quests;
    }

    public void Dispose()
    {
        for (int i = 0; i < _quests.Length; i++)
        {
            Interlocked.Exchange(ref _quests[i]!, null)?.Dispose();
        }
    }
}

public sealed class ActIVQuests : IDisposable
{
    private readonly Quest[] _quests = new Quest[8];

    public Quest Introduction => _quests[0];
    public Quest TheFallenAngel => _quests[1];
    public Quest TerrorsEnd => _quests[2];
    public Quest Hellforge => _quests[3];
    public Quest Completion => _quests[4];

    //3 shorts at the end of ActIV completion. presumably for extra quests never used.
    public Quest Extra1 => _quests[5];
    public Quest Extra2 => _quests[6];
    public Quest Extra3 => _quests[7];

    public void Write(IBitWriter writer)
    {
        for (int i = 0; i < _quests.Length; i++)
        {
            _quests[i].Write(writer);
        }
    }

    public static ActIVQuests Read(IBitReader reader)
    {
        var quests = new ActIVQuests();
        for (int i = 0; i < quests._quests.Length; i++)
        {
            quests._quests[i] = Quest.Read(reader);
        }
        return quests;
    }

    public void Dispose()
    {
        for (int i = 0; i < _quests.Length; i++)
        {
            Interlocked.Exchange(ref _quests[i]!, null)?.Dispose();
        }
    }
}

public sealed class ActVQuests : IDisposable
{
    private readonly Quest[] _quests = new Quest[16];

    public Quest Introduction => _quests[0];
    //2 shorts after ActV introduction. presumably for extra quests never used.
    public Quest Extra1 => _quests[1];
    public Quest Extra2 => _quests[2];
    public Quest SiegeOnHarrogath => _quests[3];
    public Quest RescueOnMountArreat => _quests[4];
    public Quest PrisonOfIce => _quests[5];
    public Quest BetrayalOfHarrogath => _quests[6];
    public Quest RiteOfPassage => _quests[7];
    public Quest EveOfDestruction => _quests[8];
    public Quest Completion => _quests[9];
    //6 shorts after ActV completion. presumably for extra quests never used.
    public Quest Extra3 => _quests[10];
    public Quest Extra4 => _quests[11];
    public Quest Extra5 => _quests[12];
    public Quest Extra6 => _quests[13];
    public Quest Extra7 => _quests[14];
    public Quest Extra8 => _quests[15];

    public void Write(IBitWriter writer)
    {
        for (int i = 0; i < _quests.Length; i++)
        {
            _quests[i].Write(writer);
        }
    }

    public static ActVQuests Read(IBitReader reader)
    {
        var quests = new ActVQuests();
        for (int i = 0; i < quests._quests.Length; i++)
        {
            quests._quests[i] = Quest.Read(reader);
        }
        return quests;
    }

    public void Dispose()
    {
        for (int i = 0; i < _quests.Length; i++)
        {
            Interlocked.Exchange(ref _quests[i]!, null)?.Dispose();
        }
    }
}

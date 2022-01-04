using D2SLib.IO;

namespace D2SLib.Model.Save;

public sealed class NPCDialogSection
{
    private readonly NPCDialogDifficulty[] _difficulties = new NPCDialogDifficulty[3];

    //0x02c9 [npc header identifier  = 0x01, 0x77 ".w"]
    public ushort? Header { get; set; }
    //0x02ca [npc header length = 0x34]
    public ushort? Length { get; set; }
    public NPCDialogDifficulty Normal => _difficulties[0];
    public NPCDialogDifficulty Nightmare => _difficulties[1];
    public NPCDialogDifficulty Hell => _difficulties[2];

    public void Write(IBitWriter writer)
    {
        writer.WriteUInt16(Header ?? 0x7701);
        writer.WriteUInt16(Length ?? 0x34);

        int start = writer.Position;

        for (int i = 0; i < _difficulties.Length; i++)
        {
            _difficulties[i].Write(writer);
        }

        writer.SeekBits(start + (0x30 * 8));
    }

    public static NPCDialogSection Read(IBitReader reader)
    {
        var npcDialogSection = new NPCDialogSection
        {
            Header = reader.ReadUInt16(),
            Length = reader.ReadUInt16()
        };

        Span<byte> bytes = stackalloc byte[0x30];
        reader.ReadBytes(bytes);
        using var bits = new InternalBitArray(bytes);

        for (int i = 0; i < npcDialogSection._difficulties.Length; i++)
        {
            npcDialogSection._difficulties[i] = NPCDialogDifficulty.Read(bits);
        }

        return npcDialogSection;
    }

    [Obsolete("Try the direct-read overload!")]
    public static NPCDialogSection Read(ReadOnlySpan<byte> bytes)
    {
        using var reader = new BitReader(bytes);
        return Read(reader);
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(NPCDialogSection npcDialogSection)
    {
        using var writer = new BitWriter();
        npcDialogSection.Write(writer);
        return writer.ToArray();
    }
}

//8 bytes per difficulty for Intro for each Difficulty followed by 8 bytes per difficulty for Congrats for each difficulty
public sealed class NPCDialogDifficulty
{
    private readonly NPCDialogData[] _dialogs = new NPCDialogData[41];

    private NPCDialogDifficulty() { }

    public NPCDialogData WarrivActII => _dialogs[0];
    public NPCDialogData Unk0x0001 => _dialogs[1];
    public NPCDialogData Charsi => _dialogs[2];
    public NPCDialogData WarrivActI => _dialogs[3];
    public NPCDialogData Kashya => _dialogs[4];
    public NPCDialogData Akara => _dialogs[5];
    public NPCDialogData Gheed => _dialogs[6];
    public NPCDialogData Unk0x0007 => _dialogs[7];
    public NPCDialogData Greiz => _dialogs[8];
    public NPCDialogData Jerhyn => _dialogs[9];
    public NPCDialogData MeshifActII => _dialogs[10];
    public NPCDialogData Geglash => _dialogs[11];
    public NPCDialogData Lysander => _dialogs[12];
    public NPCDialogData Fara => _dialogs[13];
    public NPCDialogData Drogan => _dialogs[14];
    public NPCDialogData Unk0x000F => _dialogs[15];
    public NPCDialogData Alkor => _dialogs[16];
    public NPCDialogData Hratli => _dialogs[17];
    public NPCDialogData Ashera => _dialogs[18];
    public NPCDialogData Unk0x0013 => _dialogs[19];
    public NPCDialogData Unk0x0014 => _dialogs[20];
    public NPCDialogData CainActIII => _dialogs[21];
    public NPCDialogData Unk0x0016 => _dialogs[22];
    public NPCDialogData Elzix => _dialogs[23];
    public NPCDialogData Malah => _dialogs[24];
    public NPCDialogData Anya => _dialogs[25];
    public NPCDialogData Unk0x001A => _dialogs[26];
    public NPCDialogData Natalya => _dialogs[27];
    public NPCDialogData MeshifActIII => _dialogs[28];
    public NPCDialogData Unk0x001D => _dialogs[29];
    public NPCDialogData Unk0x001F => _dialogs[30];
    public NPCDialogData Ormus => _dialogs[31];
    public NPCDialogData Unk0x0021 => _dialogs[32];
    public NPCDialogData Unk0x0022 => _dialogs[33];
    public NPCDialogData Unk0x0023 => _dialogs[34];
    public NPCDialogData Unk0x0024 => _dialogs[35];
    public NPCDialogData Unk0x0025 => _dialogs[36];
    public NPCDialogData CainActV => _dialogs[37];
    public NPCDialogData Qualkehk => _dialogs[38];
    public NPCDialogData Nihlathak => _dialogs[39];
    public NPCDialogData Unk0x0029 => _dialogs[40];

    // 23 bits here unused

    public void Write(IBitWriter writer)
    {
        for (int i = 0; i < _dialogs.Length; i++)
        {
            var data = _dialogs[i];
            int position = writer.Position;
            writer.WriteBit(data.Introduction);
            writer.SeekBits(position + (0x18 * 8));
            writer.WriteBit(data.Congratulations);
            writer.SeekBits(position + 1);
        }
    }

    internal static NPCDialogDifficulty Read(InternalBitArray bits)
    {
        var output = new NPCDialogDifficulty();

        for (int i = 0; i < output._dialogs.Length; i++)
        {
            var data = new NPCDialogData
            {
                Introduction = bits[i],
                Congratulations = bits[i + (0x18 * 8)]
            };
            output._dialogs[i] = data;
        }

        return output;
    }
}

public sealed class NPCDialogData
{
    public bool Introduction { get; set; }
    public bool Congratulations { get; set; }
}

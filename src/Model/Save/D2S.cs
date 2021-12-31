using D2SLib.IO;
using Microsoft.Toolkit.HighPerformance.Buffers;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace D2SLib.Model.Save;

public sealed class D2S : IDisposable
{
    private D2S(IBitReader reader)
    {
        Header = Header.Read(reader);
        ActiveWeapon = reader.ReadUInt32();
        Name = reader.ReadString(16);
        Status = Status.Read(reader.ReadByte());
        Progression = reader.ReadByte();
        Unk0x0026 = reader.ReadBytes(2);
        ClassId = reader.ReadByte();
        Unk0x0029 = reader.ReadBytes(2);
        Level = reader.ReadByte();
        Created = reader.ReadUInt32();
        LastPlayed = reader.ReadUInt32();
        Unk0x0034 = reader.ReadBytes(4);
        AssignedSkills = Enumerable.Range(0, 16).Select(_ => Skill.Read(reader)).ToArray();
        LeftSkill = Skill.Read(reader);
        RightSkill = Skill.Read(reader);
        LeftSwapSkill = Skill.Read(reader);
        RightSwapSkill = Skill.Read(reader);
        Appearances = Appearances.Read(reader);
        Location = Locations.Read(reader);
        MapId = reader.ReadUInt32();
        Unk0x00af = reader.ReadBytes(2);
        Mercenary = Mercenary.Read(reader);
        RealmData = reader.ReadBytes(140);
        Quests = QuestsSection.Read(reader);
        Waypoints = WaypointsSection.Read(reader);
        NPCDialog = NPCDialogSection.Read(reader);
        Attributes = Attributes.Read(reader);

        ClassSkills = ClassSkills.Read(reader, ClassId);
        PlayerItemList = ItemList.Read(reader, Header.Version);
        PlayerCorpses = CorpseList.Read(reader, Header.Version);

        if (Status.IsExpansion)
        {
            MercenaryItemList = MercenaryItemList.Read(reader, Mercenary, Header.Version);
            Golem = Golem.Read(reader, Header.Version);
        }
    }

    //0x0000
    public Header Header { get; set; }
    //0x0010
    public uint ActiveWeapon { get; set; }
    //0x0014 sizeof(16)
    public string Name { get; set; }
    //0x0024
    public Status Status { get; set; }
    //0x0025
    [JsonIgnore]
    public byte Progression { get; set; }
    //0x0026 [unk = 0x0, 0x0]
    [JsonIgnore]
    public byte[]? Unk0x0026 { get; set; }
    //0x0028
    public byte ClassId { get; set; }
    //0x0029 [unk = 0x10, 0x1E]
    [JsonIgnore]
    public byte[]? Unk0x0029 { get; set; }
    //0x002b
    public byte Level { get; set; }
    //0x002c
    public uint Created { get; set; }
    //0x0030
    public uint LastPlayed { get; set; }
    //0x0034 [unk = 0xff, 0xff, 0xff, 0xff]
    [JsonIgnore]
    public byte[]? Unk0x0034 { get; set; }
    //0x0038
    public Skill[] AssignedSkills { get; set; }
    //0x0078
    public Skill LeftSkill { get; set; }
    //0x007c
    public Skill RightSkill { get; set; }
    //0x0080
    public Skill LeftSwapSkill { get; set; }
    //0x0084
    public Skill RightSwapSkill { get; set; }
    //0x0088 [char menu appearance]
    public Appearances Appearances { get; set; }
    //0x00a8
    public Locations Location { get; set; }
    //0x00ab
    public uint MapId { get; set; }
    //0x00af [unk = 0x0, 0x0]
    [JsonIgnore]
    public byte[]? Unk0x00af { get; set; }
    //0x00b1
    public Mercenary Mercenary { get; set; }
    //0x00bf [unk = 0x0] (server related data)
    [JsonIgnore]
    public byte[]? RealmData { get; set; }
    //0x014b
    public QuestsSection Quests { get; set; }
    //0x0279
    public WaypointsSection Waypoints { get; set; }
    //0x02c9
    public NPCDialogSection NPCDialog { get; set; }
    //0x2fc
    public Attributes Attributes { get; set; }

    public ClassSkills ClassSkills { get; set; }

    public ItemList PlayerItemList { get; set; }
    public CorpseList PlayerCorpses { get; set; }
    public MercenaryItemList? MercenaryItemList { get; set; }
    public Golem? Golem { get; set; }

    public void Write(IBitWriter writer)
    {
        Header.Write(writer);
        writer.WriteUInt32(ActiveWeapon);
        writer.WriteString(Name, 16);
        Status.Write(writer);
        writer.WriteByte(Progression);
        //Unk0x0026
        writer.WriteBytes(Unk0x0026 ?? new byte[2]);
        writer.WriteByte(ClassId);
        //Unk0x0029
        writer.WriteBytes(Unk0x0029 ?? stackalloc byte[] { 0x10, 0x1e });
        writer.WriteByte(Level);
        writer.WriteUInt32(Created);
        writer.WriteUInt32(LastPlayed);
        //Unk0x0034
        writer.WriteBytes(Unk0x0034 ?? stackalloc byte[] { 0xff, 0xff, 0xff, 0xff });
        for (int i = 0; i < 16; i++)
        {
            AssignedSkills[i].Write(writer);
        }
        LeftSkill.Write(writer);
        RightSkill.Write(writer);
        LeftSwapSkill.Write(writer);
        RightSwapSkill.Write(writer);
        Appearances.Write(writer);
        Location.Write(writer);
        writer.WriteUInt32(MapId);
        //0x00af [unk = 0x0, 0x0]
        writer.WriteBytes(Unk0x00af ?? new byte[2]);
        Mercenary.Write(writer);
        //0x00bf [unk = 0x0] (server related data)
        writer.WriteBytes(RealmData ?? new byte[140]);
        Quests.Write(writer);
        Waypoints.Write(writer);
        NPCDialog.Write(writer);
        Attributes.Write(writer);
        ClassSkills.Write(writer);
        PlayerItemList.Write(writer, Header.Version);
        PlayerCorpses.Write(writer, Header.Version);
        if (Status.IsExpansion)
        {
            MercenaryItemList?.Write(writer, Mercenary, Header.Version);
            Golem?.Write(writer, Header.Version);
        }
    }

    public static D2S Read(ReadOnlySpan<byte> bytes)
    {
        using var reader = new BitReader(bytes);
        var d2s = new D2S(reader);
        Debug.Assert(reader.Position == (bytes.Length * 8));
        return d2s;
    }

    public static MemoryOwner<byte> WritePooled(D2S d2s)
    {
        using var writer = new BitWriter();
        d2s.Write(writer);
        var bytes = writer.ToPooledArray();
        Header.Fix(bytes.Span);
        return bytes;
    }

    public static byte[] Write(D2S d2s)
    {
        using var writer = new BitWriter();
        d2s.Write(writer);
        byte[] bytes = writer.ToArray();
        Header.Fix(bytes);
        return bytes;
    }

    public void Dispose()
    {
        Waypoints.Dispose();
        Status.Dispose();
        Quests.Dispose();
        PlayerItemList.Dispose();
        PlayerCorpses.Dispose();
        MercenaryItemList?.Dispose();
    }
}

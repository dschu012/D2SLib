using D2SLib.IO;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace D2SLib.Model.Save;

public class D2S
{
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
    public MercenaryItemList MercenaryItemList { get; set; }
    public Golem Golem { get; set; }

    public static D2S Read(ReadOnlySpan<byte> bytes)
    {
        using var reader = new BitReader(bytes);
        var d2s = new D2S
        {
            Header = Header.Read(reader),
            ActiveWeapon = reader.ReadUInt32(),
            Name = reader.ReadString(16),
            Status = Status.Read(reader.ReadByte()),
            Progression = reader.ReadByte(),
            Unk0x0026 = reader.ReadBytes(2),
            ClassId = reader.ReadByte(),
            Unk0x0029 = reader.ReadBytes(2),
            Level = reader.ReadByte(),
            Created = reader.ReadUInt32(),
            LastPlayed = reader.ReadUInt32(),
            Unk0x0034 = reader.ReadBytes(4),
            AssignedSkills = Enumerable.Range(0, 16).Select(e => Skill.Read(reader)).ToArray(),
            LeftSkill = Skill.Read(reader),
            RightSkill = Skill.Read(reader),
            LeftSwapSkill = Skill.Read(reader),
            RightSwapSkill = Skill.Read(reader),
            Appearances = Appearances.Read(reader),
            Location = Locations.Read(reader),
            MapId = reader.ReadUInt32(),
            Unk0x00af = reader.ReadBytes(2),
            Mercenary = Mercenary.Read(reader),
            RealmData = reader.ReadBytes(140),
            Quests = QuestsSection.Read(reader.ReadBytes(302)),
            Waypoints = WaypointsSection.Read(reader.ReadBytes(80)),
            NPCDialog = NPCDialogSection.Read(reader.ReadBytes(52)),
            Attributes = Attributes.Read(reader)
        };
        d2s.ClassSkills = ClassSkills.Read(reader, d2s.ClassId);
        d2s.PlayerItemList = ItemList.Read(reader, d2s.Header.Version);
        d2s.PlayerCorpses = CorpseList.Read(reader, d2s.Header.Version);
        if (d2s.Status.IsExpansion)
        {
            d2s.MercenaryItemList = MercenaryItemList.Read(reader, d2s.Mercenary, d2s.Header.Version);
            d2s.Golem = Golem.Read(reader, d2s.Header.Version);
        }
        Debug.Assert(reader.Position == (bytes.Length * 8));
        return d2s;
    }

    public static byte[] Write(D2S d2s)
    {
        using var writer = new BitWriter();
        d2s.Header.Write(writer);
        writer.WriteUInt32(d2s.ActiveWeapon);
        writer.WriteString(d2s.Name, 16);
        d2s.Status.Write(writer);
        writer.WriteByte(d2s.Progression);
        //Unk0x0026
        writer.WriteBytes(d2s.Unk0x0026 ?? new byte[2]);
        writer.WriteByte(d2s.ClassId);
        //Unk0x0029
        writer.WriteBytes(d2s.Unk0x0029 ?? stackalloc byte[] { 0x10, 0x1e });
        writer.WriteByte(d2s.Level);
        writer.WriteUInt32(d2s.Created);
        writer.WriteUInt32(d2s.LastPlayed);
        //Unk0x0034
        writer.WriteBytes(d2s.Unk0x0034 ?? stackalloc byte[] { 0xff, 0xff, 0xff, 0xff });
        for (int i = 0; i < 16; i++)
        {
            d2s.AssignedSkills[i].Write(writer);
        }
        d2s.LeftSkill.Write(writer);
        d2s.RightSkill.Write(writer);
        d2s.LeftSwapSkill.Write(writer);
        d2s.RightSwapSkill.Write(writer);
        d2s.Appearances.Write(writer);
        d2s.Location.Write(writer);
        writer.WriteUInt32(d2s.MapId);
        //0x00af [unk = 0x0, 0x0]
        writer.WriteBytes(d2s.Unk0x00af ?? new byte[2]);
        d2s.Mercenary.Write(writer);
        //0x00bf [unk = 0x0] (server related data)
        writer.WriteBytes(d2s.RealmData ?? new byte[140]);
        writer.WriteBytes(QuestsSection.Write(d2s.Quests));
        writer.WriteBytes(WaypointsSection.Write(d2s.Waypoints));
        writer.WriteBytes(NPCDialogSection.Write(d2s.NPCDialog));
        d2s.Attributes.Write(writer);
        d2s.ClassSkills.Write(writer);
        d2s.PlayerItemList.Write(writer, d2s.Header.Version);
        writer.WriteBytes(CorpseList.Write(d2s.PlayerCorpses, d2s.Header.Version));
        if (d2s.Status.IsExpansion)
        {
            d2s.MercenaryItemList.Write(writer, d2s.Mercenary, d2s.Header.Version);
            writer.WriteBytes(Golem.Write(d2s.Golem, d2s.Header.Version));
        }
        byte[] bytes = writer.ToArray();
        Header.Fix(bytes);
        return bytes;
    }
}

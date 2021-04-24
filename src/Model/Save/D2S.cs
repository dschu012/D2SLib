using D2SLib.IO;
using D2SLib.Model.Save;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace D2SLib.Model.Save
{
    public class D2S
    {
        //0x0000
        public Header Header { get; set; }
        //0x0010
        public UInt32 ActiveWeapon { get; set; }
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
        public UInt32 Created { get; set; }
        //0x0030
        public UInt32 LastPlayed { get; set; }
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
        public UInt32 MapId { get; set; }
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

        public static D2S Read(byte[] bytes)
        {
            using (BitReader reader = new BitReader(bytes))
            {
                D2S d2s = new D2S();
                d2s.Header = Header.Read(reader.ReadBytes(16));
                d2s.ActiveWeapon = reader.ReadUInt32();
                d2s.Name = reader.ReadString(16);
                d2s.Status = Status.Read(reader.ReadByte());
                d2s.Progression = reader.ReadByte();
                d2s.Unk0x0026 = reader.ReadBytes(2);
                d2s.ClassId = reader.ReadByte();
                d2s.Unk0x0029 = reader.ReadBytes(2);
                d2s.Level = reader.ReadByte();
                d2s.Created = reader.ReadUInt32();
                d2s.LastPlayed = reader.ReadUInt32();
                d2s.Unk0x0034 = reader.ReadBytes(4);
                d2s.AssignedSkills = Enumerable.Range(0, 16).Select(e => Skill.Read(reader.ReadBytes(4))).ToArray();
                d2s.LeftSkill = Skill.Read(reader.ReadBytes(4));
                d2s.RightSkill = Skill.Read(reader.ReadBytes(4));
                d2s.LeftSwapSkill = Skill.Read(reader.ReadBytes(4));
                d2s.RightSwapSkill = Skill.Read(reader.ReadBytes(4));
                d2s.Appearances = Appearances.Read(reader.ReadBytes(32));
                d2s.Location = Locations.Read(reader.ReadBytes(3));
                d2s.MapId = reader.ReadUInt32();
                d2s.Unk0x00af = reader.ReadBytes(2);
                d2s.Mercenary = Mercenary.Read(reader.ReadBytes(14));
                d2s.RealmData = reader.ReadBytes(140);
                d2s.Quests = QuestsSection.Read(reader.ReadBytes(302));
                d2s.Waypoints = WaypointsSection.Read(reader.ReadBytes(80));
                d2s.NPCDialog = NPCDialogSection.Read(reader.ReadBytes(52));
                d2s.Attributes = Attributes.Read(reader);
                d2s.ClassSkills = ClassSkills.Read(reader.ReadBytes(32), d2s.ClassId);
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
        }

        public static byte[] Write(D2S d2s)
        {
            using (BitWriter writer = new BitWriter()) {
                writer.WriteBytes(Header.Write(d2s.Header));
                writer.WriteUInt32(d2s.ActiveWeapon);
                writer.WriteString(d2s.Name, 16);
                writer.WriteBytes(Status.Write(d2s.Status));
                writer.WriteByte(d2s.Progression);
                //Unk0x0026
                writer.WriteBytes(d2s.Unk0x0026 ?? new byte[2]);
                writer.WriteByte(d2s.ClassId);
                //Unk0x0029
                writer.WriteBytes(d2s.Unk0x0029 ?? new byte[] { 0x10, 0x1e });
                writer.WriteByte(d2s.Level);
                writer.WriteUInt32(d2s.Created);
                writer.WriteUInt32(d2s.LastPlayed);
                //Unk0x0034
                writer.WriteBytes(d2s.Unk0x0034 ?? new byte[] { 0xff, 0xff, 0xff, 0xff });
                for(int i = 0; i < 16; i ++)
                {
                    writer.WriteBytes(Skill.Write(d2s.AssignedSkills[i]));
                }
                writer.WriteBytes(Skill.Write(d2s.LeftSkill));
                writer.WriteBytes(Skill.Write(d2s.RightSkill));
                writer.WriteBytes(Skill.Write(d2s.LeftSwapSkill));
                writer.WriteBytes(Skill.Write(d2s.RightSwapSkill));
                writer.WriteBytes(Appearances.Write(d2s.Appearances));
                writer.WriteBytes(Locations.Write(d2s.Location));
                writer.WriteUInt32(d2s.MapId);
                //0x00af [unk = 0x0, 0x0]
                writer.WriteBytes(d2s.Unk0x00af ?? new byte[2]);
                writer.WriteBytes(Mercenary.Write(d2s.Mercenary));
                //0x00bf [unk = 0x0] (server related data)
                writer.WriteBytes(d2s.RealmData ?? new byte[140]);
                writer.WriteBytes(QuestsSection.Write(d2s.Quests));
                writer.WriteBytes(WaypointsSection.Write(d2s.Waypoints));
                writer.WriteBytes(NPCDialogSection.Write(d2s.NPCDialog));
                writer.WriteBytes(Attributes.Write(d2s.Attributes));
                writer.WriteBytes(ClassSkills.Write(d2s.ClassSkills));
                writer.WriteBytes(ItemList.Write(d2s.PlayerItemList, d2s.Header.Version));
                writer.WriteBytes(CorpseList.Write(d2s.PlayerCorpses, d2s.Header.Version));
                if (d2s.Status.IsExpansion)
                {
                    writer.WriteBytes(MercenaryItemList.Write(d2s.MercenaryItemList, d2s.Mercenary, d2s.Header.Version));
                    writer.WriteBytes(Golem.Write(d2s.Golem, d2s.Header.Version));
                }
                byte[] bytes =  writer.ToArray();
                Header.Fix(bytes);
                return bytes;
            }
        }
    }
}

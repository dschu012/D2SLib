using D2SLib.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace D2SLib.Model.Save
{
    public class NPCDialogSection
    {
        //0x02c9 [npc header identifier  = 0x01, 0x77 ".w"]
        public UInt16? Header { get; set; }
        //0x02ca [npc header length = 0x34]
        public UInt16? Length { get; set; }
        public NPCDialogDifficulty Normal { get; set; }
        public NPCDialogDifficulty Nightmare { get; set; }
        public NPCDialogDifficulty Hell { get; set; }

        public static NPCDialogSection Read(byte[] bytes)
        {
            NPCDialogSection npcDialogSection = new NPCDialogSection();
            using (BitReader reader = new BitReader(bytes))
            {
                npcDialogSection.Header = reader.ReadUInt16();
                npcDialogSection.Length = reader.ReadUInt16();
                Type type = typeof(NPCDialogDifficulty);
                BitArray bits = new BitArray(reader.ReadBytes(0x30));
                //Introductions
                var skippedProperties = new string[] { "Header", "Length" };
                foreach (var npcDialogSectionProperty in typeof(NPCDialogSection).GetProperties())
                {
                    if (skippedProperties.Contains(npcDialogSectionProperty.Name)) continue;
                    NPCDialogDifficulty npcDialogDifficulty = new NPCDialogDifficulty();
                    int idx = 0;
                    foreach (var property in typeof(NPCDialogDifficulty).GetProperties())
                    {
                        NPCDialogData data = new NPCDialogData();
                        data.Introduction = bits[idx];
                        data.Congratulations = bits[idx + (0x18 * 8)];
                        idx++;
                        property.SetValue(npcDialogDifficulty, data);
                    }
                    npcDialogSectionProperty.SetValue(npcDialogSection, npcDialogDifficulty);
                }
                return npcDialogSection;
            }
        }

        public static byte[] Write(NPCDialogSection npcDialogSection)
        {
            using (BitWriter writer = new BitWriter())
            {
                writer.WriteUInt16(npcDialogSection.Header ?? (UInt16)0x7701);
                writer.WriteUInt16(npcDialogSection.Length ?? (UInt16)0x34);
                var skippedProperties = new string[] { "Header", "Length" };
                var start = writer.Position;
                foreach (var npcDialogSectionProperty in typeof(NPCDialogSection).GetProperties())
                {
                    if (skippedProperties.Contains(npcDialogSectionProperty.Name)) continue;
                    NPCDialogDifficulty npcDialogDifficulty = (NPCDialogDifficulty)npcDialogSectionProperty.GetValue(npcDialogSection);
                    foreach (var property in typeof(NPCDialogDifficulty).GetProperties())
                    {
                        NPCDialogData npcDialogData = (NPCDialogData)property.GetValue(npcDialogDifficulty);
                        int position = writer.Position;
                        writer.WriteBit(npcDialogData.Introduction);
                        writer.SeekBits(position + (0x18 * 8));
                        writer.WriteBit(npcDialogData.Congratulations);
                        writer.SeekBits(position + 1);
                    }
                }
                writer.SeekBits(start + (0x30 * 8));
                return writer.ToArray();
            }
        }

    }

    //8 bytes per difficulty for Intro for each Difficulty followed by 8 bytes per difficulty for Congrats for each difficulty
    public class NPCDialogDifficulty
    {
        public NPCDialogData WarrivActII { get; set; }
        public NPCDialogData Unk0x0001 { get; set; }
        public NPCDialogData Charsi { get; set; }
        public NPCDialogData WarrivActI { get; set; }
        public NPCDialogData Kashya { get; set; }
        public NPCDialogData Akara { get; set; }
        public NPCDialogData Gheed { get; set; }
        public NPCDialogData Unk0x0007 { get; set; }
        public NPCDialogData Greiz { get; set; }
        public NPCDialogData Jerhyn { get; set; }
        public NPCDialogData MeshifActII { get; set; }
        public NPCDialogData Geglash { get; set; }
        public NPCDialogData Lysander { get; set; }
        public NPCDialogData Fara { get; set; }
        public NPCDialogData Drogan { get; set; }
        public NPCDialogData Unk0x000F { get; set; }
        public NPCDialogData Alkor { get; set; }
        public NPCDialogData Hratli { get; set; }
        public NPCDialogData Ashera { get; set; }
        public NPCDialogData Unk0x0013 { get; set; }
        public NPCDialogData Unk0x0014 { get; set; }
        public NPCDialogData CainActIII { get; set; }
        public NPCDialogData Unk0x0016 { get; set; }
        public NPCDialogData Elzix { get; set; }
        public NPCDialogData Malah { get; set; }
        public NPCDialogData Anya { get; set; }
        public NPCDialogData Unk0x001A { get; set; }
        public NPCDialogData Natalya { get; set; }
        public NPCDialogData MeshifActIII { get; set; }
        public NPCDialogData Unk0x001D { get; set; }
        public NPCDialogData Unk0x001F { get; set; }
        public NPCDialogData Ormus { get; set; }
        public NPCDialogData Unk0x0021 { get; set; }
        public NPCDialogData Unk0x0022 { get; set; }
        public NPCDialogData Unk0x0023 { get; set; }
        public NPCDialogData Unk0x0024 { get; set; }
        public NPCDialogData Unk0x0025 { get; set; }
        public NPCDialogData CainActV { get; set; }
        public NPCDialogData Qualkehk { get; set; }
        public NPCDialogData Nihlathak { get; set; }
        public NPCDialogData Unk0x0029 { get; set; }

        // 23 bits here unused

    }

    public class NPCDialogData
    {
        public bool Introduction { get; set; }
        public bool Congratulations { get; set; }
    }
}

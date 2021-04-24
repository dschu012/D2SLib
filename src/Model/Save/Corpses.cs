using D2SLib.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace D2SLib.Model.Save
{
    public class CorpseList
    {
        public UInt16? Header { get; set; }
        public UInt16 Count { get; set; }

        public List<Corpse> Corpses { get; set; } = new List<Corpse>();

        public static CorpseList Read(BitReader reader, UInt32 version)
        {
            CorpseList corpseList = new CorpseList();
            corpseList.Header = reader.ReadUInt16();
            corpseList.Count = reader.ReadUInt16();
            for (int i = 0; i < corpseList.Count; i++)
            {
                corpseList.Corpses.Add(Corpse.Read(reader, version));
            }
            return corpseList;
        }

        public static byte[] Write(CorpseList corpseList, UInt32 version)
        {
            using (BitWriter writer = new BitWriter())
            {
                writer.WriteUInt16(corpseList.Header ?? 0x4D4A);
                writer.WriteUInt16(corpseList.Count);
                for (int i = 0; i < corpseList.Count; i++)
                {
                    writer.WriteBytes(Corpse.Write(corpseList.Corpses[i], version));
                }
                return writer.ToArray();
            }
        }
    }

    public class Corpse
    {
        public UInt32? Unk0x0 { get; set; }
        public UInt32 X { get; set; }
        public UInt32 Y { get; set; }
        public ItemList ItemList { get; set; }
        public static Corpse Read(BitReader reader, UInt32 version)
        {
            Corpse corpse = new Corpse();
            corpse.Unk0x0 = reader.ReadUInt32();
            corpse.X = reader.ReadUInt32();
            corpse.Y = reader.ReadUInt32();
            corpse.ItemList = ItemList.Read(reader, version);
            return corpse;
        }

        public static byte[] Write(Corpse corpse, UInt32 version)
        {
            using (BitWriter writer = new BitWriter())
            {
                writer.WriteUInt32(corpse.Unk0x0 ?? (UInt32)0x0);
                writer.WriteUInt32(corpse.X);
                writer.WriteUInt32(corpse.Y);
                writer.WriteBytes(ItemList.Write(corpse.ItemList, version));
                return writer.ToArray();
            }
        }
    }
}


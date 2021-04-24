using D2SLib.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace D2SLib.Model.Save
{
    public class Mercenary
    {
        //is this right?
        public UInt16 IsDead { get; set; }
        public UInt32 Id { get; set; }
        public UInt16 NameId { get; set; }
        public UInt16 TypeId { get; set; }
        public UInt32 Experience { get; set; }

        public static Mercenary Read(byte[] bytes)
        {
            Mercenary mercenary = new Mercenary();
            using (BitReader reader = new BitReader(bytes))
            {
                mercenary.IsDead = reader.ReadUInt16();
                mercenary.Id = reader.ReadUInt32();
                mercenary.NameId = reader.ReadUInt16();
                mercenary.TypeId = reader.ReadUInt16();
                mercenary.Experience = reader.ReadUInt32();
                return mercenary;
            }
        }

        public static byte[] Write(Mercenary mercenary)
        {
            using (BitWriter writer = new BitWriter())
            {
                writer.WriteUInt16(mercenary.IsDead);
                writer.WriteUInt32(mercenary.Id);
                writer.WriteUInt16(mercenary.NameId);
                writer.WriteUInt16(mercenary.TypeId);
                writer.WriteUInt32(mercenary.Experience);
                return writer.ToArray();
            }
        }
    }

    public class MercenaryItemList
    {
        public UInt16? Header { get; set; }
        public ItemList ItemList { get; set; }

        public static MercenaryItemList Read(BitReader reader, Mercenary mercenary, UInt32 version)
        {
            MercenaryItemList mercenaryItemList = new MercenaryItemList();
            mercenaryItemList.Header = reader.ReadUInt16();
            if(mercenary.Id != 0)
            {
                mercenaryItemList.ItemList = ItemList.Read(reader, version);
            }
            return mercenaryItemList;
        }

        public static byte[] Write(MercenaryItemList mercenaryItemList, Mercenary mercenary, UInt32 version)
        {
            using (BitWriter writer = new BitWriter())
            {
                writer.WriteUInt16(mercenaryItemList.Header ?? 0x666A);
                if (mercenary.Id != 0)
                {
                    writer.WriteBytes(ItemList.Write(mercenaryItemList.ItemList, version));
                }
                return writer.ToArray();
            }
        }
    }
}

using D2SLib.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace D2SLib.Model.Save
{
    public class Golem
    {
        public UInt16? Header { get; set; }
        public bool Exists { get; set; }
        public Item Item { get; set; }

        public static Golem Read(BitReader reader, UInt32 version)
        {
            Golem golem = new Golem();
            golem.Header = reader.ReadUInt16();
            golem.Exists = reader.ReadByte() == 1;
            if(golem.Exists)
            {
                golem.Item = Item.Read(reader, version);
            }
            return golem;
        }

        public static byte[] Write(Golem golem, UInt32 version)
        {
            using (BitWriter writer = new BitWriter())
            {
                writer.WriteUInt16(golem.Header ?? 0x666B);
                writer.WriteByte((byte)(golem.Exists ? 1 : 0));
                if(golem.Exists)
                {
                    writer.WriteBytes(Item.Write(golem.Item, version));
                }
                return writer.ToArray();
            }
        }
    }
}

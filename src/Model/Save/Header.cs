using D2SLib.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace D2SLib.Model.Save
{
    public class Header
    {
        //0x0000
        public UInt32? Magic { get; set; }
        //0x0004
        public UInt32 Version { get; set; }
        //0x0008
        public UInt32 Filesize { get; set; }
        //0x000c
        public UInt32 Checksum { get; set; }

        public static Header Read(byte[] bytes)
        {
            using (BitReader reader = new BitReader(bytes))
            {
                Header header = new Header();
                header.Magic = reader.ReadUInt32();
                header.Version = reader.ReadUInt32();
                header.Filesize = reader.ReadUInt32();
                header.Checksum = reader.ReadUInt32();
                return header;
            }
        }

        public static byte[] Write(Header header)
        {
            using(BitWriter writer = new BitWriter())
            {
                writer.WriteUInt32(header.Magic ?? 0xAA55AA55);
                writer.WriteUInt32(header.Version);
                writer.WriteUInt32(header.Filesize);
                writer.WriteUInt32(header.Checksum);
                return writer.ToArray();
            }
        }

        public static void Fix(byte[] bytes)
        {
            FixSize(bytes);
            FixChecksum(bytes);
        }

        public static void FixSize(byte[] bytes)
        {
            byte[] length = BitConverter.GetBytes((UInt32)bytes.Length);
            length.CopyTo(bytes, 0x8);
        }
        public static void FixChecksum(byte[] bytes)
        {
            new byte[4].CopyTo(bytes, 0xc);
            Int32 checksum = 0;
            for(int i = 0; i < bytes.Length; i++)
            {
                checksum = bytes[i] + (checksum * 2) + (checksum < 0 ? 1 : 0);
            }
            BitConverter.GetBytes(checksum).CopyTo(bytes, 0xc);
        }
    }
}

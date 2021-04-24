using D2SLib.Model.Save;
using System;
using System.IO;

namespace D2SLib
{
    public class Core
    {
        private static TXT _TXT = null;
        public static TXT TXT
        {
            get
            {
                return _TXT ?? ResourceFilesTXT.Instance.TXT;
            }
            set
            {
                _TXT = value;
            }
        }

        public static D2S ReadD2S(string path)
        {
            return D2S.Read(File.ReadAllBytes(path));
        }

        public static D2S ReadD2S(byte[] bytes)
        {
            return D2S.Read(bytes);
        }

        public static Item ReadItem(string path, UInt32 version)
        {
            return ReadItem(File.ReadAllBytes(path), version);
        }

        public static Item ReadItem(byte[] bytes, UInt32 version)
        {
            return Item.Read(bytes, version);
        }

        public static D2I ReadD2I(string path, UInt32 version)
        {
            return ReadD2I(File.ReadAllBytes(path), version);
        }

        public static D2I ReadD2I(byte[] bytes, UInt32 version)
        {
            return D2I.Read(bytes, version);
        }

        public static byte[] WriteD2S(D2S d2s)
        {
            return D2S.Write(d2s);
        }

        public static byte[] WriteItem(Item item, UInt32 version)
        {
            return Item.Write(item, version);
        }

        public static byte[] WriteD2I(D2I d2i, UInt32 version)
        {
            return D2I.Write(d2i, version);
        }

    }
}

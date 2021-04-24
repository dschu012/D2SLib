using D2SLib.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace D2SLib.Model.Save
{
    public class Status
    {
        [JsonIgnore]
        public BitArray? Flags { get; set; }
        public bool IsHardcore { get { return Flags[2]; } set { Flags[2] = value; } }
        public bool IsDead { get { return Flags[3]; } set { Flags[3] = value; } }
        public bool IsExpansion { get { return Flags[5]; } set { Flags[5] = value; } }
        public bool IsLadder{ get { return Flags[6]; } set { Flags[6] = value; } }

        public static Status Read(byte bytes)
        {
            Status status = new Status();
            status.Flags = new BitArray(new byte[] { bytes });
            return status;
        }

        public static byte[] Write(Status status)
        {
            using (BitWriter writer = new BitWriter())
            {
                BitArray bits = status.Flags;
                if (bits == null)
                {
                    bits = new BitArray(8);
                    bits[2] = status.IsHardcore;
                    bits[3] = status.IsDead;
                    bits[4] = status.IsExpansion;
                    bits[5] = status.IsLadder;
                }
                foreach(var bit in bits.Cast<bool>())
                {
                    writer.WriteBit(bit);
                }
                return writer.ToArray();
            }
        }
    }
}

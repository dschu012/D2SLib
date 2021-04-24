using D2SLib.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace D2SLib.Model.Save
{
    public class Locations
    {
        public Location Normal { get; set; }
        public Location Nightmare { get; set; }
        public Location Hell { get; set; }

        public static Locations Read(byte[] bytes)
        {
            Locations locations = new Locations();
            using (BitReader reader = new BitReader(bytes))
            {
                foreach (var property in typeof(Locations).GetProperties())
                {
                    Location location = new Location();
                    byte b = reader.ReadByte();
                    location.Active = (b >> 7) == 1;
                    location.Act = (byte)((b & 0x5) + 1);
                    property.SetValue(locations, location);
                }
                return locations;
            }
        }

        public static byte[] Write(Locations locations)
        {
            using (BitWriter writer = new BitWriter())
            {
                foreach (var property in typeof(Locations).GetProperties())
                {
                    Location location = (Location)property.GetValue(locations);
                    byte b = 0x0;
                    if (location.Active) b |= 0x7;
                    b |= (byte)(location.Act - 1);
                    writer.WriteByte(b);
                }
                return writer.ToArray();
            }
        }
    }

    public class Location
    {
        public bool Active { get; set; }
        public byte Act { get; set; }
    }
}

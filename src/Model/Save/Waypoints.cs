using D2SLib.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace D2SLib.Model.Save
{
    public class WaypointsSection
    {
        //0x0279 [waypoint data = 0x57, 0x53 "WS"]
        public UInt16? Header { get; set; }
        //0x027b [waypoint header version = 0x1, 0x0, 0x0, 0x0]
        public UInt32? Version { get; set; }
        //0x027f [waypoint header length = 0x50, 0x0]
        public UInt16? Length { get; set; }
        public WaypointsDifficulty Normal { get; set; }
        public WaypointsDifficulty Nightmare { get; set; }
        public WaypointsDifficulty Hell { get; set; }

        public static WaypointsSection Read(byte[] bytes)
        {
            WaypointsSection waypointsSection = new WaypointsSection();
            using (BitReader reader = new BitReader(bytes))
            {
                waypointsSection.Header = reader.ReadUInt16();
                waypointsSection.Version = reader.ReadUInt32();
                waypointsSection.Length = reader.ReadUInt16();
                var skippedProperties = new string[] { "Magic", "Header", "Version", "Length" };
                foreach (var property in typeof(WaypointsSection).GetProperties())
                {
                    if (skippedProperties.Contains(property.Name)) continue;
                    property.SetValue(waypointsSection, WaypointsDifficulty.Read(reader.ReadBytes(24)));
                }
                return waypointsSection;
            }
        }

        public static byte[] Write(WaypointsSection waypointsSection)
        {
            using (BitWriter writer = new BitWriter())
            {
                writer.WriteUInt16(waypointsSection.Header ?? (UInt16)0x5357);
                writer.WriteUInt32(waypointsSection.Version ?? 0x1);
                writer.WriteUInt16(waypointsSection.Length ?? (UInt16)0x50);
                var skippedProperties = new string[] { "Header", "Version", "Length" };
                foreach (var property in typeof(WaypointsSection).GetProperties())
                {
                    if (skippedProperties.Contains(property.Name)) continue;
                    WaypointsDifficulty waypointsDifficulty = (WaypointsDifficulty)property.GetValue(waypointsSection);
                    writer.WriteBytes(WaypointsDifficulty.Write(waypointsDifficulty));
                }

                return writer.ToArray();
            }
        }
    }

    public class WaypointsDifficulty
    {
        //[0x02, 0x01]
        public UInt16? Header { get; set; }
        public ActIWaypoints ActI { get; set; }
        public ActIIWaypoints ActII { get; set; }
        public ActIIIWaypoints ActIII { get; set; }
        public ActIVWaypoints ActIV { get; set; }
        public ActVWaypoints ActV { get; set; }

        public static WaypointsDifficulty Read(byte[] bytes)
        {
            WaypointsDifficulty waypointsDifficulty = new WaypointsDifficulty();
            using (BitReader reader = new BitReader(bytes))
            {
                waypointsDifficulty.Header = reader.ReadUInt16();
                BitArray bits = new BitArray(reader.ReadBytes(17));
                int i = 0;
                var skippedProperties = new string[] { "Header" };
                foreach (var waypointsDifficultyProperty in typeof(WaypointsDifficulty).GetProperties())
                {
                    if (skippedProperties.Contains(waypointsDifficultyProperty.Name)) continue;
                    Type type = waypointsDifficultyProperty.PropertyType;
                    var waypoints = Activator.CreateInstance(type);
                    foreach (var property in type.GetProperties())
                    {
                        property.SetValue(waypoints, bits[i++]);
                    }
                    waypointsDifficultyProperty.SetValue(waypointsDifficulty, waypoints);
                }
                return waypointsDifficulty;
            }
        }

        public static byte[] Write(WaypointsDifficulty waypointsDifficulty)
        {
            using (BitWriter writer = new BitWriter())
            {
                writer.WriteUInt16(waypointsDifficulty.Header ?? (UInt16)0x102);
                var skippedProperties = new string[] { "Header" };
                Type waypointsDifficultyType = typeof(WaypointsDifficulty);
                foreach (var waypointsDifficultyProperty in waypointsDifficultyType.GetProperties())
                {
                    if (skippedProperties.Contains(waypointsDifficultyProperty.Name)) continue;
                    Type type = waypointsDifficultyProperty.PropertyType;
                    var waypoints = waypointsDifficultyProperty.GetValue(waypointsDifficulty);
                    foreach (var property in type.GetProperties())
                    {
                        writer.WriteBit((bool)property.GetValue(waypoints));
                    }
                }
                writer.Align();
                writer.WriteBytes(new byte[17]);
                return writer.ToArray();
            }
        }
    }

    public class ActIWaypoints
    {
        public bool RogueEncampement { get; set; }
        public bool ColdPlains { get; set; }
        public bool StonyField { get; set; }
        public bool DarkWoods { get; set; }
        public bool BlackMarsh { get; set; }
        public bool OuterCloister { get; set; }
        public bool JailLvl1 { get; set; }
        public bool InnerCloister { get; set; }
        public bool CatacombsLvl2 { get; set; }
    }
    public class ActIIWaypoints
    {
        public bool LutGholein { get; set; } 
        public bool SewersLvl2 { get; set; }
        public bool DryHills { get; set; }
        public bool HallsOfTheDeadLvl2 { get; set; }
        public bool FarOasis { get; set; }
        public bool LostCity { get; set; }
        public bool PalaceCellarLvl1 { get; set; }
        public bool ArcaneSanctuary { get; set; }
        public bool CanyonOfTheMagi { get; set; }
    }
    public class ActIIIWaypoints
    {
        public bool KurastDocks { get; set; }
        public bool SpiderForest { get; set; }
        public bool GreatMarsh { get; set; }
        public bool FlayerJungle { get; set; }
        public bool LowerKurast { get; set; }
        public bool KurastBazaar { get; set; }
        public bool UpperKurast { get; set; }
        public bool Travincal { get; set; }
        public bool DuranceOfHateLvl2 { get; set; }
    }
    public class ActIVWaypoints
    {
        public bool ThePandemoniumFortress { get; set; }
        public bool CityOfTheDamned { get; set; }
        public bool RiverOfFlame { get; set; }
    }
    public class ActVWaypoints
    {
        public bool Harrogath { get; set; }
        public bool FrigidHighlands { get; set; }
        public bool ArreatPlateau { get; set; }
        public bool CrystallinePassage { get; set; }
        public bool HallsOfPain { get; set; }
        public bool GlacialTrail { get; set; }
        public bool FrozenTundra { get; set; }
        public bool TheAncientsWay { get; set; }
        public bool WorldstoneKeepLvl2 { get; set; }
    }

}

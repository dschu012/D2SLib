using D2SLib.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace D2SLib.Model.Save
{
    public class Appearances
    {
        public Appearance Head { get; set; }
        public Appearance Torso { get; set; }
        public Appearance Legs { get; set; }
        public Appearance RightArm { get; set; }
        public Appearance LeftArm { get; set; }
        public Appearance RightHand { get; set; }
        public Appearance LeftHand { get; set; }
        public Appearance Shield { get; set; }
        public Appearance Special1 { get; set; }
        public Appearance Special2 { get; set; }
        public Appearance Special3 { get; set; }
        public Appearance Special4 { get; set; }
        public Appearance Special5 { get; set; }
        public Appearance Special6 { get; set; }
        public Appearance Special7 { get; set; }
        public Appearance Special8 { get; set; }

        public static Appearances Read(byte[] bytes)
        {
            Appearances appearances = new Appearances();
            using (BitReader reader = new BitReader(bytes))
            {
                foreach (var property in typeof(Appearances).GetProperties())
                {
                    Appearance Appearance = new Appearance();
                    Appearance.Graphic = reader.ReadByte();
                    property.SetValue(appearances, Appearance);
                }
                foreach (var property in typeof(Appearances).GetProperties())
                {
                    Appearance Appearance = (Appearance)property.GetValue(appearances);
                    Appearance.Tint = reader.ReadByte();
                }
                return appearances;
            }
        }

        public static byte[] Write(Appearances appearances)
        {
            using (BitWriter writer = new BitWriter())
            {
                foreach (var property in typeof(Appearances).GetProperties())
                {
                    Appearance appearance = (Appearance)property.GetValue(appearances);
                    writer.WriteByte(appearance.Graphic);
                }
                foreach (var property in typeof(Appearances).GetProperties())
                {
                    Appearance appearance = (Appearance)property.GetValue(appearances);
                    writer.WriteByte(appearance.Tint);
                }
                return writer.ToArray();
            }
        }

    }

    public class Appearance
    {
        public byte Graphic { get; set; }
        public byte Tint { get; set; }
    }
}

using D2SLib.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace D2SLib.Model.Save
{
    public class ClassSkills
    {
        private static readonly UInt32[] SKILL_OFFSETS = { 6, 36, 66, 96, 126, 221, 251 };
        public UInt16? Header { get; set; }
        public List<ClassSkill> Skills { get; set; } = new List<ClassSkill>();

        public ClassSkill this[int i] => this.Skills[i];

        public static ClassSkills Read(byte[] bytes, int playerClass)
        {
            ClassSkills classSkills = new ClassSkills();
            using(BitReader reader = new BitReader(bytes))
            {
                classSkills.Header = reader.ReadUInt16();
                uint offset = SKILL_OFFSETS[playerClass];
                for (uint i = 0; i < 30; i++)
                {
                    ClassSkill skill = ClassSkill.Read(reader.ReadByte());
                    skill.Id = offset + i;
                    classSkills.Skills.Add(skill);
                }
                return classSkills;
            }
        }

        public static byte[] Write(ClassSkills classSkills)
        {
            using (BitWriter writer = new BitWriter())
            {
                writer.WriteUInt16(classSkills.Header ?? (UInt16)0x6669);
                for (int i = 0; i < 30; i++)
                {
                    writer.WriteBytes(ClassSkill.Write(classSkills.Skills[i]));
                }
                return writer.ToArray();
            }
        }
    }

    public class ClassSkill
    {
        public UInt32 Id { get; set; }
        public byte Points { get; set; }

        public static ClassSkill Read(byte b)
        {
            ClassSkill classSkill = new ClassSkill();
            classSkill.Points = b;
            return classSkill;
        }

        public static byte[] Write(ClassSkill classSkill)
        {
            using (BitWriter writer = new BitWriter())
            {
                writer.WriteByte(classSkill.Points);
                return writer.ToArray();
            }
        }
    }

    //header skill
    public class Skill
    {
        public UInt32 Id { get; set; }
        public static Skill Read(byte[] bytes)
        {
            Skill skill = new Skill();
            skill.Id = BitConverter.ToUInt32(bytes);
            return skill;
        }

        public static byte[] Write(Skill skill)
        {
            using (BitWriter writer = new BitWriter())
            {
                writer.WriteUInt32(skill.Id);
                return writer.ToArray();
            }
        }
    }
}

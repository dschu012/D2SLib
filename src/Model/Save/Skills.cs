using D2SLib.IO;

namespace D2SLib.Model.Save;

public class ClassSkills
{
    private const int SKILL_COUNT = 30;

    private static readonly uint[] SKILL_OFFSETS = { 6, 36, 66, 96, 126, 221, 251 };
    public ushort? Header { get; set; }
    public List<ClassSkill> Skills { get; } = new List<ClassSkill>(SKILL_COUNT);

    public ClassSkill this[int i] => Skills[i];

    public void Write(IBitWriter writer)
    {
        writer.WriteUInt16(Header ?? 0x6669);
        for (int i = 0; i < SKILL_COUNT; i++)
        {
            Skills[i].Write(writer);
        }
    }

    public static ClassSkills Read(IBitReader reader, int playerClass)
    {
        var classSkills = new ClassSkills
        {
            Header = reader.ReadUInt16()
        };
        uint offset = SKILL_OFFSETS[playerClass];
        for (uint i = 0; i < SKILL_COUNT; i++)
        {
            var skill = ClassSkill.Read(offset + i, reader.ReadByte());
            classSkills.Skills.Add(skill);
        }
        return classSkills;
    }

    [Obsolete("Try the direct-read overload!")]
    public static ClassSkills Read(ReadOnlySpan<byte> bytes, int playerClass)
    {
        using var reader = new BitReader(bytes);
        return Read(reader, playerClass);
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(ClassSkills classSkills)
    {
        using var writer = new BitWriter();
        classSkills.Write(writer);
        return writer.ToArray();
    }
}

public class ClassSkill
{
    public uint Id { get; set; }
    public byte Points { get; set; }

    public void Write(IBitWriter writer) => writer.WriteByte(Points);

    public static ClassSkill Read(uint id, byte points)
    {
        var classSkill = new ClassSkill
        {
            Id = id,
            Points = points
        };
        return classSkill;
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(ClassSkill classSkill)
    {
        using var writer = new BitWriter();
        classSkill.Write(writer);
        return writer.ToArray();
    }
}

//header skill
public class Skill
{
    public uint Id { get; set; }

    public void Write(IBitWriter writer) => writer.WriteUInt32(Id);

    public static Skill Read(IBitReader reader)
    {
        var skill = new Skill
        {
            Id = reader.ReadUInt32()
        };
        return skill;
    }

    [Obsolete("Try the direct-read overload!")]
    public static Skill Read(ReadOnlySpan<byte> bytes)
    {
        var skill = new Skill
        {
            Id = BitConverter.ToUInt32(bytes)
        };
        return skill;
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(Skill skill)
    {
        using var writer = new BitWriter();
        skill.Write(writer);
        return writer.ToArray();
    }
}

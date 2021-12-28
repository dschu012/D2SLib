using D2SLib.Model.Data;
using System.Reflection;

namespace D2SLib;

public sealed class ResourceFilesData
{
    private ResourceFilesData()
    {
        MetaData = new MetaData();
        using (Stream s = GetResource("ItemStatCost.txt"))
        {
            MetaData.ItemStatCostData = ItemStatCostData.Read(s);
        }
        using (Stream s = GetResource("Armor.txt"))
        {
            MetaData.ItemsData.ArmorData = ArmorData.Read(s);
        }
        using (Stream s = GetResource("Weapons.txt"))
        {
            MetaData.ItemsData.WeaponsData = WeaponsData.Read(s);
        }
        using (Stream s = GetResource("Misc.txt"))
        {
            MetaData.ItemsData.MiscData = MiscData.Read(s);
        }
    }

    public static ResourceFilesData Instance { get; } = new();

    public MetaData MetaData { get; set; }

    private static Stream GetResource(string file)
    {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceStream($"D2SLib.Resources.{file}")
            ?? throw new InvalidOperationException($"{file} was not found in embedded resources.");
    }
}

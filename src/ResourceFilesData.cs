using D2SLib.Model.Data;
using System.Reflection;

namespace D2SLib;

public sealed class ResourceFilesData
{
    private ResourceFilesData()
    {
        ArmorData armorData;
        WeaponsData weaponsData;
        MiscData miscData;
        ItemStatCostData itemStatCostData;

        using (Stream s = GetResource("ItemStatCost.txt"))
        {
            itemStatCostData = ItemStatCostData.Read(s);
        }
        using (Stream s = GetResource("Armor.txt"))
        {
            armorData = ArmorData.Read(s);
        }
        using (Stream s = GetResource("Weapons.txt"))
        {
            weaponsData = WeaponsData.Read(s);
        }
        using (Stream s = GetResource("Misc.txt"))
        {
            miscData = MiscData.Read(s);
        }

        MetaData = new MetaData(itemStatCostData, new ItemsData(armorData, weaponsData, miscData));
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

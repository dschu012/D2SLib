using D2SLib.Model.TXT;
using System.Reflection;

namespace D2SLib;

public sealed class ResourceFilesTXT
{
    private ResourceFilesTXT()
    {
        TXT = new TXT();
        using (Stream s = GetResource("ItemStatCost.txt"))
        {
            TXT.ItemStatCostTXT = ItemStatCostTXT.Read(s);
        }
        using (Stream s = GetResource("Armor.txt"))
        {
            TXT.ItemsTXT.ArmorTXT = ArmorTXT.Read(s);
        }
        using (Stream s = GetResource("Weapons.txt"))
        {
            TXT.ItemsTXT.WeaponsTXT = WeaponsTXT.Read(s);
        }
        using (Stream s = GetResource("Misc.txt"))
        {
            TXT.ItemsTXT.MiscTXT = MiscTXT.Read(s);
        }
    }

    public static ResourceFilesTXT Instance { get; } = new();

    public TXT TXT { get; set; }

    private static Stream GetResource(string file)
    {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceStream($"D2SLib.Resources.{file}")
            ?? throw new InvalidOperationException($"{file} was not found in embedded resources.");
    }
}

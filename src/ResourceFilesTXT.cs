using D2SLib.Model.TXT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace D2SLib
{
    public sealed class ResourceFilesTXT
    {
        static ResourceFilesTXT() { }
        private ResourceFilesTXT() { }
        public static ResourceFilesTXT Instance { get; } = Init();

        public TXT TXT { get; set; }
        private static ResourceFilesTXT Init()
        {
            ResourceFilesTXT resourceFilesTXT = new ResourceFilesTXT();
            resourceFilesTXT.TXT = new TXT();
            using(Stream s = GetResource("ItemStatCost.txt")) {
                resourceFilesTXT.TXT.ItemStatCostTXT = ItemStatCostTXT.Read(s);
            }
            using (Stream s = GetResource("Armor.txt"))
            {
                resourceFilesTXT.TXT.ItemsTXT.ArmorTXT = ArmorTXT.Read(s);
            }
            using (Stream s = GetResource("Weapons.txt"))
            {
                resourceFilesTXT.TXT.ItemsTXT.WeaponsTXT = WeaponsTXT.Read(s);
            }
            using (Stream s = GetResource("Misc.txt"))
            {
                resourceFilesTXT.TXT.ItemsTXT.MiscTXT = MiscTXT.Read(s);
            }
            return resourceFilesTXT;
        }

        private static Stream GetResource(string file)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream($"D2SLib.Resources.{file}");
        }
    }
}

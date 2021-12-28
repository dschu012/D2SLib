using D2SLib.Model.Huffman;

namespace D2SLib.Model.Data;

//collections or ArmorData MiscData WeaponsData with helper methods
public sealed class ItemsData
{
    public ArmorData ArmorData { get; set; }
    public WeaponsData WeaponsData { get; set; }
    public MiscData MiscData { get; set; }

    private HuffmanTree? _ItemCodeTree = null;
    public HuffmanTree ItemCodeTree
    {
        get
        {
            if (_ItemCodeTree == null)
            {
                _ItemCodeTree = InitializeHuffmanTree();
            }
            return _ItemCodeTree;
        }
        set => _ItemCodeTree = value;
    }

    public DataRow? this[string i] => GetByCode(i);

    public DataRow? GetByCode(string code)
    {
        return ArmorData[code] ??
            WeaponsData[code] ??
            MiscData[code];
    }

    public bool IsArmor(string code) => ArmorData[code] != null;

    public bool IsWeapon(string code) => WeaponsData[code] != null;

    public bool IsMisc(string code) => MiscData[code] != null;

    private HuffmanTree InitializeHuffmanTree()
    {
        /*
        List<string> items = new List<string>();
        foreach(var row in ArmorData.Rows)
        {
            items.Add(row["code"]);
        }
        foreach (var row in WeaponsData.Rows)
        {
            items.Add(row["code"]);
        }
        foreach (var row in MiscData.Rows)
        {
            items.Add(row["code"]);
        }
        */
        var itemCodeTree = new HuffmanTree();
        itemCodeTree.Build(new List<string>());
        return itemCodeTree;
    }

}

public sealed class ArmorData : DataFile
{
    public DataRow? this[string i] => GetByColumnAndValue("code", i);

    public static ArmorData Read(Stream data)
    {
        var armor = new ArmorData();
        armor.ReadData(data);
        return armor;
    }

    public static ArmorData Read(string file)
    {
        using Stream stream = File.OpenRead(file);
        return Read(stream);
    }
}

public sealed class WeaponsData : DataFile
{
    public DataRow? this[string i] => GetByColumnAndValue("code", i);

    public static WeaponsData Read(Stream data)
    {
        var weapons = new WeaponsData();
        weapons.ReadData(data);
        return weapons;
    }
    public static WeaponsData Read(string file)
    {
        using Stream stream = File.OpenRead(file);
        return Read(stream);
    }
}

public sealed class MiscData : DataFile
{
    public DataRow? this[string i] => GetByColumnAndValue("code", i);

    public static MiscData Read(Stream data)
    {
        var misc = new MiscData();
        misc.ReadData(data);
        return misc;
    }
    public static MiscData Read(string file)
    {
        using Stream stream = File.OpenRead(file);
        return Read(stream);
    }
}

using D2SLib.Model.Huffman;

namespace D2SLib.Model.Data;

//collections or ArmorData MiscData WeaponsData with helper methods
public sealed class ItemsData
{
    public ItemsData(ArmorData armorData, WeaponsData weaponsData, MiscData miscData)
    {
        ArmorData = armorData;
        WeaponsData = weaponsData;
        MiscData = miscData;
    }

    public ArmorData ArmorData { get; }
    public WeaponsData WeaponsData { get; }
    public MiscData MiscData { get; }

    private HuffmanTree? _itemCodeTree = null;
    internal HuffmanTree ItemCodeTree
    {
        get => _itemCodeTree ??= InitializeHuffmanTree();
        set => _itemCodeTree = value;
    }

    public DataRow? this[string code] => GetByCode(code);

    public DataRow? GetByCode(string code)
    {
        return ArmorData[code]
            ?? WeaponsData[code] 
            ?? MiscData[code];
    }

    public bool IsArmor(string code) => ArmorData[code] is not null;

    public bool IsWeapon(string code) => WeaponsData[code] is not null;

    public bool IsMisc(string code) => MiscData[code] is not null;

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
        itemCodeTree.Build();
        return itemCodeTree;
    }
}

public sealed class ArmorData : DataFile
{
    public DataRow? this[string code] => GetByColumnAndValue("code", code);

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
    public DataRow? this[string code] => GetByColumnAndValue("code", code);

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
    public DataRow? this[string code] => GetByColumnAndValue("code", code);

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

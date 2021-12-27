﻿using D2SLib.IO;
using D2SLib.Model.TXT;
using System.Text;
using System.Text.Json.Serialization;

namespace D2SLib.Model.Save;

public enum ItemMode
{
    Stored = 0x0,
    Equipped = 0x1,
    Belt = 0x2,
    Buffer = 0x4,
    Socket = 0x6,
}

public enum ItemLocation
{
    None,
    Head,
    Neck,
    Torso,
    RightHand,
    LeftHand,
    RightFinger,
    LeftFinger,
    Waist,
    Feet,
    Gloves,
    SwapRight,
    SwapLeft
}

public enum ItemQuality
{
    Inferior = 0x1,
    Normal,
    Superior,
    Magic,
    Set,
    Rare,
    Unique,
    Craft,
    Tempered
}

public class ItemList
{
    public ushort? Header { get; set; }
    public ushort Count { get; set; }
    public List<Item> Items { get; set; } = new List<Item>();

    public static ItemList Read(BitReader reader, uint version)
    {
        var itemList = new ItemList
        {
            Header = reader.ReadUInt16(),
            Count = reader.ReadUInt16()
        };
        for (int i = 0; i < itemList.Count; i++)
        {
            itemList.Items.Add(Item.Read(reader, version));
        }
        return itemList;
    }

    public static byte[] Write(ItemList itemList, uint version)
    {
        using (var writer = new BitWriter())
        {
            writer.WriteUInt16(itemList.Header ?? 0x4D4A);
            writer.WriteUInt16(itemList.Count);
            for (int i = 0; i < itemList.Count; i++)
            {
                writer.WriteBytes(Item.Write(itemList.Items[i], version));
            }
            return writer.ToArray();
        }
    }
}

public class Item
{
    public ushort? Header { get; set; }
    [JsonIgnore]
    public IList<bool>? Flags { get; set; }
    public string Version { get; set; }
    public ItemMode Mode { get; set; }
    public ItemLocation Location { get; set; }
    public byte X { get; set; }
    public byte Y { get; set; }
    public byte Page { get; set; }
    public byte EarLevel { get; set; }
    public string PlayerName { get; set; } //used for personalized or ears
    public string Code { get; set; }
    public byte NumberOfSocketedItems { get; set; }
    public byte TotalNumberOfSockets { get; set; }
    public List<Item> SocketedItems { get; set; } = new List<Item>();
    public uint Id { get; set; }
    public byte ItemLevel { get; set; }
    public ItemQuality Quality { get; set; }
    public bool HasMultipleGraphics { get; set; }
    public byte GraphicId { get; set; }
    public bool IsAutoAffix { get; set; }
    public ushort AutoAffixId { get; set; } //?
    public uint FileIndex { get; set; }
    public ushort[] MagicPrefixIds { get; set; } = new ushort[3];
    public ushort[] MagicSuffixIds { get; set; } = new ushort[3];
    public ushort RarePrefixId { get; set; }
    public ushort RareSuffixId { get; set; }
    public uint RunewordId { get; set; }
    [JsonIgnore]
    public bool HasRealmData { get; set; }
    [JsonIgnore]
    public uint[] RealmData { get; set; } = new uint[3];
    public ushort Armor { get; set; }
    public ushort MaxDurability { get; set; }
    public ushort Durability { get; set; }
    public ushort Quantity { get; set; }
    public byte SetItemMask { get; set; }
    public List<ItemStatList> StatLists { get; set; } = new List<ItemStatList>();
    public bool IsIdentified { get => Flags[4]; set => Flags[4] = value; }
    public bool IsSocketed { get => Flags[11]; set => Flags[11] = value; }
    public bool IsNew { get => Flags[13]; set => Flags[13] = value; }
    public bool IsEar { get => Flags[16]; set => Flags[16] = value; }
    public bool IsStarterItem { get => Flags[17]; set => Flags[17] = value; }
    public bool IsCompact { get => Flags[21]; set => Flags[21] = value; }
    public bool IsEthereal { get => Flags[22]; set => Flags[22] = value; }
    public bool IsPersonalized { get => Flags[24]; set => Flags[24] = value; }
    public bool IsRuneword { get => Flags[26]; set => Flags[26] = value; }

    public static Item Read(byte[] bytes, uint version)
    {
        using (var reader = new BitReader(bytes))
        {
            return Read(reader, version);
        }
    }

    public static Item Read(BitReader reader, uint version)
    {
        var item = new Item();
        if (version <= 0x60)
        {
            item.Header = reader.ReadUInt16();
        }
        ReadCompact(reader, item, version);
        if (!item.IsCompact)
        {
            ReadComplete(reader, item, version);
        }
        reader.Align();
        for (int i = 0; i < item.NumberOfSocketedItems; i++)
        {
            item.SocketedItems.Add(Read(reader, version));
        }
        return item;
    }

    public static byte[] Write(Item item, uint version)
    {
        using (var writer = new BitWriter())
        {
            if (version <= 0x60)
            {
                writer.WriteUInt16(item.Header ?? 0x4D4A);
            }
            WriteCompact(writer, item, version);
            if (!item.IsCompact)
            {
                WriteComplete(writer, item, version);
            }
            writer.Align();
            for (int i = 0; i < item.NumberOfSocketedItems; i++)
            {
                writer.WriteBytes(Item.Write(item.SocketedItems[i], version));
            }
            return writer.ToArray();
        }
    }

    protected static string ReadPlayerName(BitReader reader)
    {
        char[] name = new char[15];
        for (int i = 0; i < name.Length; i++)
        {
            name[i] = (char)reader.ReadByte(7);
            if (name[i] == '\0')
            {
                break;
            }
        }
        return new string(name);
    }

    protected static void WritePlayerName(BitWriter writer, string name)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(name.Replace("\0", ""));
        for (int i = 0; i < bytes.Length; i++)
        {
            writer.WriteByte(bytes[i], 7);
        }
        writer.WriteByte((byte)'\0', 7);
    }

    protected static void ReadCompact(BitReader reader, Item item, uint version)
    {
        Span<byte> bytes = stackalloc byte[4];
        reader.ReadBytes(4, bytes);
        item.Flags = new InternalBitArray(bytes);
        if (version <= 0x60)
        {
            item.Version = Convert.ToString(reader.ReadUInt16(10), 10);
        }
        else if (version >= 0x61)
        {
            item.Version = Convert.ToString(reader.ReadUInt16(3), 2);
        }
        item.Mode = (ItemMode)reader.ReadByte(3);
        item.Location = (ItemLocation)reader.ReadByte(4);
        item.X = reader.ReadByte(4);
        item.Y = reader.ReadByte(4);
        item.Page = reader.ReadByte(3);
        if (item.IsEar)
        {
            item.FileIndex = reader.ReadByte(3);
            item.EarLevel = reader.ReadByte(7);
            item.PlayerName = ReadPlayerName(reader);
        }
        else
        {
            item.Code = "";
            if (version <= 0x60)
            {
                item.Code = reader.ReadString(4);
            }
            else if (version >= 0x61)
            {
                for (int i = 0; i < 4; i++)
                {
                    item.Code += Core.TXT.ItemsTXT.ItemCodeTree.DecodeChar(reader);
                }
            }
            item.NumberOfSocketedItems = reader.ReadByte(item.IsCompact ? 1 : 3);
        }
    }

    protected static void WriteCompact(BitWriter writer, Item item, uint version)
    {
        if (item.Flags is not InternalBitArray flags)
        {
            flags = new InternalBitArray(32);
            flags[4] = item.IsIdentified;
            flags[11] = item.IsSocketed;
            flags[13] = item.IsNew;
            flags[16] = item.IsEar;
            flags[17] = item.IsStarterItem;
            flags[21] = item.IsCompact;
            flags[22] = item.IsEthereal;
            flags[24] = item.IsPersonalized;
            flags[26] = item.IsRuneword;
        }
        writer.WriteBits(flags);
        if (version <= 0x60)
        {
            //todo. how do we handle 1.15 version to 1.14. maybe this should be a string
            writer.WriteUInt16(Convert.ToUInt16(item.Version, 10), 10);
        }
        else if (version >= 0x61)
        {
            writer.WriteUInt16(Convert.ToUInt16(item.Version, 2), 3);
        }
        writer.WriteByte((byte)item.Mode, 3);
        writer.WriteByte((byte)item.Location, 4);
        writer.WriteByte(item.X, 4);
        writer.WriteByte(item.Y, 4);
        writer.WriteByte(item.Page, 3);
        if (item.IsEar)
        {
            writer.WriteUInt32(item.FileIndex, 3);
            writer.WriteByte(item.EarLevel, 7);
            WritePlayerName(writer, item.PlayerName);
        }
        else
        {
            byte[]? code = Encoding.ASCII.GetBytes(item.Code.PadRight(4, ' '));
            if (version <= 0x60)
            {
                writer.WriteBytes(code);
            }
            else if (version >= 0x61)
            {
                for (int i = 0; i < 4; i++)
                {
                    var bits = Core.TXT.ItemsTXT.ItemCodeTree.EncodeChar((char)code[i]);
                    foreach (bool bit in bits.Cast<bool>())
                    {
                        writer.WriteBit(bit);
                    }
                }
            }
            writer.WriteByte(item.NumberOfSocketedItems, item.IsCompact ? 1 : 3);
        }

    }

    protected static void ReadComplete(BitReader reader, Item item, uint version)
    {
        item.Id = reader.ReadUInt32();
        item.ItemLevel = reader.ReadByte(7);
        item.Quality = (ItemQuality)reader.ReadByte(4);
        item.HasMultipleGraphics = reader.ReadBit();
        if (item.HasMultipleGraphics)
        {
            item.GraphicId = reader.ReadByte(3);
        }
        item.IsAutoAffix = reader.ReadBit();
        if (item.IsAutoAffix)
        {
            item.AutoAffixId = reader.ReadUInt16(11);
        }
        switch (item.Quality)
        {
            case ItemQuality.Normal:
                break;
            case ItemQuality.Inferior:
            case ItemQuality.Superior:
                item.FileIndex = reader.ReadUInt16(3);
                break;
            case ItemQuality.Magic:
                item.MagicPrefixIds[0] = reader.ReadUInt16(11);
                item.MagicSuffixIds[0] = reader.ReadUInt16(11);
                break;
            case ItemQuality.Rare:
            case ItemQuality.Craft:
                item.RarePrefixId = reader.ReadUInt16(8);
                item.RareSuffixId = reader.ReadUInt16(8);
                for (int i = 0; i < 3; i++)
                {
                    if (reader.ReadBit())
                    {
                        item.MagicPrefixIds[i] = reader.ReadUInt16(11);
                    }
                    if (reader.ReadBit())
                    {
                        item.MagicSuffixIds[i] = reader.ReadUInt16(11);
                    }
                }
                break;
            case ItemQuality.Set:
            case ItemQuality.Unique:
                item.FileIndex = reader.ReadUInt16(12);
                break;
        }
        ushort propertyLists = 0;
        if (item.IsRuneword)
        {
            item.RunewordId = reader.ReadUInt32(12);
            propertyLists |= (ushort)(1 << (reader.ReadUInt16(4) + 1));
        }
        if (item.IsPersonalized)
        {
            item.PlayerName = ReadPlayerName(reader);
        }
        if (item.Code.Trim() == "tbk" || item.Code.Trim() == "ibk")
        {
            item.MagicSuffixIds[0] = reader.ReadByte(5);
        }
        item.HasRealmData = reader.ReadBit();
        if (item.HasRealmData)
        {
            //reader.ReadBits(96);
            reader.AdvanceBits(96);
        }
        var itemStatCostTXT = Core.TXT.ItemStatCostTXT;
        var row = Core.TXT.ItemsTXT.GetByCode(item.Code);
        bool isArmor = Core.TXT.ItemsTXT.IsArmor(item.Code);
        bool isWeapon = Core.TXT.ItemsTXT.IsWeapon(item.Code);
        bool isStackable = row["stackable"].ToBool();
        if (isArmor)
        {
            //why do i need this cast?
            item.Armor = (ushort)(reader.ReadUInt16(11) + itemStatCostTXT["armorclass"]["Save Add"].ToUInt16());
        }
        if (isArmor || isWeapon)
        {
            var maxDurabilityStat = itemStatCostTXT["maxdurability"];
            var durabilityStat = itemStatCostTXT["maxdurability"];
            item.MaxDurability = (ushort)(reader.ReadUInt16(maxDurabilityStat["Save Bits"].ToInt32()) + maxDurabilityStat["Save Add"].ToUInt16());
            if (item.MaxDurability > 0)
            {
                item.Durability = (ushort)(reader.ReadUInt16(durabilityStat["Save Bits"].ToInt32()) + durabilityStat["Save Add"].ToUInt16());
                //what is this?
                reader.ReadBit();
            }
        }
        if (isStackable)
        {
            item.Quantity = reader.ReadUInt16(9);
        }
        if (item.IsSocketed)
        {
            item.TotalNumberOfSockets = reader.ReadByte(4);
        }
        item.SetItemMask = 0;
        if (item.Quality == ItemQuality.Set)
        {
            item.SetItemMask = reader.ReadByte(5);
            propertyLists |= item.SetItemMask;
        }
        item.StatLists.Add(ItemStatList.Read(reader));
        for (int i = 1; i <= 64; i <<= 1)
        {
            if ((propertyLists & i) != 0)
            {
                item.StatLists.Add(ItemStatList.Read(reader));
            }
        }
    }

    protected static void WriteComplete(BitWriter writer, Item item, uint version)
    {
        writer.WriteUInt32(item.Id);
        writer.WriteByte(item.ItemLevel, 7);
        writer.WriteByte((byte)item.Quality, 4);
        writer.WriteBit(item.HasMultipleGraphics);
        if (item.HasMultipleGraphics)
        {
            writer.WriteByte(item.GraphicId, 3);
        }
        writer.WriteBit(item.IsAutoAffix);
        if (item.IsAutoAffix)
        {
            writer.WriteUInt16(item.AutoAffixId, 11);
        }
        switch (item.Quality)
        {
            case ItemQuality.Normal:
                break;
            case ItemQuality.Inferior:
            case ItemQuality.Superior:
                writer.WriteUInt32(item.FileIndex, 3);
                break;
            case ItemQuality.Magic:
                writer.WriteUInt16(item.MagicPrefixIds[0], 11);
                writer.WriteUInt16(item.MagicSuffixIds[0], 11);
                break;
            case ItemQuality.Rare:
            case ItemQuality.Craft:
                writer.WriteUInt16(item.RarePrefixId, 8);
                writer.WriteUInt16(item.RareSuffixId, 8);
                for (int i = 0; i < 3; i++)
                {
                    bool hasPrefix = item.MagicPrefixIds[i] > 0;
                    bool hasSuffix = item.MagicSuffixIds[i] > 0;
                    writer.WriteBit(hasPrefix);
                    if (hasPrefix)
                    {
                        writer.WriteUInt16(item.MagicPrefixIds[i], 11);
                    }
                    writer.WriteBit(hasSuffix);
                    if (hasSuffix)
                    {
                        writer.WriteUInt16(item.MagicSuffixIds[i], 11);
                    }
                }
                break;
            case ItemQuality.Set:
            case ItemQuality.Unique:
                writer.WriteUInt32(item.FileIndex, 12);
                break;
        }
        ushort propertyLists = 0;
        if (item.IsRuneword)
        {
            writer.WriteUInt32(item.RunewordId, 12);
            propertyLists |= 1 << 6;
            writer.WriteUInt16(5, 4);
        }
        if (item.IsPersonalized)
        {
            WritePlayerName(writer, item.PlayerName);
        }
        if (item.Code.Trim() == "tbk" || item.Code.Trim() == "ibk")
        {
            writer.WriteUInt16(item.MagicSuffixIds[0], 5);
        }
        writer.WriteBit(item.HasRealmData);
        if (item.HasRealmData)
        {
            //todo 96 bits
        }
        var itemStatCostTXT = Core.TXT.ItemStatCostTXT;
        var row = Core.TXT.ItemsTXT.GetByCode(item.Code);
        bool isArmor = Core.TXT.ItemsTXT.IsArmor(item.Code);
        bool isWeapon = Core.TXT.ItemsTXT.IsWeapon(item.Code);
        bool isStackable = row["stackable"].ToBool();
        if (isArmor)
        {
            writer.WriteUInt16((ushort)(item.Armor - itemStatCostTXT["armorclass"]["Save Add"].ToUInt16()), 11);
        }
        if (isArmor || isWeapon)
        {
            var maxDurabilityStat = itemStatCostTXT["maxdurability"];
            var durabilityStat = itemStatCostTXT["maxdurability"];
            writer.WriteUInt16((ushort)(item.MaxDurability - maxDurabilityStat["Save Add"].ToUInt16()), maxDurabilityStat["Save Bits"].ToInt32());
            if (item.MaxDurability > 0)
            {
                writer.WriteUInt16((ushort)(item.Durability - durabilityStat["Save Add"].ToUInt16()), durabilityStat["Save Bits"].ToInt32());
                ////what is this?
                writer.WriteBit(false);
            }
        }
        if (isStackable)
        {
            writer.WriteUInt16(item.Quantity, 9);
        }
        if (item.IsSocketed)
        {
            writer.WriteByte(item.TotalNumberOfSockets, 4);
        }
        if (item.Quality == ItemQuality.Set)
        {
            writer.WriteByte(item.SetItemMask, 5);
            propertyLists |= item.SetItemMask;
        }
        ItemStatList.Write(writer, item.StatLists[0]);
        int idx = 1;
        for (int i = 1; i <= 64; i <<= 1)
        {
            if ((propertyLists & i) != 0)
            {
                ItemStatList.Write(writer, item.StatLists[idx++]);
            }
        }
    }
}

public class ItemStatList
{
    public List<ItemStat> Stats { get; set; } = new List<ItemStat>();

    public static ItemStatList Read(BitReader reader)
    {
        var itemStatList = new ItemStatList();
        ushort id = reader.ReadUInt16(9);
        while (id != 0x1ff)
        {
            itemStatList.Stats.Add(ItemStat.Read(reader, id));
            //https://github.com/ThePhrozenKeep/D2MOO/blob/master/source/D2Common/src/Items/Items.cpp#L7332
            if (id == 52        //magicmindam
                || id == 17     //item_maxdamage_percent
                || id == 48     //firemindam
                || id == 50)    //lightmindam
            {
                itemStatList.Stats.Add(ItemStat.Read(reader, (ushort)(id + 1)));
            }
            else if (id == 54  //coldmindam
              || id == 57     //poisonmindam
              )
            {
                itemStatList.Stats.Add(ItemStat.Read(reader, (ushort)(id + 1)));
                itemStatList.Stats.Add(ItemStat.Read(reader, (ushort)(id + 2)));
            }
            id = reader.ReadUInt16(9);
        }
        return itemStatList;
    }

    public static void Write(BitWriter writer, ItemStatList itemStatList)
    {
        for (int i = 0; i < itemStatList.Stats.Count; i++)
        {
            var stat = itemStatList.Stats[i];
            var property = ItemStat.GetStatRow(stat);
            ushort id = property["ID"].ToUInt16();
            writer.WriteUInt16(id, 9);
            ItemStat.Write(writer, stat);

            //assume these stats are in order...
            //https://github.com/ThePhrozenKeep/D2MOO/blob/master/source/D2Common/src/Items/Items.cpp#L7332
            if (id == 52        //magicmindam
                || id == 17     //item_maxdamage_percent
                || id == 48     //firemindam
                || id == 50)    //lightmindam
            {
                ItemStat.Write(writer, itemStatList.Stats[++i]);
            }
            else if (id == 54  //coldmindam
              || id == 57     //poisonmindam
              )
            {
                ItemStat.Write(writer, itemStatList.Stats[++i]);
                ItemStat.Write(writer, itemStatList.Stats[++i]);
            }
        }
        writer.WriteUInt16(0x1ff, 9);
    }

}

public class ItemStat
{
    public ushort? Id { get; set; }
    public string? Stat { get; set; }
    public int? SkillTab { get; set; }
    public int? SkillId { get; set; }
    public int? SkillLevel { get; set; }
    public int? MaxCharges { get; set; }
    public int? Param { get; set; }
    public int Value { get; set; }

    public static ItemStat Read(BitReader reader, ushort id)
    {
        var itemStat = new ItemStat();
        var property = Core.TXT.ItemStatCostTXT[id];
        if (property == null)
        {
            throw new Exception($"No ItemStatCost record found for id: {id} at bit {reader.Position - 9}");
        }
        itemStat.Id = id;
        itemStat.Stat = property["Stat"].Value;
        int saveParamBitCount = property["Save Param Bits"].ToInt32();
        int encode = property["Encode"].ToInt32();
        if (saveParamBitCount != 0)
        {
            int saveParam = reader.ReadInt32(saveParamBitCount);
            //todo is there a better way to identify skill tab stats.
            switch (property["descfunc"].ToInt32())
            {
                case 14: //+[value] to [skilltab] Skill Levels ([class] Only) : stat id 188
                    itemStat.SkillTab = saveParam & 0x7;
                    itemStat.SkillLevel = (saveParam >> 3) & 0x1fff;
                    break;
                default:
                    break;
            }
            switch (encode)
            {
                case 2: //chance to cast skill
                case 3: //skill charges
                    itemStat.SkillLevel = saveParam & 0x3f;
                    itemStat.SkillId = (saveParam >> 6) & 0x3ff;
                    break;
                case 1:
                case 4: //by times
                default:
                    itemStat.Param = saveParam;
                    break;
            }
        }
        int saveBits = reader.ReadInt32(property["Save Bits"].ToInt32());
        saveBits -= property["Save Add"].ToInt32();
        switch (encode)
        {
            case 3: //skill charges
                itemStat.MaxCharges = (saveBits >> 8) & 0xff;
                itemStat.Value = saveBits & 0xff;
                break;
            default:
                itemStat.Value = saveBits;
                break;
        }
        return itemStat;
    }

    public static void Write(BitWriter writer, ItemStat stat)
    {
        var property = GetStatRow(stat);
        if (property == null)
        {
            throw new Exception($"No ItemStatCost record found for id: {stat.Id}");
        }
        int saveParamBitCount = property["Save Param Bits"].ToInt32();
        int encode = property["Encode"].ToInt32();
        if (saveParamBitCount != 0)
        {
            if (stat.Param != null)
            {
                writer.WriteInt32((int)stat.Param, saveParamBitCount);
            }
            else
            {
                int saveParamBits = 0;
                switch (property["descfunc"].ToInt32())
                {
                    case 14: //+[value] to [skilltab] Skill Levels ([class] Only) : stat id 188
                        saveParamBits |= (stat.SkillTab ?? 0 & 0x7);
                        saveParamBits |= ((stat.SkillLevel ?? 0 & 0x1fff) << 3);
                        break;
                    default:
                        break;
                }
                switch (encode)
                {
                    case 2: //chance to cast skill
                    case 3: //skill charges
                        saveParamBits |= (stat.SkillLevel ?? 0 & 0x3f);
                        saveParamBits |= ((stat.SkillId ?? 0 & 0x3ff) << 6);
                        break;
                    case 4: //by times
                    case 1:
                    default:
                        break;
                }
                //always use param if it is there.
                if (stat.Param != null)
                {
                    saveParamBits = (int)stat.Param;
                }
                writer.WriteInt32(saveParamBits, saveParamBitCount);
            }
        }
        int saveBits = stat.Value;
        saveBits += property["Save Add"].ToInt32();
        switch (encode)
        {
            case 3: //skill charges
                saveBits &= 0xff;
                saveBits |= ((stat.MaxCharges ?? 0 & 0xff) << 8);
                break;
            default:
                break;
        }
        writer.WriteInt32(saveBits, property["Save Bits"].ToInt32());
    }
    public static TXTRow GetStatRow(ItemStat stat)
    {
        if (stat.Id != null)
        {
            return Core.TXT.ItemStatCostTXT[(ushort)stat.Id];
        }
        else
        {
            return Core.TXT.ItemStatCostTXT[stat.Stat];
        }
    }

}

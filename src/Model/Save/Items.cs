using D2SLib.IO;
using D2SLib.Model.Data;
using System.Text;
using System.Text.Json.Serialization;

namespace D2SLib.Model.Save;

public enum ItemMode : byte
{
    Stored = 0x0,
    Equipped = 0x1,
    Belt = 0x2,
    Buffer = 0x4,
    Socket = 0x6,
}

public enum ItemLocation : byte
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

public enum ItemQuality : byte
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

public sealed class ItemList : IDisposable
{
    private ItemList(ushort header, ushort count)
    {
        Header = header;
        Count = count;
        Items = new List<Item>(count);
    }

    public ushort? Header { get; set; }
    public ushort Count { get; set; }
    public List<Item> Items { get; }

    public void Write(IBitWriter writer, uint version)
    {
        writer.WriteUInt16(Header ?? 0x4D4A);
        writer.WriteUInt16(Count);
        for (int i = 0; i < Count; i++)
        {
            Items[i].Write(writer, version);
        }
    }

    public static ItemList Read(IBitReader reader, uint version)
    {
        var itemList = new ItemList(
            header: reader.ReadUInt16(),
            count: reader.ReadUInt16()
        );
        for (int i = 0; i < itemList.Count; i++)
        {
            itemList.Items.Add(Item.Read(reader, version));
        }
        return itemList;
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(ItemList itemList, uint version)
    {
        using var writer = new BitWriter();
        itemList.Write(writer, version);
        return writer.ToArray();
    }

    public void Dispose()
    {
        foreach (var item in Items)
        {
            item?.Dispose();
        }
        Items.Clear();
    }
}

public sealed class Item : IDisposable
{
    private InternalBitArray _flags = new(4);

    public ushort? Header { get; set; }

    [JsonIgnore]
    public IList<bool> Flags
    {
        get => _flags;
        set
        {
            if (value is InternalBitArray flags)
            {
                _flags?.Dispose();
                _flags = flags;
            }
            else
            {
                throw new ArgumentException("Flags were not of expected type.");
            }
        }
    }

    public string? Version { get; set; }
    public ItemMode Mode { get; set; }
    public ItemLocation Location { get; set; }
    public byte X { get; set; }
    public byte Y { get; set; }
    public byte Page { get; set; }
    public byte EarLevel { get; set; }
    public string PlayerName { get; set; } = string.Empty; //used for personalized or ears
    public string Code { get; set; } = string.Empty;
    public byte NumberOfSocketedItems { get; set; }
    public byte TotalNumberOfSockets { get; set; }
    public List<Item> SocketedItems { get; set; } = new();
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
    public List<ItemStatList> StatLists { get; } = new List<ItemStatList>();
    public bool IsIdentified { get => _flags[4]; set => _flags[4] = value; }
    public bool IsSocketed { get => _flags[11]; set => _flags[11] = value; }
    public bool IsNew { get => _flags[13]; set => _flags[13] = value; }
    public bool IsEar { get => _flags[16]; set => _flags[16] = value; }
    public bool IsStarterItem { get => _flags[17]; set => _flags[17] = value; }
    public bool IsCompact { get => _flags[21]; set => _flags[21] = value; }
    public bool IsEthereal { get => _flags[22]; set => _flags[22] = value; }
    public bool IsPersonalized { get => _flags[24]; set => _flags[24] = value; }
    public bool IsRuneword { get => _flags[26]; set => _flags[26] = value; }

    public void Write(IBitWriter writer, uint version)
    {
        if (version <= 0x60)
        {
            writer.WriteUInt16(Header ?? 0x4D4A);
        }
        WriteCompact(writer, this, version);
        if (!IsCompact)
        {
            WriteComplete(writer, this, version);
        }
        writer.Align();
        for (int i = 0; i < NumberOfSocketedItems; i++)
        {
            SocketedItems[i].Write(writer, version);
        }
    }

    public static Item Read(ReadOnlySpan<byte> bytes, uint version)
    {
        using var reader = new BitReader(bytes);
        return Read(reader, version);
    }

    public static Item Read(IBitReader reader, uint version)
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
        using var writer = new BitWriter();
        item.Write(writer, version);
        return writer.ToArray();
    }

    private static string ReadPlayerName(IBitReader reader)
    {
        Span<char> name = stackalloc char[15];
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

    private static void WritePlayerName(IBitWriter writer, string name)
    {
        var nameChars = name.AsSpan().TrimEnd('\0');
        Span<byte> bytes = stackalloc byte[nameChars.Length];
        int byteCount = Encoding.ASCII.GetBytes(nameChars, bytes);
        bytes = bytes[..byteCount];
        for (int i = 0; i < bytes.Length; i++)
        {
            writer.WriteByte(bytes[i], 7);
        }
        writer.WriteByte((byte)'\0', 7);
    }

    private static void ReadCompact(IBitReader reader, Item item, uint version)
    {
        Span<byte> bytes = stackalloc byte[4];
        reader.ReadBytes(bytes);
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
            item.Code = string.Empty;
            if (version <= 0x60)
            {
                item.Code = reader.ReadString(4);
            }
            else if (version >= 0x61)
            {
                for (int i = 0; i < 4; i++)
                {
                    item.Code += Core.MetaData.ItemsData.ItemCodeTree.DecodeChar(reader);
                }
            }
            item.NumberOfSocketedItems = reader.ReadByte(item.IsCompact ? 1 : 3);
        }
    }

    private static void WriteCompact(IBitWriter writer, Item item, uint version)
    {
        if (item.Flags is not InternalBitArray flags)
        {
            flags = new InternalBitArray(32)
            {
                [04] = item.IsIdentified,
                [11] = item.IsSocketed,
                [13] = item.IsNew,
                [16] = item.IsEar,
                [17] = item.IsStarterItem,
                [21] = item.IsCompact,
                [22] = item.IsEthereal,
                [24] = item.IsPersonalized,
                [26] = item.IsRuneword
            };
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
            var itemCode = item.Code.PadRight(4, ' ');
            Span<byte> code = stackalloc byte[itemCode.Length];
            Encoding.ASCII.GetBytes(itemCode, code);
            if (version <= 0x60)
            {
                writer.WriteBytes(code);
            }
            else if (version >= 0x61)
            {
                var codeTree = Core.MetaData.ItemsData.ItemCodeTree;
                for (int i = 0; i < 4; i++)
                {
                    using var bits = codeTree.EncodeChar((char)code[i]);
                    foreach (bool bit in bits)
                    {
                        writer.WriteBit(bit);
                    }
                }
            }
            writer.WriteByte(item.NumberOfSocketedItems, item.IsCompact ? 1 : 3);
        }
    }

    private static void ReadComplete(IBitReader reader, Item item, uint version)
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
        var trimmedCode = item.Code.AsSpan().TrimEnd();
        if (trimmedCode.SequenceEqual("tbk") || trimmedCode.SequenceEqual("ibk"))
        {
            item.MagicSuffixIds[0] = reader.ReadByte(5);
        }
        item.HasRealmData = reader.ReadBit();
        if (item.HasRealmData)
        {
            //reader.ReadBits(96);
            reader.AdvanceBits(96);
        }
        var itemStatCost = Core.MetaData.ItemStatCostData;
        var row = Core.MetaData.ItemsData.GetByCode(item.Code);
        bool isArmor = Core.MetaData.ItemsData.IsArmor(item.Code);
        bool isWeapon = Core.MetaData.ItemsData.IsWeapon(item.Code);
        bool isStackable = row?["stackable"].ToBool() ?? false;
        if (isArmor)
        {
            item.Armor = (ushort)(reader.ReadUInt16(11) + itemStatCost.GetByStat("armorclass")?["Save Add"].ToUInt16() ?? 0);
        }
        if (isArmor || isWeapon)
        {
            var maxDurabilityStat = itemStatCost.GetByStat("maxdurability");
            var durabilityStat = itemStatCost.GetByStat("maxdurability");
            item.MaxDurability = (ushort)(reader.ReadUInt16(maxDurabilityStat?["Save Bits"].ToInt32() ?? 0) + maxDurabilityStat?["Save Add"].ToUInt16() ?? 0);
            if (item.MaxDurability > 0)
            {
                item.Durability = (ushort)(reader.ReadUInt16(durabilityStat?["Save Bits"].ToInt32() ?? 0) + durabilityStat?["Save Add"].ToUInt16() ?? 0);
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

    private static void WriteComplete(IBitWriter writer, Item item, uint version)
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
        var trimmedCode = item.Code.AsSpan().Trim();
        if (trimmedCode.SequenceEqual("tbk") || trimmedCode.SequenceEqual("ibk"))
        {
            writer.WriteUInt16(item.MagicSuffixIds[0], 5);
        }
        writer.WriteBit(item.HasRealmData);
        if (item.HasRealmData)
        {
            //todo 96 bits
        }
        var itemStatCost = Core.MetaData.ItemStatCostData;
        var row = Core.MetaData.ItemsData.GetByCode(item.Code);
        bool isArmor = Core.MetaData.ItemsData.IsArmor(item.Code);
        bool isWeapon = Core.MetaData.ItemsData.IsWeapon(item.Code);
        bool isStackable = row?["stackable"].ToBool() ?? false;
        if (isArmor)
        {
            writer.WriteUInt16((ushort)(item.Armor - itemStatCost.GetByStat("armorclass")?["Save Add"].ToUInt16() ?? 0), 11);
        }
        if (isArmor || isWeapon)
        {
            var maxDurabilityStat = itemStatCost.GetByStat("maxdurability");
            var durabilityStat = itemStatCost.GetByStat("maxdurability");
            writer.WriteUInt16((ushort)(item.MaxDurability - maxDurabilityStat?["Save Add"].ToUInt16() ?? 0), maxDurabilityStat?["Save Bits"].ToInt32() ?? 0);
            if (item.MaxDurability > 0)
            {
                writer.WriteUInt16((ushort)(item.Durability - durabilityStat?["Save Add"].ToUInt16() ?? 0), durabilityStat?["Save Bits"].ToInt32() ?? 0);
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

    public void Dispose()
    {
        Interlocked.Exchange(ref _flags!, null)?.Dispose();
        foreach (var item in SocketedItems)
        {
            item?.Dispose();
        }
        SocketedItems.Clear();
    }
}

public class ItemStatList
{
    private const ushort magicmindam = 52;
    private const ushort item_maxdamage_percent = 17;
    private const ushort firemindam = 48;
    private const ushort lightmindam = 50;
    private const ushort coldmindam = 54;
    private const ushort poisonmindam = 57;

    public List<ItemStat> Stats { get; set; } = new();

    public static ItemStatList Read(IBitReader reader)
    {
        var itemStatList = new ItemStatList();
        ushort id = reader.ReadUInt16(9);
        while (id != 0x1ff)
        {
            itemStatList.Stats.Add(ItemStat.Read(reader, id));
            //https://github.com/ThePhrozenKeep/D2MOO/blob/master/source/D2Common/src/Items/Items.cpp#L7332
            if (id is magicmindam or item_maxdamage_percent or firemindam or lightmindam)
            {
                itemStatList.Stats.Add(ItemStat.Read(reader, (ushort)(id + 1)));
            }
            else if (id is coldmindam or poisonmindam)
            {
                itemStatList.Stats.Add(ItemStat.Read(reader, (ushort)(id + 1)));
                itemStatList.Stats.Add(ItemStat.Read(reader, (ushort)(id + 2)));
            }
            id = reader.ReadUInt16(9);
        }
        return itemStatList;
    }

    public static void Write(IBitWriter writer, ItemStatList itemStatList)
    {
        for (int i = 0; i < itemStatList.Stats.Count; i++)
        {
            var stat = itemStatList.Stats[i];
            var property = ItemStat.GetStatRow(stat);
            ushort id = property?["ID"].ToUInt16() ?? 0;
            writer.WriteUInt16(id, 9);
            ItemStat.Write(writer, stat);

            //assume these stats are in order...
            //https://github.com/ThePhrozenKeep/D2MOO/blob/master/source/D2Common/src/Items/Items.cpp#L7332
            if (id is magicmindam or item_maxdamage_percent or firemindam or lightmindam)
            {
                ItemStat.Write(writer, itemStatList.Stats[++i]);
            }
            else if (id is coldmindam or poisonmindam)
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
    public string Stat { get; set; } = string.Empty;
    public int? SkillTab { get; set; }
    public int? SkillId { get; set; }
    public int? SkillLevel { get; set; }
    public int? MaxCharges { get; set; }
    public int? Param { get; set; }
    public int Value { get; set; }

    public static ItemStat Read(IBitReader reader, ushort id)
    {
        var itemStat = new ItemStat();
        var property = Core.MetaData.ItemStatCostData.GetById(id);
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

    public static void Write(IBitWriter writer, ItemStat stat)
    {
        var property = GetStatRow(stat);
        if (property is null)
        {
            throw new ArgumentException($"No ItemStatCost record found for id: {stat.Id}", nameof(stat));
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

    public static DataRow? GetStatRow(ItemStat stat)
    {
        return stat.Id is ushort statId
            ? Core.MetaData.ItemStatCostData.GetById(statId)
            : Core.MetaData.ItemStatCostData.GetByStat(stat.Stat);
    }
}

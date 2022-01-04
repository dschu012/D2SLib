using D2SLib.Model.Data;

namespace D2SLib;

public sealed class MetaData
{
    public MetaData(ItemStatCostData itemsStatCost, ItemsData itemsData)
    {
        ItemStatCostData = itemsStatCost;
        ItemsData = itemsData;
    }

    public ItemStatCostData ItemStatCostData { get; }
    public ItemsData ItemsData { get; }
}

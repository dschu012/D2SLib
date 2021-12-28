using D2SLib.Model.Data;

namespace D2SLib;

public class MetaData
{
    public ItemStatCostData ItemStatCostData { get; set; }
    private ItemsData? _items = null;
    public ItemsData ItemsData
    {
        get => _items ??= new ItemsData();
        set => _items = value;
    }
}

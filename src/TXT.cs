using D2SLib.Model.TXT;

namespace D2SLib;

public class TXT
{
    public ItemStatCostTXT ItemStatCostTXT { get; set; }
    private ItemsTXT? _items = null;
    public ItemsTXT ItemsTXT
    {
        get => _items ??= new ItemsTXT();
        set => _items = value;
    }
}

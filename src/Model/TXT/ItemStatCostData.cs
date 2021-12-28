namespace D2SLib.Model.Data;

public class ItemStatCostData : DataFile
{
    public DataRow this[int i] => GetByColumnAndValue("ID", i.ToString());
    public DataRow this[string i] => GetByColumnAndValue("Stat", i);

    public static ItemStatCostData Read(Stream data)
    {
        var itemStatCost = new ItemStatCostData();
        itemStatCost.ReadData(data);
        return itemStatCost;
    }

    public static ItemStatCostData Read(string file)
    {
        using Stream stream = File.OpenRead(file);
        return Read(stream);
    }
}

using D2SLib.IO;

namespace D2SLib.Model.Huffman;

internal class Node
{
    public char Symbol { get; set; }
    public int Frequency { get; set; }
    public Node? Right { get; set; }
    public Node? Left { get; set; }

    internal InternalBitArray? Traverse(char symbol, InternalBitArray data)
    {
        if (IsLeaf())
        {
            return symbol.Equals(Symbol) ? data : null;
        }
        else
        {
            if (Left is not null)
            {
                data.Add(false);
                var left = Left.Traverse(symbol, data);
                if (left is null)
                    data.Length--;
                else
                    return data;
            }

            if (Right is not null)
            {
                data.Add(true);
                var right = Right.Traverse(symbol, data);
                if (right is null)
                    data.Length--;
                else
                    return data;
            }

            return null;
        }
    }

    public bool IsLeaf() => Left is null && Right is null;
}

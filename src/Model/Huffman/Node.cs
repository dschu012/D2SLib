namespace D2SLib.Model.Huffman;

public class Node
{
    public char Symbol { get; set; }
    public int Frequency { get; set; }
    public Node? Right { get; set; }
    public Node? Left { get; set; }

    public List<bool>? Traverse(char symbol, List<bool> data)
    {
        // Leaf
        if (Right == null && Left == null)
        {
            if (symbol.Equals(Symbol))
            {
                return data;
            }
            else
            {
                return null;
            }
        }
        else
        {
            List<bool>? left = null;
            List<bool>? right = null;

            if (Left is not null)
            {
                var leftPath = new List<bool>(data) { false };
                left = Left.Traverse(symbol, leftPath);
            }

            if (Right is not null)
            {
                var rightPath = new List<bool>(data) { true };
                right = Right.Traverse(symbol, rightPath);
            }

            if (left != null)
            {
                return left;
            }
            else
            {
                return right;
            }
        }
    }

    public bool IsLeaf() => Left is null && Right is null;
}

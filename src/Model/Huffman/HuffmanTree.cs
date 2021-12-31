using D2SLib.IO;

namespace D2SLib.Model.Huffman;

//hardcoded....
internal class HuffmanTree
{
    public Node? Root { get; set; }

    public static readonly IReadOnlyDictionary<char, string> TABLE = new Dictionary<char, string>(38)
    {
        {'0', "11111011"},
        {' ', "10"},
        {'1', "1111100"},
        {'2', "001100"},
        {'3', "1101101"},
        {'4', "11111010"},
        {'5', "00010110"},
        {'6', "1101111"},
        {'7', "01111"},
        {'8', "000100"},
        {'9', "01110"},
        {'a', "11110"},
        {'b', "0101"},
        {'c', "01000"},
        {'d', "110001"},
        {'e', "110000"},
        {'f', "010011"},
        {'g', "11010"},
        {'h', "00011"},
        {'i', "1111110"},
        {'j', "000101110"},
        {'k', "010010"},
        {'l', "11101"},
        {'m', "01101"},
        {'n', "001101"},
        {'o', "1111111"},
        {'p', "11001"},
        {'q', "11011001"},
        {'r', "11100"},
        {'s', "0010"},
        {'t', "01100"},
        {'u', "00001"},
        {'v', "1101110"},
        {'w', "00000"},
        {'x', "00111"},
        {'y', "0001010"},
        {'z', "11011000"}
    };

    //todo find a way to build this like d2?
    public void Build()
    {
        Root = new Node();
        foreach (var entry in TABLE)
        {
            var current = Root;
            foreach (char bit in entry.Value.AsSpan())
            {
                if (bit == '1')
                {
                    if (current.Right == null)
                    {
                        current.Right = new Node();
                    }
                    current = current.Right;
                }
                else if (bit == '0')
                {
                    if (current.Left == null)
                    {
                        current.Left = new Node();
                    }
                    current = current.Left;
                }
            }
            current.Symbol = entry.Key;
        }
    }

    public InternalBitArray EncodeChar(char source)
    {
        var encodedSymbol = Root?.Traverse(source, new InternalBitArray(0));
        if (encodedSymbol is null)
            throw new InvalidOperationException("Could not encode with an empty tree.");
        return encodedSymbol;
    }

    public char DecodeChar(IBitReader reader)
    {
        var current = Root;
        while (!(current?.IsLeaf() ?? true))
        {
            if (reader.ReadBit())
            {
                if (current.Right is not null)
                {
                    current = current.Right;
                }
            }
            else
            {
                if (current.Left is not null)
                {
                    current = current.Left;
                }
            }
        }
        return current?.Symbol ?? '\0';
    }
}

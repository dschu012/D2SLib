using D2SLib.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace D2SLib.Model.Huffman
{
    //hardcoded....
    public class HuffmanTree
    {
        public Node Root { get; set; }

        public static Dictionary<char, string> TABLE = new Dictionary<char, string>
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
        public void Build(List<string> items)
        {
            Root = new Node();
            foreach(KeyValuePair<char, string> entry in TABLE)
            {
                var Current = Root;
                foreach(char bit in entry.Value)
                {
                    if(bit == '1')
                    {
                        if(Current.Right == null)
                        {
                            Current.Right = new Node();
                        }
                        Current = Current.Right;
                    } else if(bit == '0')
                    {
                        if (Current.Left == null)
                        {
                            Current.Left = new Node();
                        }
                        Current = Current.Left;
                    }
                }
                Current.Symbol = entry.Key;
            }
        }

        public BitArray EncodeChar(char source)
        {
            List<bool> encodedSymbol = this.Root.Traverse(source, new List<bool>());
            return new BitArray(encodedSymbol.ToArray());
        }

        public char DecodeChar(BitReader reader)
        {
            Node current = this.Root;
            while(!current.IsLeaf())
            {
                if (reader.ReadBit())
                {
                    if (current.Right != null)
                    {
                        current = current.Right;
                    }
                }
                else
                {
                    if (current.Left != null)
                    {
                        current = current.Left;
                    }
                }
            }
            return current.Symbol;
        }
    }
}

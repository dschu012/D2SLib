using System.Collections;
using System.Text;

namespace D2SLib.IO;

internal static class BitArrayExtensions
{
    public static string Print(this BitArray bits)
    {
        StringBuilder sb = new(bits.Length * 8 + bits.Length);

        for (int i = 0; i < bits.Length; i++)
        {
            if (i > 0)
            {
                if (i % 32 == 0)
                    sb.AppendLine();
                else if (i % 8 == 0)
                    sb.Append(' ');
            }

            sb.Append(bits[i] ? '1' : '0');
        }

        return sb.ToString();
    }
}

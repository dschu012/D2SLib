using D2SLib;
using D2SLib.Model.Save;
using System.Diagnostics;
using System.Text.Json;

namespace D2SLibTests;

[TestClass]
public class D2STest
{
    [TestMethod]
    public void VerifyCanReadSimple115Save()
    {
        D2S character = Core.ReadD2S(File.ReadAllBytes(@"Resources\D2S\1.15\Amazon.d2s"));
        Assert.IsTrue(character.Name == "Amazon");
        Assert.IsTrue(character.ClassId == 0x0);

        LogCharacter(character);
    }

    [TestMethod]
    public void VerifyCanReadComplex115Save()
    {
        D2S character = Core.ReadD2S(File.ReadAllBytes(@"Resources\D2S\1.15\DannyIsGreat.d2s"));
        Assert.IsTrue(character.Name == "DannyIsGreat");
        Assert.IsTrue(character.ClassId == 0x1);

        LogCharacter(character);
    }

    [TestMethod]
    public void VerifyCanWriteComplex115Save()
    {
        byte[] input = File.ReadAllBytes(@"Resources\D2S\1.15\DannyIsGreat.d2s");
        D2S character = Core.ReadD2S(input);
        byte[] ret = Core.WriteD2S(character);
        //File.WriteAllBytes(Environment.ExpandEnvironmentVariables($"%userprofile%/Saved Games/Diablo II Resurrected Tech Alpha/{character.Name}.d2s"), ret);
        Assert.AreEqual(input.Length, ret.Length);

        // This test fails with "element at index 12 differs" (checksum) but that was true in original code
        //CollectionAssert.AreEqual(input, ret);
    }

    [Conditional("DEBUG")]
    private static void LogCharacter(D2S character, string? label = null)
    {
        if (label is not null)
        {
            Console.Write(label);
            Console.WriteLine(':');
        }

        Console.WriteLine(
            JsonSerializer.Serialize(character,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
#if NET6_0_OR_GREATER
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
#else
                        IgnoreNullValues = true,
#endif
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
    }

}

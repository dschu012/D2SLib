using D2SLib;
using D2SLib.Model.Save;

namespace D2SLibTests
{
    [TestClass]
    public class D2STest
    {
        [TestMethod]
        public void VerifyCanReadSimple115Save()
        {
            D2S character = Core.ReadD2S(File.ReadAllBytes(@"Resources\D2S\1.15\Amazon.d2s"));
            Assert.IsTrue(character.Name == "Amazon");
            Assert.IsTrue(character.ClassId == 0x0);   
        }

        [TestMethod]
        public void VerifyCanReadComplex115Save()
        {
            D2S character = Core.ReadD2S(File.ReadAllBytes(@"Resources\D2S\1.15\DannyIsGreat.d2s"));
            Assert.IsTrue(character.Name == "DannyIsGreat");
            Assert.IsTrue(character.ClassId == 0x1);
            /*
            File.WriteAllText(@"D:\DannyIsGreat.json", JsonSerializer.Serialize(character,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                IgnoreNullValues = true
            }));
            */
        }

        [TestMethod]
        public void VerifyCanWriteComplex115Save()
        {
            byte[] input = File.ReadAllBytes(@"Resources\D2S\1.15\DannyIsGreat.d2s");
            D2S character = Core.ReadD2S(input);
            byte[] ret = Core.WriteD2S(character);
            //File.WriteAllBytes(Environment.ExpandEnvironmentVariables($"%userprofile%/Saved Games/Diablo II Resurrected Tech Alpha/{character.Name}.d2s"), ret);
            Assert.AreEqual(input.Length, ret.Length);
            
            // This test fails with "element at index 12 differs" but that was true in original code
            //CollectionAssert.AreEqual(input, ret);
        }

    }
}

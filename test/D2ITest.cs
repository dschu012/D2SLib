using D2SLib;
using D2SLib.Model.Save;

namespace D2SLibTests
{
    [TestClass]
    public class D2ITest
    {

        [TestMethod]
        public void VerifyCanReadSharedStash115()
        {
            //0x61 == 1.15
            D2I stash = Core.ReadD2I(File.ReadAllBytes(@"Resources\D2I\1.15\SharedStash_SoftCore.d2i"), 0x61);
            Assert.IsTrue(stash.ItemList.Count == 8);
            Assert.IsTrue(stash.ItemList.Items[0].Code == "rng ");
        }
    }
}

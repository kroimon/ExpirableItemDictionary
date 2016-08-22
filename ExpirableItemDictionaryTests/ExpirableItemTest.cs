using System;
using ExpirableDictionary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpirableDictionaryTests
{
    [TestClass]
    public class ExpirableItemTest
    {
        [TestMethod]
        public void ImplicitCastTest()
        {
            ExpirableItem<string> item = new ExpirableItem<string>("foobar", DateTime.MaxValue);
            string s = item;
            Assert.AreEqual(item.Value, s);
        }
    }
}

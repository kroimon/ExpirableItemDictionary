using System;
using ExpirableDictionary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpirableDictionaryTests
{
    [TestClass]
    public class ExpirableItemTest
    {
        [TestMethod]
        public void ExplicitCastTest()
        {
            ExpirableItem<string> item = new ExpirableItem<string>("foobar", DateTime.MaxValue);
            string s = (string)item;
            Assert.AreEqual(item.Value, s);
        }
    }
}

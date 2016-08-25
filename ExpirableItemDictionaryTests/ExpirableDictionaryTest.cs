using System;
using System.Threading;
using ExpirableDictionary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpirableDictionaryTests
{
    [TestClass]
    public class ExpirableDictionaryTest
    {
        [TestMethod]
        public void DictionaryExpiresStaleItems()
        {
            var dict = new ExpirableItemDictionary<string, object>(TimeSpan.FromMilliseconds(50));
            dict.Add("a", 1);
            dict.Add("b", 2, TimeSpan.FromMilliseconds(10));
            dict.Add("c", 3, DateTime.MaxValue);

            Assert.IsTrue(dict.ContainsKey("a"));
            Assert.IsTrue(dict.ContainsKey("b"));
            Assert.IsTrue(dict.ContainsKey("c"));

            Thread.Sleep(51);

            Assert.IsFalse(dict.ContainsKey("a"));
            Assert.IsFalse(dict.ContainsKey("b"));
            Assert.IsTrue(dict.ContainsKey("c"));
        }

        [TestMethod]
        public void DictionaryRaisesExpirationEvent()
        {
            var dict = new ExpirableItemDictionary<string, object>(TimeSpan.FromMilliseconds(50));
            string key = "a";
            object value = 1;
            dict[key] = value;

            object sender = null;
            string eventKey = null;
            object eventValue = null;
            dict.ItemExpired += (s, e) =>
            {
                sender = s;
                eventKey = e.Key;
                eventValue = e.Value;
            };

            Thread.Sleep(51);
            dict.RemoveExpiredItems();

            Assert.AreSame(sender, dict);
            Assert.AreEqual(eventKey, key);
            Assert.AreEqual(eventValue, value);
        }
    }
}
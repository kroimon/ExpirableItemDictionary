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
            var dictionary = new ExpirableItemDictionary<string, object>();
            dictionary.DefaultTimeToLive = TimeSpan.FromMilliseconds(50);
            dictionary.Add("a", "b");
            Thread.Sleep(51);
            Assert.IsFalse(dictionary.ContainsKey("a"));
        }

        [TestMethod]
        public void DictionaryDoesNotExpiredNonStaleItems()
        {
            var dictionary = new ExpirableItemDictionary<string, object>();
            dictionary.DefaultTimeToLive = TimeSpan.FromMilliseconds(50);
            dictionary.Add("a", "b");
            Assert.IsTrue(dictionary.ContainsKey("a"));
        }

        [TestMethod]
        public void DictionaryRaisesExpirationEvent()
        {
            var dictionary = new ExpirableItemDictionary<string, object>();
            dictionary.DefaultTimeToLive = TimeSpan.FromMilliseconds(50);
            string key = "a";
            object value = "b";
            dictionary[key] = value;
            object sender = null;
            string eventKey = null;
            object eventValue = null;
            dictionary.ItemExpired += (s, e) =>
            {
                sender = s;
                eventKey = e.Key;
                eventValue = e.Value;
            };
            Thread.Sleep(51);
            dictionary.ClearExpiredItems();
            Assert.AreSame(sender, dictionary);
            Assert.AreEqual(eventKey, key);
            Assert.AreEqual(eventValue, value);
        }
    }
}

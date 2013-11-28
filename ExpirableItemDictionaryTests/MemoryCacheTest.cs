using System;
using System.Collections.Specialized;
using System.Runtime.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpirableDictionaryTests
{
    /// <summary>
    /// just observing the new .NET 4.0 memory cache
    /// </summary>
    [TestClass]
    public class MemoryCacheTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var config = new NameValueCollection();
            var cache = new MemoryCache("myMemCache", config);
            cache.Add(new CacheItem("a", "b"),
                      new CacheItemPolicy
                          {
                              Priority = CacheItemPriority.NotRemovable,
                              SlidingExpiration=TimeSpan.FromMilliseconds(50)
                          });
            Assert.IsTrue(cache.Contains("a"));
            Assert.AreEqual("b", cache["a"]);
        }
    }
}

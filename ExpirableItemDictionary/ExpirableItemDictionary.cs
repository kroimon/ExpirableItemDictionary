using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ExpirableDictionary
{
    /// <summary>
    /// Defines a caching table whereby items are configured to
    /// expire and thereby be removed automatically.
    /// </summary>
    /// <typeparam name="K">Key</typeparam>
    /// <typeparam name="T">Value</typeparam>
    public class ExpirableItemDictionary<K, T> : IDictionary<K, T>, IDisposable
    {

        #region Private Fields

        private readonly Dictionary<K, ExpirableItem<T>> innerDictionary;
        private TimeSpan defaultTimeToLive = TimeSpan.FromMinutes(10);
        private TimeSpan autoClearExpiredItemsFrequency = TimeSpan.FromSeconds(15);
        private readonly Timer timer;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpirableItemDictionary&lt;K, T&gt;"/> class.
        /// </summary>
        public ExpirableItemDictionary()
            : this((IEqualityComparer<K>)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpirableItemDictionary&lt;K, T&gt;"/> class.
        /// </summary>
        /// <param name="dictionary">A dictionary of values to pre-load into the cache.</param>
        public ExpirableItemDictionary(IEnumerable<KeyValuePair<K, T>> dictionary)
            : this(dictionary, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpirableItemDictionary&lt;K, T&gt;"/> class.
        /// </summary>
        /// <param name="defaultTimeToLive">The default time-to-live.</param>
        public ExpirableItemDictionary(TimeSpan defaultTimeToLive)
            : this()
        {
            this.defaultTimeToLive = defaultTimeToLive;
        }

        /// <summary>
        /// Initializes a new instance of this type using the specified <paramref name="comparer"/>.
        /// </summary>
        /// <param name="comparer">A comparer implementation. For example,
        /// for case-insensitive keys, use <see cref="StringComparer.InvariantCulture"/>.</param>
        public ExpirableItemDictionary(IEqualityComparer<K> comparer)
        {
            timer = new Timer(e => ClearExpiredItems(), null, autoClearExpiredItemsFrequency, autoClearExpiredItemsFrequency);
            innerDictionary = new Dictionary<K, ExpirableItem<T>>(comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpirableItemDictionary&lt;K, T&gt;"/> class.
        /// </summary>
        /// <param name="dictionary">A dictionary of values to pre-load into the cache.</param>
        /// <param name="comparer">A comparer implementation. For example,
        /// for case-insensitive keys, use <see cref="StringComparer.InvariantCulture"/>.</param>
        public ExpirableItemDictionary(IEnumerable<KeyValuePair<K, T>> dictionary, IEqualityComparer<K> comparer)
            : this(comparer)
        {
            foreach (var kvp in dictionary)
            {
                innerDictionary.Add(kvp.Key, new ExpirableItem<T>(kvp.Value, defaultTimeToLive));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpirableItemDictionary&lt;K, T&gt;"/> class
        /// using the specified <paramref name="comparer"/>.
        /// </summary>
        /// <param name="comparer">A comparer implementation. For example,
        /// for case-insensitive keys, use <see cref="StringComparer.InvariantCulture"/>.</param>
        /// <param name="defaultTimeToLive">The default time-to-live.</param>
        public ExpirableItemDictionary(IEqualityComparer<K> comparer, TimeSpan defaultTimeToLive)
            : this(comparer)
        {
            this.defaultTimeToLive = defaultTimeToLive;
        }


        #endregion

        #region Events

        public event EventHandler<ExpirableItemRemovedEventArgs<K, T>> ItemExpired;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the default time-to-live.
        /// </summary>
        /// <value>The default time to live.</value>
        public TimeSpan DefaultTimeToLive
        {
            get { return defaultTimeToLive; }
            set { defaultTimeToLive = value; }
        }

        /// <summary>
        /// Gets or sets the frequency at which expired items are automatically cleared.
        /// </summary>
        /// <value>The auto clear expired items frequency.</value>
        public TimeSpan AutoClearExpiredItemsFrequency
        {
            get { return autoClearExpiredItemsFrequency; }
            set
            {
                autoClearExpiredItemsFrequency = value;
                timer.Change(value, value);
            }
        }

        /// <summary>
        /// Gets the inner dictionary which exposes the expiration strategy of each item.
        /// </summary>
        /// <value>The expirable items.</value>
        public Dictionary<K, ExpirableItem<T>> ExpirableItems
        {
            get { return innerDictionary; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a new expirable item to the collection.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(K key, T value)
        {
            Add(key, value, defaultTimeToLive);
        }

        /// <summary>
        /// Adds a new expirable item to the collection.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="timeToLive">The time-to-live.</param>
        public void Add(K key, T value, TimeSpan timeToLive)
        {
            Add(key, new ExpirableItem<T>(value, timeToLive));
        }

        /// <summary>
        /// Adds a new expirable item to the collection.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expires">The explicit date/time to expire the added item.</param>
        public void Add(K key, T value, DateTime expires)
        {
            Add(key, new ExpirableItem<T>(value, expires));
        }

        /// <summary>
        /// Adds a new expirable item to the collection.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(KeyValuePair<K, T> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Adds a new expirable item to the collection.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(KeyValuePair<K, ExpirableItem<T>> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Adds a new expirable item to the collection.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(K key, ExpirableItem<T> value)
        {
            lock (innerDictionary)
            {
                innerDictionary.Add(key, value);
            }
        }

        /// <summary>
        /// Determines whether the dictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// 	<c>true</c> if the dictionary contains the specified key and the item has not expired; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>This method will auto-clear expired items.</remarks>
        public bool ContainsKey(K key)
        {
            lock (innerDictionary)
            {
                if (innerDictionary.ContainsKey(key))
                {
                    if (innerDictionary[key].HasExpired)
                    {
                        ItemExpired?.Invoke(this, new ExpirableItemRemovedEventArgs<K, T>(key, innerDictionary[key].Value));

                        innerDictionary.Remove(key);
                        return false;
                    }
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets the keys of the collection for all items that have not yet expired.
        /// </summary>
        /// <value>The keys.</value>
        public ICollection<K> Keys
        {
            get
            {
                lock (innerDictionary)
                {
                    ClearExpiredItems();
                    return innerDictionary.Keys;
                }
            }
        }

        /// <summary>
        /// Removes the item having the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool Remove(K key)
        {
            lock (innerDictionary)
            {
                if (ContainsKey(key))
                {
                    innerDictionary.Remove(key);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Tries to the get item having the specified key. Returns <c>true</c> if
        /// the item exists and has not expired.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue(K key, out T value)
        {
            lock (innerDictionary)
            {
                if (ContainsKey(key))
                {
                    value = innerDictionary[key].Value;
                    return true;
                }
                value = default(T);
                return false;
            }
        }

        /// <summary>
        /// Tries to the get item having the specified key. Returns <c>true</c> if
        /// the item exists and has not expired. Updates the item's time-to-live.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="timeToLive">The new time-to-live.</param>
        /// <returns></returns>
        public bool TryGetValueAndUpdate(K key, out T value, TimeSpan timeToLive)
        {
            lock (innerDictionary)
            {
                if (ContainsKey(key))
                {
                    var item = innerDictionary[key];
                    value = item.Value;
                    item.TimeToLive = timeToLive;
                    return true;
                }
                value = default(T);
                return false;
            }
        }

        /// <summary>
        /// Gets all of the values in the dictionary, without any key mappings.
        /// </summary>
        /// <value>The values.</value>
        public ICollection<T> Values
        {
            get
            {
                return this.Cast<T>().ToList();
            }
        }

        /// <summary>
        /// Gets or sets the <typeparamref name="T"/> value with the specified key.
        /// </summary>
        /// <value></value>
        public T this[K key]
        {
            get
            {
                lock (innerDictionary)
                {
                    return ContainsKey(key) ? innerDictionary[key].Value : default(T);
                }
            }
            set
            {
                lock (innerDictionary)
                {
                    innerDictionary[key] = new ExpirableItem<T>(value, defaultTimeToLive);
                }
            }
        }

        /// <summary>
        /// Sets the <typeparamref name="T"/> value with the specified key and time-to-live.
        /// </summary>
        /// <value></value>
        public T this[K key, TimeSpan timeToLive]
        {
            set
            {
                lock (innerDictionary)
                {
                    innerDictionary[key] = new ExpirableItem<T>(value, timeToLive);
                }
            }
        }

        /// <summary>
        /// Sets the <typeparamref name="T"/> value with the specified key an explicit expiration date/time.
        /// </summary>
        /// <value></value>
        public T this[K key, DateTime expires]
        {
            set
            {
                lock (innerDictionary)
                {
                    innerDictionary[key] = new ExpirableItem<T>(value, expires);
                }
            }
        }

        /// <summary>
        /// Removes all items from the internal dictionary.
        /// </summary>
        public void Clear()
        {
            lock (innerDictionary)
            {
                innerDictionary.Clear();
            }
        }

        bool ICollection<KeyValuePair<K, T>>.Contains(KeyValuePair<K, T> item)
        {
            lock (innerDictionary)
            {
                return ContainsKey(item.Key) &&
                       (object)innerDictionary[item.Key].Value
                       == (object)item.Value;
            }
        }

        void ICollection<KeyValuePair<K, T>>.CopyTo(KeyValuePair<K, T>[] array, int arrayIndex)
        {
            // if you need it, implement it
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the number of non-expired cache items.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                lock (innerDictionary)
                {
                    ClearExpiredItems();
                    return innerDictionary.Count;
                }
            }
        }

        bool ICollection<KeyValuePair<K, T>>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<KeyValuePair<K, T>>.Remove(KeyValuePair<K, T> item)
        {
            lock (innerDictionary)
            {
                if (ContainsKey(item.Key))
                {
                    innerDictionary.Remove(item.Key);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Manual invocation clears all items that have expired.
        /// This method is not required for expiration but can be
        /// used for tuning application performance and memory,
        /// somewhat similar to GC.Collect(). 
        /// </summary>
        public void ClearExpiredItems()
        {
            lock (innerDictionary)
            {
                List<KeyValuePair<K, ExpirableItem<T>>> removeList
                    = innerDictionary.Where(kvp => kvp.Value.HasExpired).ToList();

                removeList.ForEach(kvp =>
                {
                    ItemExpired?.Invoke(this, new ExpirableItemRemovedEventArgs<K, T>(kvp.Key, kvp.Value.Value));
                    innerDictionary.Remove(kvp.Key);
                });
            }
        }

        /// <summary>
        /// Returns a *cloned* dictionary 
        /// (will not throw an exception on MoveNext if an item expires).
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<K, T>> GetEnumerator()
        {
            lock (innerDictionary)
            {
                ClearExpiredItems();
                var ret = innerDictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);
                return ret.GetEnumerator();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Resets the time-to-live for the specified item if the item exists in the dictionary,
        /// and then cleans out expired items.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeToLive"></param>
        public void Update(K key, TimeSpan timeToLive)
        {
            lock (innerDictionary)
            {
                if (ContainsKey(key))
                    innerDictionary[key].TimeToLive = timeToLive;
                ClearExpiredItems();
            }
        }

        /// <summary>
        /// Disposes of resources such as the auto-clearing timer.
        /// </summary>
        public void Dispose()
        {
            timer.Dispose();
        }

        #endregion
    }
}

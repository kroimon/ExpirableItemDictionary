using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpirableDictionary
{
    /// <summary>
    ///     Defines a caching table whereby items are configured to
    ///     expire and thereby be removed automatically.
    /// </summary>
    /// <typeparam name="TKey">Type of dictionary keys</typeparam>
    /// <typeparam name="TValue">Type of dictionary values</typeparam>
    public class ExpirableItemDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        #region Private Fields

        private readonly Dictionary<TKey, ExpirableItem<TValue>> innerDictionary;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExpirableItemDictionary&lt;K, T&gt;" /> class.
        /// </summary>
        public ExpirableItemDictionary()
            : this((IEqualityComparer<TKey>)null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExpirableItemDictionary&lt;K, T&gt;" /> class.
        /// </summary>
        /// <param name="dictionary">A dictionary of values to pre-load into the cache.</param>
        public ExpirableItemDictionary(IEnumerable<KeyValuePair<TKey, TValue>> dictionary)
            : this(dictionary, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExpirableItemDictionary&lt;K, T&gt;" /> class.
        /// </summary>
        /// <param name="defaultTimeToLive">The default time-to-live.</param>
        public ExpirableItemDictionary(TimeSpan defaultTimeToLive)
            : this()
        {
            DefaultTimeToLive = defaultTimeToLive;
        }

        /// <summary>
        ///     Initializes a new instance of this type using the specified <paramref name="comparer" />.
        /// </summary>
        /// <param name="comparer">
        ///     A comparer implementation. For example,
        ///     for case-insensitive keys, use <see cref="StringComparer.InvariantCulture" />.
        /// </param>
        public ExpirableItemDictionary(IEqualityComparer<TKey> comparer)
        {
            innerDictionary = new Dictionary<TKey, ExpirableItem<TValue>>(comparer);
            DefaultTimeToLive = TimeSpan.FromMinutes(10);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExpirableItemDictionary&lt;K, T&gt;" /> class.
        /// </summary>
        /// <param name="dictionary">A dictionary of values to pre-load into the cache.</param>
        /// <param name="comparer">
        ///     A comparer implementation. For example,
        ///     for case-insensitive keys, use <see cref="StringComparer.InvariantCulture" />.
        /// </param>
        public ExpirableItemDictionary(IEnumerable<KeyValuePair<TKey, TValue>> dictionary,
            IEqualityComparer<TKey> comparer)
            : this(comparer)
        {
            foreach (var kvp in dictionary)
            {
                innerDictionary.Add(kvp.Key, new ExpirableItem<TValue>(kvp.Value, DefaultTimeToLive));
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExpirableItemDictionary&lt;K, T&gt;" /> class
        ///     using the specified <paramref name="comparer" />.
        /// </summary>
        /// <param name="comparer">
        ///     A comparer implementation. For example,
        ///     for case-insensitive keys, use <see cref="StringComparer.InvariantCulture" />.
        /// </param>
        /// <param name="defaultTimeToLive">The default time-to-live.</param>
        public ExpirableItemDictionary(IEqualityComparer<TKey> comparer, TimeSpan defaultTimeToLive)
            : this(comparer)
        {
            DefaultTimeToLive = defaultTimeToLive;
        }

        #endregion

        #region Events

        public event EventHandler<ExpirableItemRemovedEventArgs<TKey, TValue>> ItemExpired;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the default time-to-live for newly added items.
        /// </summary>
        public TimeSpan DefaultTimeToLive { get; set; }

        /// <summary>
        /// Gets the inner dictionary which exposes the expiration strategy of each item.
        /// </summary>
        public Dictionary<TKey, ExpirableItem<TValue>> ExpirableItems
        {
            get { return innerDictionary; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Adds a new expirable item to the collection using the default time to live.
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            Add(key, value, DefaultTimeToLive);
        }

        /// <summary>
        ///     Adds a new expirable item to the collection, specifying the time to live.
        /// </summary>
        public void Add(TKey key, TValue value, TimeSpan timeToLive)
        {
            Add(key, new ExpirableItem<TValue>(value, timeToLive));
        }

        /// <summary>
        ///     Adds a new expirable item to the collection, specifying an explicit expiration time.
        /// </summary>
        public void Add(TKey key, TValue value, DateTime expires)
        {
            Add(key, new ExpirableItem<TValue>(value, expires));
        }

        /// <summary>
        ///     Adds a new expirable item to the collection using the default time to live.
        /// </summary>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        ///     Adds a new expirable item to the collection.
        /// </summary>
        public void Add(KeyValuePair<TKey, ExpirableItem<TValue>> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        ///     Adds a new expirable item to the collection.
        /// </summary>
        public void Add(TKey key, ExpirableItem<TValue> value)
        {
            innerDictionary.Add(key, value);
        }

        /// <summary>
        ///     Determines whether the dictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///     <c>true</c> if the dictionary contains the specified key and the item has not expired; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>This method will auto-clear expired items.</remarks>
        public bool ContainsKey(TKey key)
        {
            TValue value;
            return TryGetValue(key, out value);
        }

        /// <summary>
        ///     Gets the keys of the collection for all items that have not yet expired.
        /// </summary>
        public ICollection<TKey> Keys
        {
            get
            {
                ClearExpiredItems();
                return innerDictionary.Keys;
            }
        }

        /// <summary>
        ///     Removes the item having the specified key.
        /// </summary>
        public bool Remove(TKey key)
        {
            return innerDictionary.Remove(key);
        }

        /// <summary>
        ///     Tries to the get item having the specified key. Returns <c>true</c> if the item exists and has not expired.
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            ExpirableItem<TValue> item;

            if (innerDictionary.TryGetValue(key, out item))
            {
                if (item.HasExpired)
                {
                    ItemExpired?.Invoke(this, new ExpirableItemRemovedEventArgs<TKey, TValue>(key, item.Value));
                    innerDictionary.Remove(key);

                    value = default(TValue);
                    return false;
                }

                value = item.Value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        ///     Tries to the get item having the specified key. Returns <c>true</c> if the item exists and has not expired. Updates
        ///     the item's time-to-live.
        /// </summary>
        public bool TryGetValueAndUpdate(TKey key, out TValue value, TimeSpan timeToLive)
        {
            ExpirableItem<TValue> item;

            if (innerDictionary.TryGetValue(key, out item))
            {
                item.TimeToLive = timeToLive;
                value = item.Value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        ///     Tries to the get item having the specified key. Returns <c>true</c> if the item exists and has not expired. Updates
        ///     the item's expiration time.
        /// </summary>
        public bool TryGetValueAndUpdate(TKey key, out TValue value, DateTime expires)
        {
            ExpirableItem<TValue> item;

            if (innerDictionary.TryGetValue(key, out item))
            {
                item.Expires = expires;
                value = item.Value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        ///     Gets all of the values in the dictionary, without any key mappings.
        /// </summary>
        public ICollection<TValue> Values
        {
            get { return this.Cast<TValue>().ToList(); }
        }

        /// <summary>
        ///     Gets or sets the <typeparamref name="TValue" /> value with the specified key.
        /// </summary>
        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (!TryGetValue(key, out value))
                    throw new KeyNotFoundException();
                return value;
            }
            set { innerDictionary[key] = new ExpirableItem<TValue>(value, DefaultTimeToLive); }
        }

        /// <summary>
        ///     Sets the <typeparamref name="TValue" /> value with the specified key and time-to-live.
        /// </summary>
        /// <value></value>
        public TValue this[TKey key, TimeSpan timeToLive]
        {
            set { innerDictionary[key] = new ExpirableItem<TValue>(value, timeToLive); }
        }

        /// <summary>
        ///     Sets the <typeparamref name="TValue" /> value with the specified key an explicit expiration date/time.
        /// </summary>
        /// <value></value>
        public TValue this[TKey key, DateTime expires]
        {
            set { innerDictionary[key] = new ExpirableItem<TValue>(value, expires); }
        }

        /// <summary>
        ///     Removes all items from the internal dictionary.
        /// </summary>
        public void Clear()
        {
            innerDictionary.Clear();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            EqualityComparer<TValue> c = EqualityComparer<TValue>.Default;
            return ContainsKey(item.Key) && c.Equals(innerDictionary[item.Key].Value, item.Value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            // if you need it, implement it
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Gets the number of non-expired cache items.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                ClearExpiredItems();
                return innerDictionary.Count;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        /// <summary>
        ///     Removes all expired items from the dictionary.
        /// </summary>
        public void ClearExpiredItems()
        {
            var removeList = innerDictionary.Where(kvp => kvp.Value.HasExpired).ToList();

            foreach (var kvp in removeList)
            {
                ItemExpired?.Invoke(this, new ExpirableItemRemovedEventArgs<TKey, TValue>(kvp.Key, kvp.Value.Value));
                innerDictionary.Remove(kvp.Key);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            ClearExpiredItems();
            var ret = innerDictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);
            return ret.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Resets the time-to-live for the specified item if the item exists in the dictionary.
        ///     Expired items will be removed afterwards.
        /// </summary>
        public void Update(TKey key, TimeSpan timeToLive)
        {
            ExpirableItem<TValue> item;
            if (innerDictionary.TryGetValue(key, out item))
            {
                item.TimeToLive = timeToLive;
            }
            ClearExpiredItems();
        }

        /// <summary>
        ///     Resets the expiration time for the specified item if the item exists in the dictionary.
        ///     Expired items will be removed afterwards.
        /// </summary>
        public void Update(TKey key, DateTime expires)
        {
            ExpirableItem<TValue> item;
            if (innerDictionary.TryGetValue(key, out item))
            {
                item.Expires = expires;
            }
            ClearExpiredItems();
        }

        #endregion
    }
}
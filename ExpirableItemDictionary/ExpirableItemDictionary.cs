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

        public ExpirableItemDictionary(TimeSpan defaultTimeToLive)
            : this(defaultTimeToLive, 0, null)
        {
        }

        public ExpirableItemDictionary(TimeSpan defaultTimeToLive, int capacity)
            : this(defaultTimeToLive, capacity, null)
        {
        }

        public ExpirableItemDictionary(TimeSpan defaultTimeToLive, int capacity, IEqualityComparer<TKey> comparer)
        {
            DefaultTimeToLive = defaultTimeToLive;
            innerDictionary = new Dictionary<TKey, ExpirableItem<TValue>>(capacity, comparer);
        }

        public ExpirableItemDictionary(TimeSpan defaultTimeToLive, IDictionary<TKey, TValue> dictionary)
            : this(defaultTimeToLive, dictionary.Count, null)
        {
            Add(dictionary);
        }

        public ExpirableItemDictionary(TimeSpan defaultTimeToLive, IDictionary<TKey, TValue> dictionary,
            IEqualityComparer<TKey> comparer)
            : this(defaultTimeToLive, dictionary.Count, comparer)
        {
            Add(dictionary);
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
        ///     Adds all pairs from the given dictionary.
        /// </summary>
        public void Add(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            foreach (var kvp in items)
            {
                Add(kvp);
            }
        }

        /// <summary>
        ///     Adds a new expirable item to the collection.
        /// </summary>
        protected void Add(TKey key, ExpirableItem<TValue> value)
        {
            innerDictionary.Add(key, value);
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
            RemoveExpiredItems();
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
            RemoveExpiredItems();
        }

        /// <summary>
        ///     Removes the item having the specified key.
        /// </summary>
        public bool Remove(TKey key)
        {
            return innerDictionary.Remove(key);
        }

        /// <summary>
        ///     Removes all expired items from the dictionary.
        /// </summary>
        public void RemoveExpiredItems()
        {
            var removeList = innerDictionary.Where(kvp => kvp.Value.HasExpired).ToList();

            foreach (var kvp in removeList)
            {
                ItemExpired?.Invoke(this, new ExpirableItemRemovedEventArgs<TKey, TValue>(kvp.Key, kvp.Value.Value));
                innerDictionary.Remove(kvp.Key);
            }
        }

        /// <summary>
        ///     Removes all items from the internal dictionary.
        /// </summary>
        public void Clear()
        {
            innerDictionary.Clear();
        }

        /// <summary>
        ///     Gets the number of non-expired cache items.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                RemoveExpiredItems();
                return innerDictionary.Count;
            }
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
                RemoveExpiredItems();
                return innerDictionary.Keys;
            }
        }

        /// <summary>
        ///     Gets all of the values in the dictionary, without any key mappings.
        /// </summary>
        public ICollection<TValue> Values
        {
            get
            {
                RemoveExpiredItems();
                return innerDictionary.Values.Select(item => item.Value).ToList();
            }
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

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            EqualityComparer<TValue> c = EqualityComparer<TValue>.Default;
            return ContainsKey(item.Key) && c.Equals(innerDictionary[item.Key].Value, item.Value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            RemoveExpiredItems();
            innerDictionary.Select(kvp => new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value.Value))
                .ToArray()
                .CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            RemoveExpiredItems();
            return
                innerDictionary.Select(kvp => new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value.Value)).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
using System;

namespace ExpirableDictionary
{
    /// <summary>
    ///     Contains the key/value pair of the item that expired and has been removed from the dictionary.
    /// </summary>
    /// <typeparam name="K">Type of dictionary keys</typeparam>
    /// <typeparam name="T">Type of dictionary values</typeparam>
    public class ExpirableItemRemovedEventArgs<K, T> : EventArgs
    {
        public K Key { get; set; }
        public T Value { get; set; }

        public ExpirableItemRemovedEventArgs(K key, T value)
        {
            Key = key;
            Value = value;
        }
    }
}
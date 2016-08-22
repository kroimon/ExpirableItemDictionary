using System;

namespace ExpirableDictionary
{
    public class ExpirableItem<T>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ExpirableItem&lt;T&gt;" /> class, populating it with the specified
        ///     value and an explicit expiration date/time.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="expires">The expiration time.</param>
        public ExpirableItem(T value, DateTime expires)
        {
            Value = value;
            Expires = expires;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExpirableItem&lt;T&gt;" /> class, populating it with the specified
        ///     value and time-to-live.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="timeToLive">The time-to-live.</param>
        public ExpirableItem(T value, TimeSpan timeToLive)
        {
            Value = value;
            TimeToLive = timeToLive;
        }

        /// <summary>
        ///     Gets or sets the item's value.
        /// </summary>
        /// <value>The item's value.</value>
        public T Value { get; set; }

        /// <summary>
        ///     Gets or sets the item's expiration date/time.
        /// </summary>
        /// <value>The expiration item's date/time.</value>
        public DateTime Expires { get; set; }

        /// <summary>
        ///     Gets or sets the item's time-to-live.
        /// </summary>
        /// <value>The item's time to live.</value>
        public TimeSpan TimeToLive
        {
            get { return Expires - DateTime.Now; }
            set
            {
                if (value == TimeSpan.MaxValue)
                {
                    Expires = DateTime.MaxValue;
                }
                else
                {
                    Expires = DateTime.Now + value;
                }
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this item has expired.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this item has expired; otherwise, <c>false</c>.
        /// </value>
        public bool HasExpired
        {
            get { return DateTime.Now > Expires; }
        }
    }
}
using System;

namespace ExpirableDictionary
{
    public class ExpirableItem<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpirableItem&lt;T&gt;"/> class.
        /// The value should be specified in the object initializer.
        /// </summary>
        public ExpirableItem()
        {
            TimeStamp = DateTime.Now;
            try
            {
                TimeToLive = System.Web.HttpContext.Current != null
                                 ? TimeSpan.FromMinutes(System.Web.HttpContext.Current.Session.Timeout)
                                 : TimeSpan.FromMinutes(20);
            }
            catch (Exception)
            {
                TimeToLive = TimeSpan.FromMinutes(20);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpirableItem&lt;T&gt;"/> class,
        /// populating it with the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public ExpirableItem(T value)
            : this()
        {
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpirableItem&lt;T&gt;"/> class,
        /// populating it with the specified value and time-to-live.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="timeToLive">The time-to-live.</param>
        public ExpirableItem(T value, TimeSpan timeToLive)
            : this(value)
        {
            this.TimeToLive = timeToLive;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpirableItem&lt;T&gt;"/> class,
        /// populating it with the specified value and an explicit expiration date/time.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="expires">The expires.</param>
        public ExpirableItem(T value, DateTime expires)
            : this(value)
        {
            this.Expires = expires;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpirableItem&lt;T&gt;"/> class,
        /// populating it with the specified value, timestamp, and time-to-live.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="timeStamp">The time stamp.</param>
        /// <param name="timeToLive">The time-to-live.</param>
        public ExpirableItem(T value, DateTime timeStamp, TimeSpan timeToLive)
            : this(value, timeToLive)
        {
            this.TimeStamp = timeStamp;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpirableItem&lt;T&gt;"/> class,
        /// populating it with the specified value, timestamp, and explicit expiration date/time.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="timeStamp">The time stamp.</param>
        /// <param name="expires">The expires.</param>
        public ExpirableItem(T value, DateTime timeStamp, DateTime expires)
            : this(value)
        {
            this.TimeStamp = timeStamp;
            this.Expires = expires;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public T Value { get; set; }

        /// <summary>
        /// Gets or sets the time stamp.
        /// </summary>
        /// <value>The time stamp.</value>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the time-to-live.
        /// </summary>
        /// <value>The time to live.</value>
        public TimeSpan TimeToLive { get; set; }

        /// <summary>
        /// Gets or sets the explicit expiration date/time. This works by offsetting
        /// the <see cref="TimeSpan"/> with the <see cref="TimeToLive"/>.
        /// </summary>
        /// <value>The expiration date/time.</value>
        public DateTime Expires
        {
            get
            {
                return TimeStamp + TimeToLive;
            }
            set
            {
                TimeToLive = value - TimeStamp;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this item has expired.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this item has expired; otherwise, <c>false</c>.
        /// </value>
        public bool HasExpired
        {
            get
            {
                return DateTime.Now > Expires;
            }
        }
    }
}

namespace Uneti.NET
{
    /// <summary>
    /// Represents an apparatus used for counting.
    /// </summary>
    internal sealed class Counter
    {
        /// <summary>
        /// The current value of the current counter.
        /// </summary>
        private int Value { get; set; }

        /// <summary>
        /// Gets the current value.
        /// </summary>
        /// <returns>The current value.</returns>
        internal int Current()
        {
            return Value;
        }

        /// <summary>
        /// Gets the current value and then increases it by one.
        /// </summary>
        /// <returns>The current value.</returns>
        internal int Next()
        {
            return Value++;
        }
    }
}
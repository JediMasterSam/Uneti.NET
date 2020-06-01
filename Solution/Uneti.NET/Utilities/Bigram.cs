using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Uneti.NET
{
    /// <summary>
    /// Represents a collection of character pairs.
    /// </summary>
    internal sealed class Bigram
    {
        /*
         * LIMITATIONS:
         *    - Character order will not be accounted for.  (i.e. aaba = {aa, ab, ba}, abaa = {ab, ba, aa}, so aaba = abaa)
         *    - Repeating pairs will be lost. (i.e. aa = {aa}, aaaaaa = {aa}, so aa = aaaaaa)
         *
         * WHEN TO USE:
         *     - The text being tokenized is English.
         *     - Most words in the English language do not heavily consist of consecutive repeating characters, so neither limitation will be in effect.
         *
         * WHEN NOT TO USE:
         *     - The text being tokenized is randomly generated.
         *     - There is no guarantee the text will be homogeneous, so there can be false positives.
         */
        
        /// <summary>
        /// A bigram with no character pairs.
        /// </summary>
        internal static readonly Bigram Empty = new Bigram(null);

        /// <summary>
        /// Creates a new instance of <see cref="Bigram"/>.
        /// </summary>
        /// <param name="value">The value to tokenize.</param>
        internal Bigram(string value)
        {
            Tokens = GenerateTokens(value).ToImmutableArray();
        }

        /// <summary>
        /// Is the current bigram empty? (i.e. there are no character pairs in the bigram)
        /// </summary>
        internal bool IsEmpty => Tokens.IsEmpty;

        /// <summary>
        /// The collection of compressed character pairs of the current bigram.
        /// </summary>
        private ImmutableArray<uint> Tokens { get; }

        /// <summary>
        /// Compares the current bigram to the specified bigram using the Sørensen–Dice coefficient.
        /// </summary>
        /// <param name="other">The other bigram.</param>
        /// <returns>A value between 0 and 1, where 1 is a perfect match.</returns>
        internal float CompareTo(Bigram other)
        {
            return Compare(this, other);
        }

        /// <summary>
        /// Compares the specified bigrams using the Sørensen–Dice coefficient.
        /// </summary>
        /// <param name="a">The first bigram.</param>
        /// <param name="b">The second bigram.</param>
        /// <returns>A value between 0 and 1, where 1 is a perfect match.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static float Compare(Bigram a, Bigram b)
        {
            if (a.Tokens.IsEmpty && b.Tokens.IsEmpty) return 1.0f;
            if (a.Tokens.IsEmpty || b.Tokens.IsEmpty) return 0.0f;

            var enumeratorA = a.Tokens.GetEnumerator();
            var enumeratorB = b.Tokens.GetEnumerator();

            var moveNextA = enumeratorA.MoveNext();
            var moveNextB = enumeratorB.MoveNext();

            var count = 0;

            while (moveNextA && moveNextB)
            {
                var comparison = enumeratorA.Current.CompareTo(enumeratorB.Current);

                if (comparison < 0)
                {
                    moveNextA = enumeratorA.MoveNext();
                }
                else if (comparison > 0)
                {
                    moveNextB = enumeratorB.MoveNext();
                }

                else
                {
                    count++;

                    moveNextA = enumeratorA.MoveNext();
                    moveNextB = enumeratorB.MoveNext();
                }
            }

            return (float) count / Math.Max(a.Tokens.Length, b.Tokens.Length);
        }

        /// <summary>
        /// Concatenates the specified numbers. (i.e. 123 and 45 = 12345)
        /// </summary>
        /// <param name="a">The first number.</param>
        /// <param name="b">The second number.</param>
        /// <returns>The concatenation of the specified numbers.</returns>
        private static uint Concatenate(ushort a, ushort b)
        {
            const ushort c0 = 10;
            const ushort c1 = 100;
            const ushort c2 = 1000;

            if (b < c0) return 10u * a + b;
            if (b < c1) return 100u * a + b;
            if (b < c2) return 1000u * a + b;

            return 10000u * a + b;
        }

        /// <summary>
        /// Gets the and compresses the character pairs for the specified value.
        /// </summary>
        /// <param name="value">The value to tokenize.</param>
        /// <returns>A sorted collection of numbers that represents the character pairs of the specified value.</returns>
        private static IEnumerable<uint> GenerateTokens(string value)
        {
            if (string.IsNullOrEmpty(value)) yield break;

            if (value.Length == 1)
            {
                yield return value[0];
            }
            else
            {
                var tokens = new uint[value.Length - 1];

                for (var index = 0; index < tokens.Length; index++)
                {
                    tokens[index] = Concatenate(value[index], value[index + 1]);
                }

                Array.Sort(tokens);

                using var enumerator = ((IEnumerable<uint>) tokens).GetEnumerator();

                enumerator.MoveNext();

                var previous = enumerator.Current;

                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;

                    if (previous == current) continue;

                    yield return previous = current;
                }
            }
        }
    }
}
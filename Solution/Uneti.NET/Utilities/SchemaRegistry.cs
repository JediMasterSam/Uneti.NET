using System.Collections.Generic;
using System.Collections.Immutable;

namespace Uneti.NET
{
    /// <summary>
    /// Stores XML node schemata.
    /// </summary>
    internal sealed class SchemaRegistry
    {
        /// <summary>
        /// Creates a new instance of <see cref="SchemaRegistry"/>.
        /// </summary>
        internal SchemaRegistry()
        {
            Counter = new Counter();
            Schemata = new Dictionary<string, Schema>();
        }

        /// <summary>
        /// The ID counter of the current registry.
        /// </summary>
        private Counter Counter { get; }

        /// <summary>
        /// The XML schemata of the current registry.
        /// </summary>
        private Dictionary<string, Schema> Schemata { get; }

        /// <summary>
        /// Adds the specified property names to the specified schema.
        /// </summary>
        /// <param name="signature">The schema signature.</param>
        /// <param name="propertyNames">The property names.</param>
        internal void AddPropertyNames(string signature, IEnumerable<string> propertyNames)
        {
            if (!Schemata.TryGetValue(signature, out var schema))
            {
                schema = Schemata[signature] = new Schema(Counter);
            }

            schema.AddPropertyNames(propertyNames);
        }

        /// <summary>
        /// Formats the specified properties to match the specified schema.
        /// </summary>
        /// <param name="signature">The schema signature.</param>
        /// <param name="properties">The properties to be formatted.</param>
        /// <param name="schemaId">The ID of the schema.</param>
        /// <returns>The formatted properties.</returns>
        internal ImmutableArray<Bigram> Format(string signature, IReadOnlyDictionary<string, Bigram> properties, out int schemaId)
        {
            if (Schemata.TryGetValue(signature, out var schema))
            {
                return schema.Format(properties, out schemaId);
            }

            schemaId = -1;
            return ImmutableArray<Bigram>.Empty;
        }

        /// <summary>
        /// Represents the structure of an XML node.
        /// </summary>
        private sealed class Schema
        {
            /// <summary>
            /// Creates a new instance of <see cref="Schema"/>.
            /// </summary>
            /// <param name="counter">Sets the ID.</param>
            internal Schema(Counter counter)
            {
                Id = counter.Next();
                PropertyNames = new HashSet<string>();
            }

            /// <summary>
            /// The ID of the current schema.
            /// </summary>
            private int Id { get; }

            /// <summary>
            /// The property names of the current schema.
            /// </summary>
            private HashSet<string> PropertyNames { get; }

            /// <summary>
            /// Adds the specified property names.
            /// </summary>
            /// <param name="propertyNames">The property names to add.</param>
            internal void AddPropertyNames(IEnumerable<string> propertyNames)
            {
                foreach (var propertyName in propertyNames)
                {
                    PropertyNames.Add(propertyName);
                }
            }

            /// <summary>
            /// Formats the specified properties to match the current schema.
            /// </summary>
            /// <param name="properties">The properties to be formatted.</param>
            /// <param name="id">The ID of the current schema.</param>
            /// <returns>The formatted properties.</returns>
            internal ImmutableArray<Bigram> Format(IReadOnlyDictionary<string, Bigram> properties, out int id)
            {
                id = Id;

                var count = 0;
                var values = new Bigram[PropertyNames.Count];

                foreach (var propertyName in PropertyNames)
                {
                    values[count++] = properties.TryGetValue(propertyName, out var value) ? value : Bigram.Empty;
                }

                return values.ToImmutableArray();
            }
        }
    }
}
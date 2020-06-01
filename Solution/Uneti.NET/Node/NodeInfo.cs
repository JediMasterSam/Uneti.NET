using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Uneti.NET
{
    /// <summary>
    /// Represents the raw information from <see cref="XElement"/> that can be used to create a <see cref="Node"/>.
    /// </summary>
    internal sealed class NodeInfo
    {
        /// <summary>
        /// Creates a new instance of <see cref="NodeInfo"/>.
        /// </summary>
        /// <param name="element">The XML element.</param>
        /// <param name="parentSignature">The signature of the parent XML node.</param>
        /// <param name="predicate">If true, the <see cref="XElement"/> will not be loaded.</param>
        /// <param name="schemaRegistry">Stores the schema of the XML node.</param>
        private NodeInfo(XElement element, string parentSignature, Func<XElement, bool> predicate, SchemaRegistry schemaRegistry)
        {
            var name = element.Name.LocalName;
            var signature = parentSignature != null ? $"{parentSignature}.{name}" : name;
            var children = element.Elements().Where(predicate).Select(childElement => new NodeInfo(childElement, signature, predicate, schemaRegistry));
            var properties = new Dictionary<string, Bigram>();
            var text = (element.FirstNode as XText)?.Value;
            
            foreach (var attribute in element.Attributes())
            {
                properties[attribute.Name.LocalName] = new Bigram(attribute.Value);
            }

            if (text != null)
            {
                properties["&text"] = new Bigram(text);
            }
            
            schemaRegistry.AddPropertyNames(signature, properties.Keys);

            Children = children;
            Element = element;
            Properties = properties;
            Signature = signature;
        }

        /// <summary>
        /// The nodes underneath and directly connected to the current node.
        /// </summary>
        internal IEnumerable<NodeInfo> Children { get; }

        /// <summary>
        /// The raw XML element for the current node.
        /// </summary>
        internal XElement Element { get; }

        /// <summary>
        /// The properties of the current node.
        /// </summary>
        internal IReadOnlyDictionary<string, Bigram> Properties { get; }

        /// <summary>
        /// The signature of the current node.
        /// </summary>
        internal string Signature { get; }

        /// <summary>
        /// Parses the specified text into a tree of <see cref="NodeInfo"/>.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <param name="predicate">If true, the <see cref="XElement"/> will not be loaded.</param>
        /// <param name="schemaRegistry">Stores the schema of each XML node.</param>
        /// <returns>The root <see cref="NodeInfo"/>.</returns>
        internal static NodeInfo Parse(string text, Func<XElement, bool> predicate, SchemaRegistry schemaRegistry)
        {
            return new NodeInfo(XDocument.Parse(text, LoadOptions.SetLineInfo).Root, null, predicate, schemaRegistry);
        }
    }
}
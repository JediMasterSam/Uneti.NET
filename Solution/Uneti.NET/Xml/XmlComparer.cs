using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Uneti.NET
{
    /// <summary>
    /// Used for comparing XML documents.
    /// </summary>
    public sealed class XmlComparer
    {
        /// <summary>
        /// Should empty XML nodes be excluded from the result?
        /// </summary>
        public bool ExcludeEmptyNodes { get; set; }
        
        /// <summary>
        /// If true, the <see cref="XElement"/> will not be loaded.
        /// </summary>
        public Func<XElement, bool> Predicate { get; set; }

        /// <summary>
        /// Gets the minimal set of set of edits to transform the specified expected text into the specified actual text.
        /// </summary>
        /// <param name="expectedText">The expected text.</param>
        /// <param name="actualText">The actual text.</param>
        /// <returns>A new <see cref="XmlEdit"/> for each edit found.</returns>
        public IEnumerable<XmlEdit> GetEdits(string expectedText, string actualText)
        {
            return NodeEdit.GetEdits(expectedText, actualText, ExcludeEmptyNodes, Predicate ?? DefaultPredicate).Select(nodeEdit => new XmlEdit(nodeEdit.ActualNode?.Element, nodeEdit.ExpectedNode?.Element, nodeEdit.Operation));
        }

        /// <summary>
        /// The default predicate.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>True.</returns>
        private static bool DefaultPredicate(XElement element)
        {
            return true;
        }
    }
}
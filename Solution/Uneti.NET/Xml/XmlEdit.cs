using System.Xml;
using System.Xml.Linq;

namespace Uneti.NET
{
    /// <summary>
    /// Represents an edit between two <see cref="XElement"/>.
    /// </summary>
    public sealed class XmlEdit
    {
        /// <summary>
        /// Create a new instance of <see cref="XmlEdit"/>.
        /// </summary>
        /// <param name="actualElement">The actual element.</param>
        /// <param name="expectedElement">The expected element.</param>
        /// <param name="operation">The edit operation.</param>
        internal XmlEdit(XElement actualElement, XElement expectedElement, Operation operation)
        {
            ActualElement = actualElement;
            ActualLineNumber = GetLineNumber(actualElement);
            ExpectedElement = expectedElement;
            ExpectedLineNumber = GetLineNumber(expectedElement);
            Operation = operation;
        }

        /// <summary>
        /// The actual element of the current XML edit.
        /// </summary>
        public XElement ActualElement { get; }

      
        /// <summary>
        /// The line number of <see cref="ActualElement"/>.
        /// </summary>
        public int ActualLineNumber { get; }

        /// <summary>
        /// The expected element of the current XML edit.
        /// </summary>
        public XElement ExpectedElement { get; }

        /// <summary>
        /// The line number of <see cref="ExpectedElement"/>.
        /// </summary>
        public int ExpectedLineNumber { get; }
        
        /// <summary>
        /// The operation of the current edit.
        /// </summary>
        public Operation Operation { get; }

        /// <summary>
        /// Gets the line number of the specified element.
        /// </summary>
        /// <param name="element">The element from which to get the line number.</param>
        /// <returns>The line number of the specified element.</returns>
        private static int GetLineNumber(XElement element)
        {
            return element is IXmlLineInfo lineInfo ? lineInfo.LineNumber : -1;
        }
    }
}
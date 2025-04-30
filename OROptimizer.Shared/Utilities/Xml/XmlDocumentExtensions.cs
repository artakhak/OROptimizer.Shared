// This software is part of the OROptimizer library
// Copyright © 2018 OROptimizer Contributors
// http://oroptimizer.com
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using JetBrains.Annotations;

namespace OROptimizer.Utilities.Xml
{
    /// <summary>
    /// Xml extension methods.
    /// </summary>
    public static class XmlDocumentExtensions
    {
        /// <summary>
        /// Clones a sibling of element at <paramref name="xmlElementPath"/> and adds to parent next to the coned element.
        /// </summary>
        /// <param name="xmlDocument">Xml document.</param>
        /// <param name="xmlElementPath">Cloned xml element path.</param>
        /// <param name="predicate">A predicate for selecting </param>
        /// <returns>Returns the cloned element.</returns>
        [NotNull]
        public static XmlElement AddSiblingClone([NotNull] this XmlDocument xmlDocument, [NotNull] string xmlElementPath, [CanBeNull] Predicate<XmlElement> predicate = null)
        {
            var xmlElement = xmlDocument.SelectElement(xmlElementPath, predicate);
            var clonedXmlElement = (XmlElement)xmlElement.Clone();
            xmlElement.ParentNode?.InsertAfter(clonedXmlElement, xmlElement);
            return clonedXmlElement;
        }

        /// <summary>
        /// Creates a new element and inserts it at index <paramref name="index"/> in element <paramref name="parentXmlElement"/>. 
        /// </summary>
        /// <param name="parentXmlElement">Xml element to which to add the new element.</param>
        /// <param name="elementName">Inserted element name.</param>
        /// <param name="index">Index at which to insert the new element.</param>
        /// <returns>Returns the new element.</returns>
        /// <exception cref="ArgumentException">Throws this exception if <paramref name="parentXmlElement"/>.<see cref="XmlElement.OwnerDocument"/> is null.</exception>
        [NotNull]
        public static XmlElement InsertChildElement(this XmlElement parentXmlElement, string elementName, int? index = null)
        {
            if (parentXmlElement.OwnerDocument == null)
                throw new ArgumentException($"The value of {nameof(parentXmlElement)}.{nameof(XmlElement.OwnerDocument)} cannot ne null");

            var childElement = parentXmlElement.OwnerDocument.CreateElement(elementName);

            XmlElement siblingElementBeforeWhichToInsert = null;

            if (index != null)
            {
                var currentSiblingIndex = 0;

                foreach (var siblingNode in parentXmlElement.ChildNodes)
                {
                    var siblingElement = siblingNode as XmlElement;

                    if (siblingElement == null)
                        continue;

                    if (currentSiblingIndex == index)
                    {
                        siblingElementBeforeWhichToInsert = siblingElement;
                        break;
                    }

                    ++currentSiblingIndex;
                }
            }

            if (siblingElementBeforeWhichToInsert != null)
                parentXmlElement.InsertBefore(childElement, siblingElementBeforeWhichToInsert);
            else
                parentXmlElement.AppendChild(childElement);

            return childElement;
        }

        /// <summary>
        /// Removes attribute from element <paramref name="xmlElement"/> and returns the element.
        /// </summary>
        /// <param name="xmlElement"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        [NotNull]
        public static XmlElement Remove(this XmlElement xmlElement, string attributeName)
        {
            xmlElement.RemoveAttribute(attributeName);
            return xmlElement;
        }

        /// <summary>
        /// Removes the xml element <paramref name="xmlElement"/> from parent of <paramref name="xmlElement"/>.
        /// </summary>
        /// <param name="xmlElement">The removed element.</param>
        public static void Remove(this XmlElement xmlElement)
        {
            xmlElement.ParentNode?.RemoveChild(xmlElement);
        }

        /// <summary>
        /// Removes a child element at <paramref name="childElementPath"/> from element <paramref name="xmlElement"/>
        /// and returns the element <paramref name="xmlElement"/>.
        /// </summary>
        /// <param name="xmlElement">Xml element from which to remove a child.</param>
        /// <param name="childElementPath">Child element path relative to <paramref name="xmlElement"/>.</param>
        /// <param name="predicate">A predicate applied when looking up the child element.</param>
        /// <returns>Returns <paramref name="xmlElement"/>.</returns>
        [NotNull]
        public static XmlElement RemoveChildElement(this XmlElement xmlElement, [NotNull] string childElementPath,
                                                    [CanBeNull] Predicate<XmlElement> predicate = null)
        {
            var childElement = xmlElement.SelectChildElement(childElementPath, predicate);
            childElement.ParentNode?.RemoveChild(childElement);

            return xmlElement;
        }

        /// <summary>
        /// Selects the first child in <paramref name="xmlElement"/> at path <paramref name="childElementPath"/>
        /// relative to <paramref name="xmlElement"/> which is also selected by the predicate <paramref name="predicate"/>, if the
        /// <paramref name="predicate"/> is not null.
        /// </summary>
        /// <param name="xmlElement">Xml element in which child element is selected.</param>
        /// <param name="childElementPath">Child element path relative to element <paramref name="xmlElement"/>.</param>
        /// <param name="predicate">A predicate used for selecting a child element. The value can be null.</param>
        /// <returns>Returns the selected child element.</returns>
        /// <exception cref="Exception">Throws an exception if child element is not found.</exception>
        [NotNull]
        public static XmlElement SelectChildElement([NotNull] this XmlElement xmlElement, [NotNull] string childElementPath,
                                                    [CanBeNull] Predicate<XmlElement> predicate = null)
        {
            var allChildNodes = xmlElement.SelectNodes(childElementPath);

            if (allChildNodes != null)
                foreach (var xmlNode in allChildNodes)
                {
                    var childElement = xmlNode as XmlElement;

                    if (childElement == null)
                        continue;

                    if (predicate == null || predicate(childElement))
                        return childElement;
                }

            throw new Exception("Child element not found.");
        }

        /// <summary>
        /// Selects the first element at path <paramref name="xmlElementPath"/> in document <paramref name="xmlDocument"/>.
        /// </summary>
        /// <param name="xmlDocument">Document where the element is looked up.</param>
        /// <param name="xmlElementPath">Selected element path.</param>
        /// <param name="predicate">A predicate to apply when looking up the element. The value can be null. </param>
        /// <returns>Returns the selected element.</returns>
        /// <exception cref="Exception">Throws an exception if no element was selected.</exception>
        [NotNull]
        public static XmlElement SelectElement(this XmlDocument xmlDocument, string xmlElementPath, Predicate<XmlElement> predicate = null)
        {
            var firstElement = xmlDocument.SelectElements(xmlElementPath).FirstOrDefault(x => predicate == null || predicate(x));

            if (firstElement == null)
                throw new Exception($"No element with path='{xmlElementPath}' was found.");

            return firstElement;
        }

        /// <summary>
        /// Selects collection of elements at path <paramref name="xmlElementPath"/> in document <paramref name="xmlDocument"/>.
        /// </summary>
        /// <param name="xmlDocument">Xml document.</param>
        /// <param name="xmlElementPath">Selected elements path.</param>
        /// <returns>Returns a collection of selected elements.</returns>
        [NotNull]
        [ItemNotNull]
        public static IEnumerable<XmlElement> SelectElements(this XmlDocument xmlDocument, string xmlElementPath)
        {
            var selectedNodes = xmlDocument.SelectNodes(xmlElementPath);
            if (selectedNodes != null)
                foreach (var node in selectedNodes)
                    if (node is XmlElement element)
                        yield return element;
        }

        /// <summary>
        /// Sets the value of attribute <paramref name="attributeName"/> to <paramref name="newValue"/> in element <paramref name="xmlElement"/> and returns element <paramref name="xmlElement"/>.
        /// </summary>
        /// <param name="xmlElement">Xml element.</param>
        /// <param name="attributeName">Attribute name.</param>
        /// <param name="newValue">New value of the attribute.</param>
        /// <returns>Returns the element <paramref name="xmlElement"/>.</returns>
        [NotNull]
        public static XmlElement SetAttributeValue(this XmlElement xmlElement, string attributeName, string newValue)
        {
            xmlElement.SetAttribute(attributeName, newValue);
            return xmlElement;
        }

        /// <summary>
        /// Checks if element <paramref name="xmlElementPath"/> exists.
        /// </summary>
        /// <param name="xmlDocument">The xml document.</param>
        /// <param name="xmlElementPath">Element path.</param>
        /// <param name="predicate">Predicate applied to elements.</param>
        /// <exception cref="Exception">Throws this exception if element with path <paramref name="xmlElementPath"/> that satisfies the predicate <paramref name="predicate"/>
        /// does not exist.</exception>
        public static void ValidateElementExists(this XmlDocument xmlDocument, string xmlElementPath, Predicate<XmlElement> predicate = null)
        {
            xmlDocument.SelectElement(xmlElementPath, predicate);
        }
    }
}
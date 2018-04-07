using System;
using System.Xml;

namespace CoreWebAppExample
{
    public static class XmlExtensions
    {
        public static XmlElement AppendElement(this XmlElement parent, string prefix, string localName, string namespaceURI)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");

            return (XmlElement)parent.AppendChild(parent.OwnerDocument.CreateElement(prefix, localName, namespaceURI));
        }

        public static XmlElement AppendElement(this XmlElement parent, string qualifiedName, string namespaceURI)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");

            return (XmlElement)parent.AppendChild(parent.OwnerDocument.CreateElement(qualifiedName, namespaceURI));
        }

        public static XmlElement AppendElement(this XmlElement parent, string name)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");

            return (XmlElement)parent.AppendChild(parent.OwnerDocument.CreateElement(name));
        }

        public static XmlElement AppendText(this XmlElement element, string text, bool? asCData = null)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            XmlCharacterData lastChild = (element.IsEmpty) ? null : element.LastChild as XmlCharacterData;
            
            if (asCData.HasValue)
            {
                if (asCData.Value)
                {
                    if (lastChild != null && lastChild is XmlCDataSection)
                    {
                        if (!String.IsNullOrEmpty(text))
                            lastChild.InnerText += text;
                    }
                    else
                        element.AppendChild(element.OwnerDocument.CreateCDataSection(text ?? ""));
                    return element;
                }

                if (lastChild != null)
                {
                    if (ReferenceEquals(lastChild, element.FirstChild))
                    {
                        element.InnerText = text;
                        return element;
                    }
                    
                    if (lastChild is XmlWhitespace)
                    {
                        element.AppendChild(element.OwnerDocument.CreateTextNode(text ?? ""));
                        element.RemoveChild(lastChild);
                        return element;
                    }
                    if (lastChild is XmlSignificantWhitespace)
                    {
                        if (!String.IsNullOrEmpty(text))
                        {
                            element.AppendChild(element.OwnerDocument.CreateTextNode(lastChild.InnerText + text));
                            element.RemoveChild(lastChild);
                        }
                        return element;
                    }
                    if (lastChild is XmlText)
                    {
                        if (!String.IsNullOrEmpty(text))
                            lastChild.InnerText += text;
                        return element;
                    }
                }
            }

            if (element.IsEmpty)
                element.InnerText = text ?? "";
            else
                element.AppendChild(element.OwnerDocument.CreateTextNode(text ?? ""));
            return element;
        }
        
        public static XmlElement AppendTextElement(this XmlElement parent, string prefix, string localName, string namespaceURI, string innerText, bool asCData = false)
        {
            XmlElement element = parent.AppendElement(prefix, localName, namespaceURI);
            if (asCData)
                element.AppendChild(parent.OwnerDocument.CreateCDataSection(innerText));
            else
                element.InnerText = innerText;
            return element;
        }

        public static XmlElement AppendTextElement(this XmlElement parent, string qualifiedName, string namespaceURI, string innerText, bool asCData = false)
        {
            XmlElement element = parent.AppendElement(qualifiedName, namespaceURI);
            if (asCData)
                element.AppendChild(parent.OwnerDocument.CreateCDataSection(innerText));
            else
                element.InnerText = innerText;
            return element;
        }

        public static XmlElement AppendTextElement(this XmlElement parent, string name, string innerText, bool asCData = false)
        {
            XmlElement element = parent.AppendElement(name);
            if (asCData)
                element.AppendChild(parent.OwnerDocument.CreateCDataSection(innerText));
            else
                element.InnerText = innerText;
            return element;
        }

        public static XmlAttribute GetNamedAttribute(this XmlElement element, string localName, string namespaceURI)
        {
            return element.Attributes.GetNamedItem(localName, namespaceURI) as XmlAttribute;
        }

        public static XmlAttribute GetNamedAttribute(this XmlElement element, string name)
        {
            return element.Attributes.GetNamedItem(name) as XmlAttribute;
        }

        public static XmlElement ApplyAttributeValue(this XmlElement element, string prefix, string localName, string namespaceURI, string value)
        {
            XmlAttribute attribute = element.GetNamedAttribute(localName, namespaceURI);
            if (attribute != null)
            {
                if (value == null)
                {
                    element.Attributes.Remove(attribute);
                    return element;
                }
                if (prefix == attribute.Prefix)
                {
                    attribute.Value = value;
                    return element;
                }
                element.Attributes.Remove(attribute);
            }

            element.Attributes.Append(element.OwnerDocument.CreateAttribute(prefix, localName, namespaceURI)).Value = value;
            return element;
        }

        public static XmlElement ApplyAttributeValue(this XmlElement element, string name, string value)
        {
            XmlAttribute attribute = element.GetNamedAttribute(name);
            if (attribute != null)
            {
                if (value == null)
                {
                    element.Attributes.Remove(attribute);
                    return element;
                }
                if (name == attribute.Name)
                {
                    attribute.Value = value;
                    return element;
                }
                element.Attributes.Remove(attribute);
            }

            element.Attributes.Append(element.OwnerDocument.CreateAttribute(name)).Value = value;
            return element;
        }
    }
}
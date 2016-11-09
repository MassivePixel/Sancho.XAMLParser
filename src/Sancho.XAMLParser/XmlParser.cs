// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Sancho.DOM.Model;
using Serilog;

namespace Sancho.XAMLParser
{
    public class XmlParser : IParser
    {
        public XmlParser()
        {
            Log.Debug("Parser instantiated");
        }

        public XamlNode Parse(string xaml)
        {
            Log.Debug("Parser invoked with {XAML}", xaml?.Substring(0, Math.Min(xaml?.Length ?? 0, 30)));

            if (string.IsNullOrWhiteSpace(xaml))
            {
                Log.Information("The specified XAML is empty");
                return null;
            }

            var doc = XDocument.Parse(xaml);
            if (doc == null)
            {
                Log.Information("Cannot parse xaml");
                return null;
            }

            var root = ParseNode(doc.Root);
            return root;
        }

        public static XamlNode ParseNode(XElement element)
        {
            var node = new XamlNode(element.Name);

            foreach (var attribute in element.Attributes())
                node.Properties.Add(ParseAttribute(element, attribute));

            if (element.HasElements)
            {
                foreach (var child in element.Elements())
                {
                    if (child.Name.LocalName.Contains("."))
                    {
                        if (child.HasElements)
                        {
                            node.Properties.Add(new XamlNodesProperty(child.Name, ParseNodes(child.Elements())));
                        }
                        else if (!string.IsNullOrEmpty(child.Value))
                        {
                            node.Properties.Add(new XamlStringProperty(child.Name, child.Value));
                        }
                    }
                    else
                    {
                        node.Children.Add(ParseNode(child));
                    }
                }
            }
            else if (!string.IsNullOrEmpty(element.Value))
            {
                node.Properties.Add(new XamlStringProperty(null, element.Value));
            }

            return node;
        }

        static IEnumerable<XamlNode> ParseNodes(IEnumerable<XElement> elements)
        => elements.Select(el => ParseNode(el));

        public static XamlProperty ParseAttribute(XElement element, XAttribute attribute)
        => attribute.IsNamespaceDeclaration
            ? (XamlProperty)new XamlNamespaceProperty(attribute.Name, attribute.Value)
            : (XamlProperty)new XamlStringProperty(attribute.Name, attribute.Value);
    }
}

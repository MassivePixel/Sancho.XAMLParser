// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SimpleXamlParser.Interfaces;

namespace SimpleXamlParser
{
    public class Parser
    {
        IXamlDOM dom;

        public Action<string> Logger { get; }

        public Parser(IXamlDOM dom, Action<string> logger = null)
        {
            this.dom = dom;
            Logger = logger ?? delegate (string s) { };
            dom.Logger = Logger;
        }

        public XamlNode Parse(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
            {
                Logger("The specified XAML is empty");
                return null;
            }

            var doc = XDocument.Parse(xaml);
            if (doc == null)
            {
                Logger("Cannot parse xaml");
                return null;
            }

            var root = ParseNode(doc.Root);
            // TODO: extract DOM creation outside of parser
            dom.AddNode(root);
            return root;
        }

        public static XamlNode ParseNode(XElement element)
        {
            var node = new XamlNode(element.Name.LocalName);

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
                            // TODO: do this as an extra pass rather than here
                            if (IsContentNode(element, child))
                            {
                                node.Children.AddRange(child.Elements()
                                                            .Select(el => ParseNode(el)));
                            }
                            else
                            {
                                node.Properties.Add(new XamlNodesProperty(child.Name, ParseNodes(child.Elements())));
                            }
                        }
                        else if (!string.IsNullOrEmpty(child.Value))
                        {
                            node.Properties.Add(new XamlStringProperty(child.Value));
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
                node.Properties.Add(new XamlStringProperty(element.Value));
            }

            return node;
        }

        static IEnumerable<XamlNode> ParseNodes(IEnumerable<XElement> elements)
        => elements.Select(el => ParseNode(el));

        public static XamlProperty ParseAttribute(XElement element, XAttribute attribute)
        => new XamlStringProperty(attribute.Name, attribute.Value);

        // TODO: extract
        public static bool IsContentNode(XElement element, XElement child)
        {
            var lastDot = child?.Name?.LocalName?.LastIndexOf(".");
            if (lastDot >= 0)
            {
                var type = ReflectionHelpers.GetType(element?.Name?.LocalName);
                if (type != null)
                {
                    var contentPropName = ReflectionHelpers.GetContentProperty(type)?.Name;
                    return !string.IsNullOrWhiteSpace(contentPropName) &&
                           contentPropName == child.Name.LocalName.Substring(lastDot.Value + 1);
                }
            }

            return false;
        }
    }
}

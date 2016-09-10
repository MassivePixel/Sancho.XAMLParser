// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Sancho.XAMLParser
{
    public static class XamlPropertyExtensions
    {
        public static string GetString(this XamlProperty prop)
        {
            return prop is XamlStringProperty
                ? ((XamlStringProperty)prop).Value
                : null;
        }

        public static List<XamlNode> GetNodes(this XamlProperty prop)
        {
            return prop is XamlNodesProperty
                ? ((XamlNodesProperty)prop).Nodes
                : new List<XamlNode>();
        }
    }

    public abstract class XamlProperty
    {
        public string Name { get; set; }

        public XamlProperty()
        {
        }

        public XamlProperty(string name)
        {
            Name = name;
        }
    }

    public class XamlStringProperty : XamlProperty
    {
        public string Value { get; }

        public XamlStringProperty(XName name, string value = null)
            : base(name.LocalName)
        {
            Value = value;
        }

        public XamlStringProperty(string name, string value = null)
            : base(name)
        {
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("[Name={0}, Value={1}]",
                                 Name,
                                 Value != null ? Value.Substring(0, Math.Min(20, Value.Length)) : null);
        }
    }

    public class XamlNodesProperty : XamlProperty
    {
        public List<XamlNode> Nodes { get; }

        public XamlNodesProperty()
        {
            Nodes = new List<XamlNode>();
        }

        public XamlNodesProperty(XName name, IEnumerable<XamlNode> nodes)
            : base(name.LocalName)
        {
            Nodes = (nodes ?? Enumerable.Empty<XamlNode>()).ToList();
        }

        public XamlNodesProperty(string name, IEnumerable<XamlNode> nodes)
            : base(name)
        {
            Nodes = (nodes ?? Enumerable.Empty<XamlNode>()).ToList();
        }

        public override string ToString()
        {
            return string.Format("[Name={0}, Nodes={1}]", Name,
                                 Nodes.Any() == false
                                 ? "{ }"
                                 : Nodes.Count == 1
                                 ? $"{{ {Nodes[0]} }}"
                                 : $"{{ {Nodes[0]}, ... }}");
        }
    }
}

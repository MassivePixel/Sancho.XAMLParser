// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SimpleXamlParser
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
                ? ((XamlNodesProperty)prop).Values
                : new List<XamlNode>();
        }
    }

    public abstract class XamlProperty
    {
        public string Name { get; }

        public XamlProperty()
        {
        }

        public XamlProperty(string name)
        {
            // extract last part of the name
            var dotIndex = name.IndexOf(".");
            Name = dotIndex != -1 ? name.Substring(dotIndex + 1) : name;
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
        public List<XamlNode> Values { get; }

        public XamlNodesProperty()
        {
            Values = new List<XamlNode>();
        }

        public XamlNodesProperty(XName name, IEnumerable<XamlNode> values)
            : base(name.LocalName)
        {
            Values = (values ?? Enumerable.Empty<XamlNode>()).ToList();
        }

        public XamlNodesProperty(string name, IEnumerable<XamlNode> values)
            : base(name)
        {
            Values = (values ?? Enumerable.Empty<XamlNode>()).ToList();
        }

        public override string ToString()
        {
            return string.Format("[Name={0}, Values={1}]", Name,
                                 Values.Any() == false
                                 ? "{ }"
                                 : Values.Count == 1
                                 ? $"{{ {Values[0]} }}"
                                 : $"{{ {Values[0]}, ... }}");
        }
    }
}

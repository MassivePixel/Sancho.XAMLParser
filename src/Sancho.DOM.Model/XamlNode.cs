// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Xml.Linq;

namespace Sancho.DOM.Model
{
    public class XamlNode
    {
        public string Namespace { get; }
        public string Name { get; }

        public List<XamlNode> Children { get; } = new List<XamlNode>();
        public List<XamlProperty> Properties { get; } = new List<XamlProperty>();

        public XamlNode(string name)
        {
            Name = name;
        }

        public XamlNode(XName name)
        {
            Namespace = name.NamespaceName;
            Name = name.LocalName;
        }

        public override string ToString()
        {
            return string.Format("[Name={0}, Children={1}, Properties={2}]",
                                 Name,
                                 Children.Count == 0
                                 ? "{ }"
                                 : Children.Count == 1
                                 ? $"{{ {Children[0]} }}"
                                 : $"{{ {Children[0]}, ... }}",
                                 Properties.Count == 0
                                 ? "{}"
                                 : Properties.Count == 1
                                 ? $"{{ {Properties[0]} }}"
                                 : $"{{ {Properties[0]}, ... }}");
        }
    }
}

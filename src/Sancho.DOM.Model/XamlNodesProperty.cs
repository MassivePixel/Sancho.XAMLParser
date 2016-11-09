// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Sancho.DOM.Model
{
    public class XamlNodesProperty : XamlProperty
    {
        public List<XamlNode> Nodes { get; }

        public XamlNodesProperty()
        {
            Nodes = new List<XamlNode>();
        }

        public XamlNodesProperty(XName name, IEnumerable<XamlNode> nodes)
            : base(name)
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

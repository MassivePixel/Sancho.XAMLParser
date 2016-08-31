// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace SimpleXamlParser
{
    public class XamlNodeCollection
    {
        public List<XamlNode> Nodes { get; } = new List<XamlNode>();

        public XamlNodeCollection(IEnumerable<XamlNode> nodes)
        {
            Nodes = nodes.ToList();
        }

        public override string ToString()
        {
            return string.Format("[XamlNodeCollection: Nodes={0}]", Nodes);
        }
    }
}

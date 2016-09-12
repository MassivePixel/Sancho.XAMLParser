// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System.Linq;
using Sancho.XAMLParser;
using Serilog;

namespace Sancho.DOM.XamarinForms
{
    public class ContentNodeProcessor : IXamlNodeProcessor
    {
        public XamlNode Process(XamlNode node)
        {
            if (node == null)
            {
                Log.Warning("Root node is null");
                return node;
            }

            foreach (var prop in node.Properties
                                     .Where(p => XamlNodeHelpers.IsContentNode(node.Name, p.Name))
                                     .ToList())
            {
                if (prop is XamlNodesProperty)
                {
                    foreach (var child in ((XamlNodesProperty)prop).Nodes)
                        node.Children.Add(child);
                    node.Properties.Remove(prop);
                }
            }

            foreach (var prop in node.Properties.OfType<XamlNodesProperty>())
            {
                foreach (var child in prop.Nodes)
                    Process(child);
            }

            foreach (var child in node.Children)
                Process(child);

            return node;
        }
    }
}

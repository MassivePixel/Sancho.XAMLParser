// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Sancho.XAMLParser;
using Serilog;

namespace Sancho.DOM.XamarinForms
{
    public class ExpandedPropertiesProcessor : IXamlNodeProcessor
    {
        public XamlNode Process(XamlNode node)
        {
            if (node == null)
            {
                Log.Warning("Node is null");
                return node;
            }

            foreach (var prop in node.Properties
                                     .Where(p => p?.Name?.Contains(".") == true)
                                     .ToList())
            {
                var parts = prop.Name.Split(new[] { '.' });
                if (parts[0] == node.Name)
                {
                    // this could still be an attached property
                    // e.g. <Grid Grid.Row
                    var type = ReflectionHelpers.GetType(node.Name);
                    if (type?.GetRuntimeProperty(parts[1]) != null)
                        prop.Name = parts[1];
                }
            }

            foreach (var child in node.Children)
                Process(child);

            return node;
        }
    }
}

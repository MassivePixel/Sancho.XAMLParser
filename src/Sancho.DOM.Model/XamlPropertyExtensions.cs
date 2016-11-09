// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Sancho.DOM.Model
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
}

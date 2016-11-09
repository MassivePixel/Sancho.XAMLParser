// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System.Xml.Linq;

namespace Sancho.DOM.Model
{
    public abstract class XamlProperty
    {
        public string Namespace { get; }
        public string Name { get; set; }

        public bool IsContent => string.IsNullOrWhiteSpace(Name);

        public XamlProperty()
        {
        }

        public XamlProperty(XName name)
        {
            Namespace = name.NamespaceName;
            Name = name.LocalName;
        }

        public XamlProperty(string name)
        {
            Name = name;
        }
    }
}

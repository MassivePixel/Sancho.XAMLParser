// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System.Xml.Linq;

namespace Sancho.DOM.Model
{
    public class XamlNamespaceProperty : XamlProperty
    {
        public string Value { get; }

        public XamlNamespaceProperty(XName name, string value)
            : base(name)
        {
            Value = value;
        }

        public override string ToString()
        => $"[XamlNamespace Namespace={Namespace}, Name={Name}: Value={Value}]";
    }
}

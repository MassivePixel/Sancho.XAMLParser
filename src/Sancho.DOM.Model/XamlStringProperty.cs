// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;
using System.Xml.Linq;

namespace Sancho.DOM.Model
{
    public class XamlStringProperty : XamlProperty
    {
        public string Value { get; }

        public XamlStringProperty(XName name, string value = null)
            : base(name)
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
}

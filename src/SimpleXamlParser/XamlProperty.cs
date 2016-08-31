// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

namespace SimpleXamlParser
{
    public class XamlProperty
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            return string.Format("[XamlProperty: Name={0}, Value={1}]", Name, Value);
        }
    }
}

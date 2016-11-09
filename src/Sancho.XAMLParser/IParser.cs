// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using Sancho.DOM.Model;

namespace Sancho.XAMLParser
{
    public interface IParser
    {
        XamlNode Parse(string doc);
    }
}
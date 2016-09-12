// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using Sancho.XAMLParser;

namespace Sancho.DOM.XamarinForms
{
    public interface IXamlNodeProcessor
    {
        XamlNode Process(XamlNode node);
    }
}

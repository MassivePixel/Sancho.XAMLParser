// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;

namespace SimpleXamlParser.Interfaces
{
    public interface IPlatformServices
    {
        List<Assembly> GetAssemblies();
    }
}

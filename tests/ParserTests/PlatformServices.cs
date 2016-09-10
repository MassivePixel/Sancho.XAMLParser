// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sancho.XAMLParser.Interfaces;

namespace XamarinFormsTests
{
    public class PlatformServices : IPlatformServices
    {
        public List<Assembly> GetAssemblies() => AppDomain.CurrentDomain.GetAssemblies().ToList();
    }
}

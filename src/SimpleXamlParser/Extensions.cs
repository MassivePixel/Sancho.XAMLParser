// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

namespace SimpleXamlParser
{
    public static class Extensions
    {
        public static T As<T>(this object o) where T : class => o as T;
        public static T Cast<T>(this object o) => (T)o;
    }
}

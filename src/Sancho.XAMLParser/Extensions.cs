// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;

namespace Sancho.XAMLParser
{
    public static class Extensions
    {
        public static T As<T>(this object o) where T : class => o as T;
        public static T Cast<T>(this object o) => (T)o;

        public static R IfNotNull<T, R>(this T o, Func<T, R> func)
            where T : class
        {
            if (o == null) return default(R);
            return func(o);
        }
    }
}

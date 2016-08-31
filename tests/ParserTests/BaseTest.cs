// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;
using System.Text;
using Xunit;
using SimpleXamlParser;

namespace DeserializationTests
{
    public class ReflectionFixture : IDisposable
    {
        public ReflectionFixture()
        {
            ReflectionHelpers.PlatformServices = new PlatformServices();
        }

        public void Dispose() { }
    }

    public class BaseTest
        : IClassFixture<ReflectionFixture>
    {
        StringBuilder sb = new StringBuilder();
        protected Parser parser;
        XamlDOMCreator dom = new XamlDOMCreator();

        public BaseTest()
        {
            parser = new Parser(dom, s => sb.AppendLine(s));
        }

        protected object ParseVisualElement(string xaml)
        {
            parser.Parse(xaml);
            return dom.Root;
        }

        protected T ParseVisualElement<T>(string xaml)
            where T : class
        {
            parser.Parse(xaml);
            return dom.Root as T;
        }
    }
}

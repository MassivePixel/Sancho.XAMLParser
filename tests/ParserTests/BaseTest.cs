// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;
using System.Text;
using Sancho.DOM.XamarinForms;
using Sancho.XAMLParser;
using Xunit;

namespace XamarinFormsTests
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
        XamlDOMCreator dom = new XamlDOMCreator
        {
            Platform = Xamarin.Forms.TargetPlatform.iOS
        };

        public BaseTest()
        {
            parser = new Parser();
        }

        //protected object ParseVisualElement(string xaml)
        //{
        //    var rootNode = parser.Parse(xaml);
        //    rootNode = new ContentNodeProcessor().Process(rootNode);
        //    return dom.CreateNode(rootNode);
        //}

        protected T ParseVisualElement<T>(string xaml)
            where T : class
        {
            var rootNode = parser.Parse(xaml);
            rootNode = new ContentNodeProcessor().Process(rootNode);
            rootNode = new ExpandedPropertiesProcessor().Process(rootNode);
            dom.AddNode(rootNode);
            return dom.Root as T;
        }
    }
}

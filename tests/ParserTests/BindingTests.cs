// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using Xamarin.Forms;
using Xunit;
using SimpleXamlParser;

namespace DeserializationTests
{
    public class BindingTests : BaseTest
    {
        [Fact]
        public void ParseAttributeBinding()
        {
            var label = ParseVisualElement<Label>(@"<Label Text=""{Binding X}"" />");
            label.BindingContext = new
            {
                X = "hello"
            };
            Assert.Equal("hello", label.Text);
        }
        
        [Fact]
        public void ParseBinding()
        {
            var label = new Label
            {
                BindingContext = new
                {
                    X = "hello"
                }
            };

            AttributeHelper.Apply(label, label.GetType().GetProperty("Text"), "{Binding X}");
            Assert.Equal("hello", label.Text);
        }

        [Fact]
        public void ParseBinding2()
        {
            var label = new Label
            {
                BindingContext = "hello"
            };

            AttributeHelper.Apply(label, label.GetType().GetProperty("Text"), "{Binding}");
            Assert.Equal("hello", label.Text);
        }

        [Fact]
        public void ParseBinding3()
        {
            var label = new Label
            {
                BindingContext = "hello"
            };

            AttributeHelper.Apply(label, label.GetType().GetProperty("Text"), "{Binding .}");
            Assert.Equal("hello", label.Text);
        }
    }
}

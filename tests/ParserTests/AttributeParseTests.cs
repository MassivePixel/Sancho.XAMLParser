// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;
using Sancho.DOM.XamarinForms;
using Xamarin.Forms;
using Xunit;

namespace XamarinFormsTests
{
    public class AttributeParseTests : BaseTest
    {
        [Fact]
        public void ParseString() => TestConversion<string>("hello", "hello");

        [Fact]
        public void ParseDouble() => TestConversion<double>(1.2, "1.2");

        [Fact]
        public void ParseInt32() => TestConversion<Int32>(1, "1");

        [Fact]
        public void ParseColor() => TestConversion(Color.Red, "Red");

        [Fact]
        public void ParseMargin() => TestConversion(new Thickness(1), "1");

        [Fact]
        public void ParseMargin2() => TestConversion(new Thickness(1, 2), "1,2");

        [Fact]
        public void ParseMargin3() => TestConversion(new Thickness(1, 2, 3, 4), "1,2,3,4");

        [Fact]
        public void TestLayoutOptions() => TestConversion(LayoutOptions.Start, "Start");

        [Fact]
        public void ParseStackOrientation() => TestConversion(StackOrientation.Horizontal, "Horizontal");

        void TestConversion<T>(T expected, string input)
        {
            object value;
            var can = AttributeHelper.Parse(typeof(T), input, out value);
            Assert.True(can);
            Assert.Equal(expected, (T)value);
        }
    }
}

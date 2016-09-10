// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using Xamarin.Forms;
using Xunit;

namespace XamarinFormsTests
{
    public class PropertiesTests : BaseTest
    {
        [Fact]
        public void StringAttribute()
        {
            Assert.Equal("hello", ParseVisualElement<Label>(@"<Label Text=""hello"" />").Text);
        }

        [Fact]
        public void DoubleAttribute()
        {
            Assert.Equal(1.2, ParseVisualElement<Grid>(@"<Grid RowSpacing=""1.2"" />").RowSpacing);
        }

        [Fact]
        public void ColorAttribute()
        {
            Assert.Equal(Color.Red, ParseVisualElement<Label>(@"<Label TextColor=""Red"" />").TextColor);
        }
    }
}

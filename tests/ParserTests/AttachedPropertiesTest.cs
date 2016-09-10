// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using Sancho.DOM.XamarinForms;
using Xamarin.Forms;
using Xunit;

namespace XamarinFormsTests
{
    public class AttachedPropertiesTest : BaseTest
    {
        [Fact]
        public void TestGridRow()
        {
            var btn = new Button();
            new XamlDOMCreator().HandleAttachedProperty(btn, "Grid.Row", "2");
            Assert.Equal(2, Grid.GetRow(btn));
        }

        [Fact]
        public void TestGridColumn()
        {
            var btn = new Button();
            new XamlDOMCreator().HandleAttachedProperty(btn, "Grid.Column", "2");
            Assert.Equal(2, Grid.GetColumn(btn));
        }

        [Fact]
        public void TestGridRowSpan()
        {
            var btn = new Button();
            new XamlDOMCreator().HandleAttachedProperty(btn, "Grid.RowSpan", "2");
            Assert.Equal(2, Grid.GetRowSpan(btn));
        }

        [Fact]
        public void TestGridColumnSpan()
        {
            var btn = new Button();
            new XamlDOMCreator().HandleAttachedProperty(btn, "Grid.ColumnSpan", "2");
            Assert.Equal(2, Grid.GetColumnSpan(btn));
        }

        [Fact]
        public void TestXamlAttachedProperty()
        {
            var grid = ParseVisualElement<Grid>(@"<Grid Grid.Row=""1"" />");
            Assert.Equal(1, Grid.GetRow(grid));
        }
    }
}

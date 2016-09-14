// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System.Collections;
using System.Linq;
using System.Reflection;
using Sancho.DOM.XamarinForms;
using Xamarin.Forms;
using Xunit;

namespace XamarinFormsTests
{
    public class ReflectionTests
    {
        [Fact]
        public void TestInternalProperties()
        {
            var style = new Style(typeof(Label));
            style.Setters.Add(Label.TextColorProperty, Color.Red);

            var label = new Label
            {
                Margin = new Thickness(1),
                Style = style
            };
            label.SetBinding(Label.TextProperty, new Binding("Name"));

            var fields = typeof(BindableObject).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            var propsField = fields.FirstOrDefault(x => x.Name == "_properties");

            Assert.NotNull(propsField);

            var props = propsField.GetValue(label) as IEnumerable;
            Assert.NotNull(props);

            foreach (var prop in props)
            {
                var p = prop.GetType().GetField("Property").GetValue(prop);
                var v = prop.GetType().GetField("Value").GetValue(prop);
            }

            var props2 = BindableObjectHelper.GetProperties(label);
        }
    }
}


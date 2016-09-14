using System.Linq;
using Xamarin.Forms;
using Xunit;

namespace Sancho.DOM.XamarinForms.Tests
{
    public class BindableObjectHelperTests
    {
        [Fact]
        public void TestSimpleProperty()
        {
            var label = new Label
            {
                Text = "Hello"
            };
            var props = label.GetProperties();

            var prop = props.FirstOrDefault(p => p.Property == Label.TextProperty);

            Assert.NotNull(prop);
            Assert.Equal(label.Text, prop.Value);
            Assert.Equal(BindableContextAttributes.IsManuallySet, prop.Attributes);
        }

        [Fact]
        public void TestBoundProp()
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, new Binding("Name"));

            var props = label.GetProperties();

            var prop = props.FirstOrDefault(p => p.Property == Label.TextProperty);

            Assert.NotNull(prop);
            Assert.Equal(label.Text, prop.Value);
            Assert.Equal(0, (int)prop.Attributes);
            Assert.NotNull(prop.Binding);
        }

        [Fact]
        public void TestPropFromStyle()
        {
            var style = new Style(typeof(Label));
            style.Setters.Add(Label.TextColorProperty, Color.Red);

            var label = new Label
            {
                Style = style
            };

            var props = label.GetProperties();
            var prop = props.FirstOrDefault(p => p.Property == Label.TextColorProperty);

            Assert.NotNull(prop);
            Assert.Equal(Color.Red, prop.Value);
            Assert.Equal(BindableContextAttributes.IsSetFromStyle, prop.Attributes);
        }

        [Fact]
        public void TestDynamicResource()
        {
            var label = new Label();
            label.SetDynamicResource(Label.TextProperty, "Key");

            var props = label.GetProperties();
            var prop = props.FirstOrDefault(p => p.Property == Label.TextProperty);

            Assert.NotNull(prop);
            Assert.Equal(BindableContextAttributes.IsDynamicResource, prop.Attributes);
        }
    }
}

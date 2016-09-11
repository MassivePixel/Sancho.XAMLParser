using Xamarin.Forms;

namespace Sancho.DOM.XamarinForms
{
    public class XamlServices : IXamlServices
    {
        public object GetResource(string key)
        {
            object value = null;

            if (Application.Current.Resources.TryGetValue(key, out value))
                return value;

            return null;
        }
    }
}

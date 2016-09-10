using Sancho.XAMLParser;

namespace Sancho.DOM.XamarinForms
{
    public interface IXamlNodeProcessor
    {
        XamlNode Process(XamlNode node);
    }
}

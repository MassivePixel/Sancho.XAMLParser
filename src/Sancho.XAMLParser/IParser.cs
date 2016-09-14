namespace Sancho.XAMLParser
{
    public interface IParser
    {
        XamlNode Parse(string doc);
    }
}
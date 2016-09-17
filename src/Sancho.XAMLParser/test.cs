using Sancho.Parser.Core;

namespace Sancho
{
    public class test
    {
        public test()
        {
            var root = XamlProcessor.parseXml("");
            foreach(var children in root.children)
            {
                
            }
        }
    }
}

// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using Sancho.XAMLParser;
using Xamarin.Forms;

namespace Sancho.DOM.XamarinForms
{
    public class ContentViewInjector
    {
        ContentView host;

        public ContentViewInjector(ContentView host)
        {
            this.host = host;
        }

        public void SetXaml(string text)
        {
            var parser = new Parser();
            var rootNode = parser.Parse(text);

            rootNode = new ContentNodeProcessor().Process(rootNode);
            rootNode = new ExpandedPropertiesProcessor().Process(rootNode);

            var creator = new XamlDOMCreator(new XamlServices());
            creator.AddNode(rootNode);
            var dom = creator.Root;

            if (dom is View)
                host.Content = (View)dom;
            else if (dom is ContentPage)
                host.Content = ((ContentPage)dom).Content;
        }

        public void SetBindingContext(object bc)
        {
            host.BindingContext = bc;
        }
    }
}

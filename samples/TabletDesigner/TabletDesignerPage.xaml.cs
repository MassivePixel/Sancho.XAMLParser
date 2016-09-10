using System;
using Sancho.DOM.XamarinForms;
using Sancho.XAMLParser;
using Xamarin.Forms;

namespace TabletDesigner
{
    public interface ILogAccess
    {
        void Clear();
        string Log { get; }
    }

    public partial class TabletDesignerPage : ContentPage
    {
        ILogAccess logAccess;

        public TabletDesignerPage()
        {
            InitializeComponent();
            logAccess = DependencyService.Get<ILogAccess>();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            editor.Text = @"<Button Text=""Hello world"" />";
        }

        void Handle_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            try
            {
                logAccess.Clear();
                var parser = new Parser();
                var rootNode = parser.Parse(e.NewTextValue);

                rootNode = new ContentNodeProcessor().Process(rootNode);
                rootNode = new ExpandedPropertiesProcessor().Process(rootNode);

                var view = new XamlDOMCreator().CreateNode(rootNode) as View;
                Root.Content = view;

                LoggerOutput.Text = logAccess.Log;
                LoggerOutput.TextColor = Color.Black;
            }
            catch (Exception ex)
            {
                LoggerOutput.Text = ex.ToString();
                LoggerOutput.TextColor = Color.Red;
            }
        }
    }
}

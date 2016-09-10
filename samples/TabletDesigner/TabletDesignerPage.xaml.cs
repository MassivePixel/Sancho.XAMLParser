using System;
using Sancho.DOM.XamarinForms;
using Sancho.XAMLParser;
using TabletDesigner.Helpers;
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
            editor.Text = Settings.Xaml;
        }

        void Handle_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            try
            {
                Settings.Xaml = editor.Text;

                logAccess.Clear();
                var parser = new Parser();
                var rootNode = parser.Parse(e.NewTextValue);

                rootNode = new ContentNodeProcessor().Process(rootNode);
                rootNode = new ExpandedPropertiesProcessor().Process(rootNode);

                var dom = new XamlDOMCreator().CreateNode(rootNode);
                if (dom is View)
                    Root.Content = (View)dom;
                else if (dom is ContentPage)
                    Root.Content = ((ContentPage)dom).Content;

                LoggerOutput.FormattedText = FormatLog(logAccess.Log);
                LoggerOutput.TextColor = Color.White;
            }
            catch (Exception ex)
            {
                LoggerOutput.FormattedText = FormatException(ex);
                LoggerOutput.TextColor = Color.FromHex("#FF3030");
            }
        }

        FormattedString FormatLog(string log)
        {
            var fs = new FormattedString();
            var lines = log.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                fs.Spans.Add(new Span
                {
                    Text = line + Environment.NewLine,
                    FontFamily = LoggerOutput.FontFamily,
                    FontSize = LoggerOutput.FontSize,
                    ForegroundColor = GetOutputColor(line)
                });
            }
            return fs;
        }

        Color GetOutputColor(string line)
        {
            if (line.Contains("[Warning]"))
                return Color.FromHex("#FFA500");
            if (line.Contains("[Error]"))
                return Color.Red;

            return Color.White;
        }

        FormattedString FormatException(Exception ex)
        {
            var fs = new FormattedString();
            while (ex != null)
            {
                fs.Spans.Add(new Span
                {
                    Text = ex.Message + Environment.NewLine,
                    ForegroundColor = Color.Red,
                    FontFamily = LoggerOutput.FontFamily,
                    FontSize = LoggerOutput.FontSize,
                });
                fs.Spans.Add(new Span
                {
                    Text = ex.StackTrace + Environment.NewLine,
                    ForegroundColor = Color.Red,
                    FontFamily = LoggerOutput.FontFamily,
                    FontSize = LoggerOutput.FontSize,
                });

                if (ex.InnerException != null)
                {
                   fs.Spans.Add(new Span
                   {
                       Text = "--" + Environment.NewLine,
                       ForegroundColor = Color.Red,
                       FontFamily = LoggerOutput.FontFamily,
                       FontSize = LoggerOutput.FontSize,
                   }); 
                }

                ex = ex.InnerException;
            }
            return fs;
        }
    }
}

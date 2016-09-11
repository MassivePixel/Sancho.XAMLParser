// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;
using Sancho.DOM.XamarinForms;
using Sancho.XAMLParser;
using TabletDesigner.Helpers;
using Xamarin.Forms;

namespace TabletDesigner
{
    public enum EditorArea
    {
        XAML,
        JSON
    }

    public partial class TabletDesignerPage : ContentPage
    {
        ILogAccess logAccess;
        EditorArea currentArea;

        JsonModel model;

        public TabletDesignerPage()
        {
            InitializeComponent();
            logAccess = DependencyService.Get<ILogAccess>();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            editor.Text = Settings.Xaml;
            HandleJsonChanges(Settings.Json);
        }

        void Editor_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            switch (this.currentArea)
            {
                case EditorArea.XAML:
                    Settings.Xaml = editor.Text;
                    HandleXamlChanges(e.NewTextValue);
                    break;

                case EditorArea.JSON:
                    Settings.Json = editor.Text;
                    HandleJsonChanges(e.NewTextValue);
                    break;
            }
        }

        void HandleXamlChanges(string text)
        {
            try
            {
                logAccess.Clear();
                var parser = new Parser();
                var rootNode = parser.Parse(text);

                rootNode = new ContentNodeProcessor().Process(rootNode);
                rootNode = new ExpandedPropertiesProcessor().Process(rootNode);

                var dom = new XamlDOMCreator().CreateNode(rootNode);
                if (dom is View)
                    Root.Content = (View)dom;
                else if (dom is ContentPage)
                    Root.Content = ((ContentPage)dom).Content;

                if (Root.Content != null && model != null)
                    Root.Content.BindingContext = model;

                LoggerOutput.FormattedText = FormatLog(logAccess.Log);
                LoggerOutput.TextColor = Color.White;
            }
            catch (Exception ex)
            {
                LoggerOutput.FormattedText = FormatException(ex);
                LoggerOutput.TextColor = Color.FromHex("#FF3030");
            }
        }

        void HandleJsonChanges(string text)
        {
            try
            {
                logAccess.Clear();

                model = JsonModel.Parse(text);
                if (Root.Content != null)
                    Root.Content.BindingContext = model;
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

        void BtnXaml_Clicked(object sender, EventArgs args)
        {
            editor.TextChanged -= Editor_TextChanged;
            editor.Text = Settings.Xaml;
            currentArea = EditorArea.XAML;
            editor.TextChanged += Editor_TextChanged;
        }

        void BtnJson_Clicked(object sender, EventArgs args)
        {
            editor.TextChanged -= Editor_TextChanged;
            editor.Text = Settings.Json;
            currentArea = EditorArea.JSON;
            editor.TextChanged += Editor_TextChanged;
        }
    }
}

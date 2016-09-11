// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;
using Xamarin.Forms;
using Xunit;

namespace XamarinFormsTests
{
    public class QuickStartTest : BaseTest
    {
        [Fact]
        public void AdvancedTest()
        {
            var xaml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ContentPage xmlns=""http://xamarin.com/schemas/2014/forms""
             xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
             x:Class=""Phoneword.MainPage"">
     <ContentPage.Padding>
        <OnPlatform x:TypeArguments=""Thickness""
                    iOS=""20, 40, 20, 20""
                    Android=""20, 20, 20, 20""
                    WinPhone=""20, 20, 20, 20"" />
     </ContentPage.Padding>
    <ContentPage.Content>
        <StackLayout VerticalOptions=""FillAndExpand""
                     HorizontalOptions=""FillAndExpand""
                     Orientation=""Vertical""
                     Spacing=""15"">
            <Label Text=""Enter a Phoneword:"" />
            <Entry x:Name=""phoneNumberText"" Text=""1-855-XAMARIN"" />
            <Button x:Name=""translateButon"" Text=""Translate"" Clicked=""OnTranslate"" />
            <Button x:Name=""callButton"" Text=""Call"" IsEnabled=""false"" Clicked=""OnCall"" />
        </StackLayout>
    </ContentPage.Content>
</ContentPage>";
            var page = ParseVisualElement<ContentPage>(xaml);

            Assert.NotNull(page);
            Assert.Equal(new Thickness(20, 40, 20, 20), page.Padding);

            var sl = page.Content as StackLayout;
            Assert.NotNull(sl);
            Assert.Equal(LayoutOptions.FillAndExpand, sl.VerticalOptions);
            Assert.Equal(LayoutOptions.FillAndExpand, sl.HorizontalOptions);
            Assert.Equal(StackOrientation.Vertical, sl.Orientation);
            Assert.Equal(15, sl.Spacing);

            var label = sl.Children[0] as Label;
            Assert.Equal("Enter a Phoneword:", label.Text);

            var entry = sl.Children[1] as Entry;
            Assert.Equal("1-855-XAMARIN", entry.Text);

            var btn1 = sl.Children[2] as Button;
            Assert.Equal("Translate", btn1.Text);

            var btn2 = sl.Children[3] as Button;
            Assert.Equal("Call", btn2.Text);
            Assert.Equal(false, btn2.IsEnabled);
        }
    }
}


// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using Xamarin.Forms;
using Xunit;

namespace DeserializationTests
{
    public class ControlTests : BaseTest
    {
        [Fact]
        public void ParseButton()
        {
            Assert.IsType<Button>(ParseVisualElement("<Button />"));
        }

        [Fact]
        public void ParseLabelWithContent()
        {
            var label = ParseVisualElement<Label>("<Label>hello</Label>");
            Assert.NotNull(label);
            Assert.Equal("hello", label.Text);
        }

        [Fact]
        public void ParseChildren()
        {
            var xaml = @"
<StackLayout>
    <Button />
    <Button />
</StackLayout>
";
            var root = ParseVisualElement(xaml);

            Assert.IsType<StackLayout>(root);

            var sl = (StackLayout)root;
            Assert.Equal(2, sl.Children.Count);
            Assert.IsType<Button>(sl.Children[0]);
            Assert.IsType<Button>(sl.Children[1]);
        }

        [Fact]
        public void ParseChildrenChildren()
        {
            var xaml = @"
<StackLayout>
    <StackLayout>
        <Button />
    </StackLayout>
</StackLayout>
";
            var root = ParseVisualElement(xaml);

            Assert.IsType<StackLayout>(root);

            var sl = (StackLayout)root;
            Assert.Equal(1, sl.Children.Count);
            Assert.IsType<StackLayout>(sl.Children[0]);

            var sl2 = (StackLayout)sl.Children[0];
            Assert.Equal(1, sl2.Children.Count);
            Assert.IsType<Button>(sl2.Children[0]);
        }

        [Fact]
        public void ParseExpandedChildren()
        {
            var xaml = @"
<StackLayout>
    <StackLayout.Children>
        <Button />
        <Button />
    </StackLayout.Children>
</StackLayout>
";
            var root = ParseVisualElement(xaml);

            Assert.IsType<StackLayout>(root);

            var sl = (StackLayout)root;
            Assert.Equal(2, sl.Children.Count);
            Assert.IsType<Button>(sl.Children[0]);
            Assert.IsType<Button>(sl.Children[1]);
        }

        [Fact]
        public void ParseGridChildren()
        {
            var xaml = @"
<Grid>
    <Button />
    <Button />
</Grid>
";
            var root = ParseVisualElement(xaml);

            Assert.IsType<Grid>(root);

            var grid = (Grid)root;
            Assert.Equal(2, grid.Children.Count);
            Assert.IsType<Button>(grid.Children[0]);
            Assert.IsType<Button>(grid.Children[1]);
        }

        [Fact]
        public void ParseGridExpandedChildren()
        {
            var xaml = @"
<Grid>
    <Grid.Children>
        <Button />
        <Button />
    </Grid.Children>
</Grid>
";
            var root = ParseVisualElement(xaml);

            Assert.IsType<Grid>(root);

            var grid = (Grid)root;
            Assert.Equal(2, grid.Children.Count);
            Assert.IsType<Button>(grid.Children[0]);
            Assert.IsType<Button>(grid.Children[1]);
        }

        [Fact]
        public void ParseGridDefinitions()
        {
            var xaml = @"
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height=""*"" />
        <RowDefinition Height=""Auto"" />
        <RowDefinition Height=""3"" />
    </Grid.RowDefinitions>
</Grid>";

            var grid = ParseVisualElement(xaml) as Grid;

            Assert.Equal(3, grid.RowDefinitions.Count);
            Assert.Equal(GridUnitType.Star, grid.RowDefinitions[0].Height.GridUnitType);
            Assert.Equal(GridUnitType.Auto, grid.RowDefinitions[1].Height.GridUnitType);
            Assert.Equal(GridUnitType.Absolute, grid.RowDefinitions[2].Height.GridUnitType);
        }

        [Fact]
        public void ParseScrollViewContent()
        {
            var sv = ParseVisualElement(@"
<ScrollView>
    <Grid />
</ScrollView>") as ScrollView;
            Assert.IsType<Grid>(sv.Content);
        }

        [Fact]
        public void ParseScrollViewExpandedContent()
        {
            var sv = ParseVisualElement(@"
<ScrollView>
    <ScrollView.Content>
        <Grid />
    </ScrollView.Content>
</ScrollView>") as ScrollView;
            Assert.IsType<Grid>(sv.Content);
        }

        [Fact]
        public void ParseContentPageContent()
        {
            var contentPage = ParseVisualElement(@"
<ContentPage>
    <Grid />
</ContentPage>
            ") as ContentPage;

            Assert.IsType<Grid>(contentPage?.Content);
        }

        [Fact]
        public void ParseContentPageExpandedContent()
        {
            var contentPage = ParseVisualElement(@"
<ContentPage>
    <ContentPage.Content>
        <Grid />
    </ContentPage.Content>
</ContentPage>
            ") as ContentPage;

            Assert.IsType<Grid>(contentPage?.Content);
        }

        [Fact]
        public void ParseExpandedStringProperty()
        {
            var text = "hello";
            var label = ParseVisualElement($@"
<Label>
    <Label.Text>{text}</Label.Text>
</Label>") as Label;

            Assert.Equal(text, label?.Text);
        }

        [Fact]
        public void ParseExpandedColorProperty()
        {
            var text = "Red";
            var label = ParseVisualElement($@"
<Label>
    <Label.TextColor>{text}</Label.TextColor>
</Label>") as Label;

            Assert.Equal(Color.Red, label?.TextColor);
        }

        [Fact]
        public void ParseDataTemplate()
        {
            var dt = ParseVisualElement(@"
<DataTemplate>
    <Grid />
</DataTemplate>") as DataTemplate;
            Assert.IsType<Grid>(dt.CreateContent());
        }

        [Fact]
        public void ParseItemTemplate()
        {
            CheckParseItemTemplate(ParseVisualElement<ListView>(@"
<ListView>
    <ListView.ItemTemplate>
        <DataTemplate>
            <ViewCell>
                <Grid />
            </ViewCell>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>"));
        }

        [Fact]
        public void ParseItemTemplateExplicitViewCellVie()
        {
            CheckParseItemTemplate(ParseVisualElement<ListView>(@"
<ListView>
    <ListView.ItemTemplate>
        <DataTemplate>
            <ViewCell>
                <ViewCell.View>
                    <Grid />
                </ViewCell.View>
            </ViewCell>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>"));
        }

        void CheckParseItemTemplate(ListView listView)
        {
            Assert.NotNull(listView);
            Assert.NotNull(listView?.ItemTemplate);
            var content = listView?.ItemTemplate?.CreateContent() as ViewCell;
            Assert.IsType<ViewCell>(content);
            Assert.IsType<Grid>(content.View);
        }
    }
}

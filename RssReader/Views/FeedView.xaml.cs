//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------

using RssReader.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace RssReader.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FeedView : Page
    {
        public MainViewModel ViewModel => AppShell.Current.ViewModel;

        public FeedView()
        {
            this.InitializeComponent();
            ViewModel.Initialized += (s, e) =>
            {
                // Realize the UI elements marked x:DeferLoadStrategy="Lazy". 
                // Deferred loading ensures that these elements do not appear 
                // in the UI before the feed data is available.
                FindName("FeedErrorMessage");
                FindName("FavoritesIsEmptyMessage");
            };
        }

        private void ArticlesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Ensures that PropertyChanged will be raised even if the clicked article is already current.
            // This ensures that clicking an article in master-only view will always navigate to details-only view. 
            if (ViewModel.CurrentArticle == e.ClickedItem as ArticleViewModel)
            {
                ViewModel.OnPropertyChanged(nameof(ViewModel.CurrentArticle));
            }
        }

        private void ToggleButton_Toggled(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleButton;
            var article = toggle.DataContext as ArticleViewModel;

            if (toggle.IsChecked.Value) ViewModel.FavoritesFeed.Articles.Add(article);
            else
            {
                ViewModel.FavoritesFeed.Articles.Remove(article);

                // Save only for removals. Adds are automatically saved via 
                // CollectionChanged handler in MainViewModel.InitializeFeedsAsync.
                var withoutAwait = ViewModel.SaveFavoritesAsync();
            }
        }

        private async void RefreshFeed_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.CurrentFeed.RefreshAsync();
        }

        private void RemoveFeed_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RemoveBadFeed();
        }
    }
}

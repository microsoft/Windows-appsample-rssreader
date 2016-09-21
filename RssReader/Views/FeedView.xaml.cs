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
    /// Represents the UI for viewing a list of articles from a feed. 
    /// </summary>
    public sealed partial class FeedView : Page
    {
        /// <summary>
        /// Gets the MainViewModel used by the app. 
        /// </summary>
        public MainViewModel ViewModel => AppShell.Current.ViewModel;

        /// <summary>
        /// Initializes a new instance of the FeedView class. 
        /// </summary>
        public FeedView()
        {
            this.InitializeComponent();
            ViewModel.Initialized += (s, e) =>
            {
                // Realize the UI elements marked x:DeferLoadStrategy="Lazy". 
                // Deferred loading ensures that these elements do not appear 
                // in the UI before the feed data is available.
                FindName("NormalFeedView");
                FindName("FeedErrorMessage");
                FindName("FavoritesIsEmptyMessage");
            };
        }

        /// <summary>
        /// Sets the ViewModel.CurrentArticle property to the clicked item. 
        /// </summary>
        /// <remarks>
        /// The ArticlesListView.ItemsSource property must be bound to a property
        /// of type Object, so it is bound to ViewModel.CurrentArticleAsObject.
        /// Making this a two-way binding would require a CurrentArticleAsObject
        /// setter that updates CurrentArticle. However, CurrentArticle must
        /// raise the PropertyChanged event with every setter call (not just ones 
        /// that change its value), which causes an infinite recursion. The easiest
        /// way to prevent this is to use a one-way binding, and update CurrentArticle
        /// in this ItemClick event handler. 
        /// </remarks>
        private void ArticlesListView_ItemClick(object sender, ItemClickEventArgs e) => 
            ViewModel.CurrentArticle = e.ClickedItem as ArticleViewModel;

        /// <summary>
        /// Updates the favorites list when the user stars or unstars an article. 
        /// </summary>
        private void ToggleButton_Toggled(object sender, RoutedEventArgs e) =>
            ViewModel.SyncFavoritesFeed(((ToggleButton)sender).DataContext as ArticleViewModel);

    }
}

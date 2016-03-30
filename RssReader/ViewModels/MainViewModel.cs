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

using RssReader.Common;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace RssReader.ViewModels
{
    public class MainViewModel : BindableBase
    {
        public MainViewModel()
        {
            Feeds = new ObservableCollection<FeedViewModel>();
            Feeds.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasNoFeeds));
        }

        public ObservableCollection<FeedViewModel> Feeds { get; }
        public FeedViewModel FavoritesFeed => Feeds.FirstOrDefault();
        public bool HasNoFeeds => Feeds.Count == 1;
        public bool IsCurrentFeedFavoritesFeed => CurrentFeed == FavoritesFeed;
        public async Task SaveFeedsAsync() => await Feeds.SaveAsync();
        public async Task SaveFavoritesAsync() => await Feeds.SaveFavoritesAsync();

        private FeedViewModel _currentFeed;
        public FeedViewModel CurrentFeed
        {
            get { return _currentFeed; }
            set
            {
                if (SetProperty(ref _currentFeed, value))
                {
                    OnPropertyChanged(nameof(CurrentFeedAsObject));
                    OnPropertyChanged(nameof(IsCurrentFeedFavoritesFeed));
                    if (_currentFeed.Articles.Count > 0)
                    {
                        CurrentArticle = _currentFeed.Articles.First();
                    }
                    else
                    {
                        NotifyCollectionChangedEventHandler handler = null;
                        handler = (s, e) =>
                        {
                            if (e.Action == NotifyCollectionChangedAction.Add)
                            {
                                _currentFeed.Articles.CollectionChanged -= handler;
                                CurrentArticle = _currentFeed.Articles.First();
                            }
                        };
                        _currentFeed.Articles.CollectionChanged += handler;
                    }
                }
            }
        }
        public object CurrentFeedAsObject
        {
            get { return CurrentFeed as object; }
            set { if (value != null) CurrentFeed = value as FeedViewModel; }
        }

        private ArticleViewModel _currentArticle;
        public ArticleViewModel CurrentArticle
        {
            get { return _currentArticle; }
            set { if (SetProperty(ref _currentArticle, value)) OnPropertyChanged(nameof(CurrentArticleAsObject)); }
        }
        public object CurrentArticleAsObject
        {
            get { return CurrentArticle as object; }
            set { if (value != null) CurrentArticle = value as ArticleViewModel; }
        }

        private bool _isHamburgerMenuVisible = true;
        public bool IsHamburgerMenuVisible
        {
            get { return _isHamburgerMenuVisible; }
            set { if (SetProperty(ref _isHamburgerMenuVisible, value)) OnPropertyChanged(nameof(IsHamburgerMenuHidden)); }
        }
        public bool IsHamburgerMenuHidden => !IsHamburgerMenuVisible;

        private bool _isFeedAddedMessageShowing;
        public bool IsFeedAddedMessageShowing
        {
            get { return _isFeedAddedMessageShowing; }
            set { SetProperty(ref _isFeedAddedMessageShowing, value); }
        }

        private static void DoAfterDelay(int millisecondsDelay, Action action)
        {
            var withoutAwait = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, 
                async () => { await Task.Delay(millisecondsDelay); action(); });
        }

        public void AddCurrentFeed()
        {
            Feeds.Add(CurrentFeed);
            IsFeedAddedMessageShowing = true;
            DoAfterDelay(5000, () => IsFeedAddedMessageShowing = false);
            var withoutAwait = SaveFeedsAsync();
        }

        public event EventHandler BadFeedRemoved;
        public void RemoveBadFeed()
        {
            var index = Feeds.IndexOf(CurrentFeed);
            Feeds.Remove(CurrentFeed);
            CurrentFeed = Feeds[Feeds.Count > index ? index : index - 1];
            var withoutAwait = SaveFeedsAsync();
            BadFeedRemoved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Populates the Feeds list and initializes the CurrentFeed and CurrentArticle properties. 
        /// </summary>
        public async Task InitializeFeedsAsync()
        {
            Feeds.Clear();
            (await FeedDataSource.GetFeedsAsync()).ForEach(feed => Feeds.Add(feed));
            CurrentFeed = Feeds[Feeds.Count == 1 ? 0 : 1];
            CurrentArticle = CurrentFeed.Articles.FirstOrDefault() ?? CurrentArticle;
            if (FavoritesFeed.Articles.Count == 0) FavoritesFeed.ErrorMessage = NO_ARTICLES_MESSAGE;
            FavoritesFeed.Articles.CollectionChanged += async (s, e) =>
            {
                // This handles list saving for both newly-starred items and for 
                // reordering of the Favorites list (which causes a Remove followed by an Add). 
                // List saving for removals due to an unstarring are handled in FeedView.xaml.cs.
                if (e.Action == NotifyCollectionChangedAction.Add) await SaveFavoritesAsync();
                FavoritesFeed.ErrorMessage = FavoritesFeed.Articles.Count > 0 ? null : NO_ARTICLES_MESSAGE;
            };
            Initialized?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Initialized;

        private const string NO_ARTICLES_MESSAGE = "There are no starred articles.";
    }
}

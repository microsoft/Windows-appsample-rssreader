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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace RssReader.ViewModels
{
    /// <summary>
    /// Represents a collection of RSS feeds and user interactions with the feeds. 
    /// </summary>
    public class MainViewModel : BindableBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class. 
        /// </summary>
        public MainViewModel()
        {
            Feeds = new ObservableCollection<FeedViewModel>();
            Feeds.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(FeedsWithFavorites));
                OnPropertyChanged(nameof(HasNoFeeds));
            };
        }

        /// <summary>
        /// Gets the feed that represents the starred articles. 
        /// </summary>
        public FeedViewModel FavoritesFeed { get; private set; }

        /// <summary>
        /// Gets the collection of RSS feeds.
        /// </summary>
        public ObservableCollection<FeedViewModel> Feeds { get; }

        /// <summary>
        /// Gets the collection of RSS feeds, including the Favorites feed in the first position. 
        /// </summary>
        public IEnumerable<FeedViewModel> FeedsWithFavorites => new[] { FavoritesFeed }.Concat(Feeds);

        /// <summary>
        /// Occurs when the MainViewModel finishes loading feed data. 
        /// </summary>
        public event EventHandler Initialized;

        /// <summary>
        /// Populates the Feeds list and initializes the CurrentFeed property. 
        /// </summary>
        public async Task InitializeFeedsAsync()
        {
            FavoritesFeed = await FeedDataSource.GetFavoritesAsync();
            Feeds.Clear();
            (await FeedDataSource.GetFeedsAsync()).ForEach(feed => Feeds.Add(feed));
            CurrentFeed = Feeds.Count == 0 ? FavoritesFeed : Feeds[0];
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

        /// <summary>
        /// Updates the FavoritesFeed when an article is starred or unstarred. 
        /// </summary>
        public void SyncFavoritesFeed(ArticleViewModel article)
        {
            if (article.IsStarred.Value) FavoritesFeed.Articles.Insert(0, article);
            else
            {
                FavoritesFeed.Articles.Remove(article);

                // Save only for removals. Adds are automatically saved via 
                // CollectionChanged handler in InitializeFeedsAsync.
                var withoutAwait = SaveFavoritesAsync();
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the feeds list (not counting the Favorites feed) is empty.
        /// </summary>
        public bool HasNoFeeds => Feeds.Count == 0;

        /// <summary>
        /// Gets a value that indicates whether the current feed is the Favorites feed.
        /// </summary>
        public bool IsCurrentFeedFavoritesFeed => CurrentFeed == FavoritesFeed;

        /// <summary>
        /// Saves the feed list (not counting the Favorites feed) to local storage. 
        /// </summary>
        public async Task SaveFeedsAsync() => await Feeds.SaveAsync();

        /// <summary>
        /// Saves the Favorites feed to local storage.
        /// </summary>
        public async Task SaveFavoritesAsync() => await FavoritesFeed.SaveFavoritesAsync();

        /// <summary>
        /// Gets or sets the feed that the user is currently interacting with.
        /// </summary>
        public FeedViewModel CurrentFeed
        {
            get { return _currentFeed; }
            set
            {
                if (SetProperty(ref _currentFeed, value))
                {
                    OnPropertyChanged(nameof(IsCurrentFeedFavoritesFeed));
                    if (_currentFeed.Articles.Count > 0)
                    {
                        CurrentArticle = _currentFeed.Articles.First();
                    }
                    else
                    {
                        // If the articles have not yet been loaded, clear CurrentArticle then
                        // wait until the articles are loaded before selecting the first one. 
                        CurrentArticle = null;
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
        private FeedViewModel _currentFeed;

        /// <summary>
        /// Retrieves feed data from the server and updates the appropriate FeedViewModel properties.
        /// </summary>
        /// <remarks>
        /// Extension methods are not bindable, so this method provides a bindable wrapper to 
        /// the FeedDataSource.RefreshAsync extension method. 
        /// </remarks>
        public void RefreshCurrentFeed() { var withoutAwait = CurrentFeed.RefreshAsync(); }

        /// <summary>
        /// Gets or sets the article that the user is currently viewing. 
        /// </summary>
        public ArticleViewModel CurrentArticle
        {
            get { return _currentArticle; }
            set
            {
                // CurrentArticle is a special case, so it doesn't use SetProperty 
                // to update the backing field, raising the PropertyChanged event
                // only when the field value changes. Instead, CurrentArticle raises
                // PropertyChanged every time the setter is called. This ensures
                // that the ListView selection is updated when changing feeds, even 
                // if the first article is the same in both feeds. It also ensures
                // that clicking an article in the narrow view will always navigate
                // to the details view, even if the article is already the current one.
                _currentArticle = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentArticleAsObject));
            }
        }
        private ArticleViewModel _currentArticle;

        /// <summary>
        /// Gets the current article as an instance of type Object. 
        /// </summary>
        public object CurrentArticleAsObject => CurrentArticle as object; 

        /// <summary>
        /// Gets or sets a value that indicates whether the app is showing the narrow, details-only view. 
        /// </summary>
        public bool IsInDetailsMode
        {
            get { return _isInDetailsMode; }
            set { SetProperty(ref _isInDetailsMode, value); }
        }
        private bool _isInDetailsMode = false;

        /// <summary>
        /// Gets or sets a value indicating whether the message about a successfully-added feed is showing. 
        /// </summary>
        public bool IsFeedAddedMessageShowing
        {
            get { return _isFeedAddedMessageShowing; }
            set { SetProperty(ref _isFeedAddedMessageShowing, value); }
        }
        private bool _isFeedAddedMessageShowing;

        /// <summary>
        /// Gets the name of the feed that the user just added to the feeds list.
        /// </summary>
        /// <remarks>
        /// This value diverges from MainViewModel.CurrentFeed as soon as the user adds
        /// the feed because CurrentFeed is automatically reset to a new FeedViewModel. 
        /// </remarks>
        public string NameOfFeedJustAdded
        {
            get { return _nameOfFeedJustAdded; }
            set { SetProperty(ref _nameOfFeedJustAdded, value); }
        }
        private string _nameOfFeedJustAdded;

        /// <summary>
        /// Adds the current feed (in the Add Feed view) to the list of saved feeds.
        /// </summary>
        public void AddCurrentFeed()
        {
            Feeds.Add(CurrentFeed);
            NameOfFeedJustAdded = CurrentFeed.Name;
            IsFeedAddedMessageShowing = true;
            Helpers.DoAfterDelay(5000, () => IsFeedAddedMessageShowing = false);
            var withoutAwait = SaveFeedsAsync();
        }

        /// <summary>
        /// Adds the current feed to the feeds list, if it hasn't been added already; otherwise, puts the feed into an error state.
        /// </summary>
        /// <returns>true if the feed was added; false if the feed has already been added.</returns>
        public bool TryAddCurrentFeed()
        {
            if (Feeds.Contains(CurrentFeed))
            {
                CurrentFeed.IsInError = true;
                CurrentFeed.ErrorMessage = ALREADY_ADDED_MESSAGE;
                return false;
            }
            else
            {
                AddCurrentFeed();
                return true;
            }
        }

        /// <summary>
        /// Removes the specified feeds from the feeds list. 
        /// </summary>
        public void RemoveFeeds(IEnumerable<FeedViewModel> feeds)
        {
            feeds.ToList().ForEach(feed => Feeds.Remove(feed));
            var withoutAwait = SaveFeedsAsync();
        }

        /// <summary>
        /// Removes the current feed (which is an error state) from the feeds list. 
        /// </summary>
        public void RemoveBadFeed()
        {
            var index = Feeds.IndexOf(CurrentFeed);
            Feeds.Remove(CurrentFeed);
            CurrentFeed = Feeds[Feeds.Count > index ? index : index - 1];
            var withoutAwait = SaveFeedsAsync();
            BadFeedRemoved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the user deletes a feed that's in an error state. 
        /// </summary>
        public event EventHandler BadFeedRemoved;

        private const string NO_ARTICLES_MESSAGE = "There are no starred articles.";
        private const string ALREADY_ADDED_MESSAGE = "This feed has already been added.";
    }
}

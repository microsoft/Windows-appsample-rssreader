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
using System.Runtime.Serialization;
using Windows.UI.Xaml.Controls;

namespace RssReader.ViewModels
{
    /// <summary>
    /// Represents an RSS feed and user interactions with the feed. 
    /// </summary>
    public class FeedViewModel : BindableBase
    {
        /// <summary>
        /// Initializes a new instance of the FeedViewModel class. 
        /// </summary>
        public FeedViewModel()
        {
            Articles = new ObservableCollection<ArticleViewModel>();
            Articles.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(IsEmpty));
                OnPropertyChanged(nameof(IsNotEmpty));
                OnPropertyChanged(nameof(IsInErrorAndEmpty));
                OnPropertyChanged(nameof(IsInErrorAndNotEmpty));
                OnPropertyChanged(nameof(IsLoadingAndNotEmpty));
            };
        }

        /// <summary>
        /// Gets or sets the URI of the feed.
        /// </summary>
        public Uri Link
        {
            get { return _link; }
            set { if (SetProperty(ref _link, value)) OnPropertyChanged(nameof(LinkAsString)); }
        }
        private Uri _link;

        /// <summary>
        /// Gets or sets a string representation of the URI of the feed.
        /// </summary>
        [IgnoreDataMember] public string LinkAsString
        {
            get { return Link?.OriginalString ?? String.Empty; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;

                if (!value.Trim().StartsWith("http://") && !value.Trim().StartsWith("https://"))
                {
                    IsInError = true;
                    ErrorMessage = NOT_HTTP_MESSAGE;
                }
                else
                {
                    Uri uri = null;
                    if (Uri.TryCreate(value.Trim(), UriKind.Absolute, out uri)) Link = uri;
                    else
                    {
                        IsInError = true;
                        ErrorMessage = INVALID_URL_MESSAGE;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the name of the feed.
        /// </summary>
        public string Name { get { return _name; } set { SetProperty(ref _name, value); } }
        private string _name;

        /// <summary>
        /// Gets a description of the feed.
        /// </summary>
        public string Description { get { return _description; } set { SetProperty(ref _description, value); } }
        private string _description;

        /// <summary>
        /// Gets the symbol that represents the feed in the navigation pane.
        /// </summary>
        public Symbol Symbol
        {
            get { return _symbol; }
            set { if (SetProperty(ref _symbol, value)) OnPropertyChanged(nameof(SymbolAsChar)); }
        }
        private Symbol _symbol = Symbol.PostUpdate;

        /// <summary>
        /// Gets a character representation of the symbol that represents the feed in the navigation pane.
        /// </summary>
        public char SymbolAsChar => (char)Symbol;

        /// <summary>
        /// Gets the collection of articles that have been loaded for this feed. 
        /// </summary>
        public ObservableCollection<ArticleViewModel> Articles { get; }

        /// <summary>
        /// Gets the articles collection as an instance of type Object. 
        /// </summary>
        public object ArticlesAsObject => Articles as object;

        /// <summary>
        /// Gets a value that indicates whether the articles collection is empty. 
        /// </summary>
        public bool IsEmpty => Articles.Count == 0;

        /// <summary>
        /// Gets a value that indicates whether the feed has at least one article.
        /// </summary>
        public bool IsNotEmpty => !IsEmpty;

        /// <summary>
        /// Gets or sets a value that indicates whether the feed represents the collection of starred articles.
        /// </summary>
        public bool IsFavoritesFeed { get; set; }

        /// <summary>
        /// Gets a value that indicates whether the feed is a normal (non-favorites) feed that isn't in error.
        /// </summary>
        public bool IsNotFavoritesOrInError => !IsFavoritesFeed && !IsInError;

        /// <summary>
        /// Gets or sets the date and time of the last successful article retrieval. 
        /// </summary>
        public DateTime LastSyncDateTime { get { return _lastSyncDateTime; } set { SetProperty(ref _lastSyncDateTime, value); } }
        private DateTime _lastSyncDateTime;

        /// <summary>
        /// Gets a message to display when new articles cannot be retrieved. 
        /// </summary>
        public string FeedDownMessage
        {
            get
            {
                string lastSync = LastSyncDateTime.ToString(LastSyncDateTime.Date == DateTime.Today ? "t" : "g");
                return $"It looks like this feed is down. Last synced {lastSync}. Tap here to refresh.";
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the feed is the current selection in the navigation pane. 
        /// </summary>
        [IgnoreDataMember] public bool IsSelectedInNavList
        {
            get { return _isSelectedInNavList; }
            set { SetProperty(ref _isSelectedInNavList, value); }
        }
        [IgnoreDataMember] private bool _isSelectedInNavList;

        /// <summary>
        /// Gets or sets a value that indicates whether the feed is currently loading article data. 
        /// </summary>
        [IgnoreDataMember] public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(IsInError));
                    OnPropertyChanged(nameof(IsLoadingAndNotEmpty));
                    OnPropertyChanged(nameof(IsNotFavoritesOrInError));
                    OnPropertyChanged(nameof(IsInErrorAndEmpty));
                    OnPropertyChanged(nameof(IsInErrorAndNotEmpty));
                }
            }
        }
        [IgnoreDataMember] private bool _isLoading;

        public bool IsLoadingAndNotEmpty => IsLoading && !IsEmpty;

        /// <summary>
        /// Gets or sets a value that indicates whether the feed is currently being renamed. 
        /// </summary>
        [IgnoreDataMember] public bool IsInEdit { get { return _isInEdit; } set { SetProperty(ref _isInEdit, value); } }
        [IgnoreDataMember] private bool _isInEdit;

        /// <summary>
        /// Gets or sets a value that indicates whether the feed is currently in an error state
        /// and is no longer trying to retrieve new data. 
        /// </summary>
        [IgnoreDataMember] public bool IsInError
        {
            get { return _isInError && !IsLoading; }
            set
            {
                if (SetProperty(ref _isInError, value))
                {
                    OnPropertyChanged(nameof(IsNotFavoritesOrInError));
                    OnPropertyChanged(nameof(IsInErrorAndEmpty));
                    OnPropertyChanged(nameof(IsInErrorAndNotEmpty));
                }
            }
        }
        [IgnoreDataMember] private bool _isInError;

        /// <summary>
        /// Gets a value that indicates whether the feed is both in error and has no articles. 
        /// </summary>
        public bool IsInErrorAndEmpty => IsInError && IsEmpty;

        /// <summary>
        ///  Gets a value that indicates whether the feed is in error, but has already retrieved some articles. 
        /// </summary>
        public bool IsInErrorAndNotEmpty => IsInError && !IsEmpty;

        /// <summary>
        /// Gets or sets the description of the current error, if the feed is in an error state. 
        /// </summary>
        [IgnoreDataMember] public string ErrorMessage { get { return _errorMessage; } set { SetProperty(ref _errorMessage, value); } }
        [IgnoreDataMember] public string _errorMessage;

        /// <summary>
        /// Determines whether the specified object is equal to the current object. 
        /// </summary>
        public override bool Equals(object obj) => 
            obj is FeedViewModel ? (obj as FeedViewModel).GetHashCode() == GetHashCode() : false;

        /// <summary>
        /// Returns the hash code of the FeedViewModel, which is based on 
        /// a string representation the Link value, using only the host and path.  
        /// </summary>
        public override int GetHashCode() => 
            Link?.GetComponents(UriComponents.Host | UriComponents.Path, UriFormat.Unescaped).GetHashCode() ?? 0;

        private const string NOT_HTTP_MESSAGE = "Sorry. The URL must begin with http:// or https://";
        private const string INVALID_URL_MESSAGE = "Sorry. That is not a valid URL.";
    }
}

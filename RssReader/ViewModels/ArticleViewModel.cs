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

namespace RssReader.ViewModels
{
    /// <summary>
    /// Represents an article in an RSS feed and user interactions with the article. 
    /// </summary>
    public class ArticleViewModel : BindableBase
    {
        /// <summary>
        /// Gets or sets the title of the article.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a summary describing the article contents. 
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets a string that indicates the article author(s). 
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the URI of the article. 
        /// </summary>
        public Uri Link { get; set; }

        /// <summary>
        /// Gets or sets the date that the article was published. 
        /// </summary>
        public DateTimeOffset PublishedDate { get; set; }

        /// <summary>
        /// Gets a formatted version of the article's publication date. 
        /// </summary>
        public string PublishedDateFormatted => PublishedDate.ToString("MMM dd, yyyy    h:mm tt").ToUpper();

        /// <summary>
        /// Updates the FavoritesFeed when an article is starred or unstarred. 
        /// </summary>
        public void SyncFavoritesFeed() => AppShell.Current.ViewModel.SyncFavoritesFeed(this);

        /// <summary>
        /// Determines whether the specified object is equal to the current object. 
        /// </summary>
        public override bool Equals(object obj) => 
            obj is ArticleViewModel ? (obj as ArticleViewModel).GetHashCode() == GetHashCode() : false;

        /// <summary>
        /// Returns the hash code of the ArticleViewModel, which is based on 
        /// a string representation the Link value, using only the host and path.  
        /// </summary>
        public override int GetHashCode() => 
            Link.GetComponents(UriComponents.Host | UriComponents.Path, UriFormat.Unescaped).GetHashCode();

        /// <summary>
        /// Gets or sets a value that indicates whether the user has starred the article. 
        /// </summary>
        public bool? IsStarred { get { return _isStarred; } set { SetProperty(ref _isStarred, value); } }
        private bool? _isStarred = false;
    }
}

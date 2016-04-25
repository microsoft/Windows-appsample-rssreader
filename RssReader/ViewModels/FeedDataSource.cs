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
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.Web.Syndication;

namespace RssReader.ViewModels
{
    public static class FeedDataSource
    {
        /// <summary>
        /// Gets the initial set of feeds, either from local storage or 
        /// from the app package if there is nothing in local storage.
        /// </summary>
        public static async Task<List<FeedViewModel>> GetFeedsAsync()
        {
            var feeds = new List<FeedViewModel>();

            var favoritesFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("favorites.dat") as StorageFile;
            if (favoritesFile != null)
            {
                var buffer = await FileIO.ReadBufferAsync(favoritesFile);
                feeds.Add(Serializer.Deserialize<FeedViewModel>(buffer.ToArray()));
            }
            else
            {
                var feed = new FeedViewModel
                {
                    Name = "Favorites",
                    Description = "Articles that you've starred",
                    Symbol = Symbol.OutlineStar, 
                    Link = new Uri("http://localhost"),
                    IsFavoritesFeed = true
                };
                feeds.Add(feed);
            }

            var feedFile =
                await ApplicationData.Current.LocalFolder.TryGetItemAsync("feeds.dat") as StorageFile ??
                await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/feeds.dat"));
            if (feedFile != null)
            {
                var bytes = (await FileIO.ReadBufferAsync(feedFile)).ToArray();
                var feedData = Serializer.Deserialize<string[][]>(bytes);
                foreach (var feed in feedData) feeds.Add(new FeedViewModel { Name = feed[0], Link = new Uri(feed[1]) });
            }

            return feeds;
        }

        /// <summary>
        /// Retrieves feed data from the server and updates the appropriate FeedViewModel properties.
        /// </summary>
        public static async Task RefreshAsync(this FeedViewModel feedViewModel)
        {
            if (feedViewModel.Link.Host == "localhost" ||
                (feedViewModel.Link.Scheme != "http" && feedViewModel.Link.Scheme != "https")) return;

            feedViewModel.IsInError = false;
            feedViewModel.ErrorMessage = null;
            feedViewModel.IsLoading = true;
            feedViewModel.Symbol = Symbol.PostUpdate;

            Func<Task<bool>> tryGetFeed = async () =>
            { 
                try
                {
                    var feed = await new SyndicationClient().RetrieveFeedAsync(feedViewModel.Link);

                    feedViewModel.Name = String.IsNullOrEmpty(feedViewModel.Name) ? feed.Title.Text : feedViewModel.Name;
                    feedViewModel.Description = feed.Subtitle?.Text ?? String.Empty;

                    feed.Items.Select(item => new ArticleViewModel
                    {
                        Title = item.Title.Text,
                        Summary = item.Summary == null ? string.Empty :
                            item.Summary.Text.RegexRemove("\\&.{0,4}\\;").RegexRemove("<.*?>"),
                        Author = item.Authors.Select(a => a.NodeValue).FirstOrDefault(),
                        Link = item.ItemUri ?? item.Links.Select(l => l.Uri).FirstOrDefault(),
                        PublishedDate = item.PublishedDate
                    })
                    .ToList().ForEach(article =>
                    {
                        var favorites = AppShell.Current.ViewModel.FavoritesFeed;
                        var existingCopy = favorites.Articles.FirstOrDefault(a => a.Equals(article));
                        article = existingCopy ?? article;
                        if (!feedViewModel.Articles.Contains(article)) feedViewModel.Articles.Add(article);
                    });
                    return true;
                }
                catch (Exception)
                {
                    if (feedViewModel.Articles.Count == 0)
                    {
                        feedViewModel.IsInError = true;
                        feedViewModel.ErrorMessage = "Hmm... Are you sure this is an RSS URL?";
                    }
                    else
                    {
                        // TODO display error state that doesn't replace articles that have already been retrieved.
                    }
                    return false;
                }
            };

            int numberOfAttempts = 5;
            bool success = false;
            do { success = await tryGetFeed(); }
            while (!success && numberOfAttempts-- > 0);

            feedViewModel.IsLoading = false;
        }

        /// <summary>
        /// Saves the favorites feed (the first feed of the feeds list) to local storage. 
        /// </summary>
        public static async Task SaveFavoritesAsync(this IEnumerable<FeedViewModel> feeds)
        {
            var favorites = feeds.First();
            var file = await ApplicationData.Current.LocalFolder
                .CreateFileAsync("favorites.dat", CreationCollisionOption.ReplaceExisting);
            byte[] array = Serializer.Serialize(favorites);
            await FileIO.WriteBytesAsync(file, array);
        }

        /// <summary>
        /// Saves the feed data (not including the Favorites feed) to local storage. 
        /// </summary>
        public static async Task SaveAsync(this IEnumerable<FeedViewModel> feeds)
        {
            var file = await ApplicationData.Current.LocalFolder
                .CreateFileAsync("feeds.dat", CreationCollisionOption.ReplaceExisting);
            byte[] array = Serializer.Serialize(feeds.Skip(1).Select(feed => new[] { feed.Name, feed.Link.ToString() }).ToArray());
            await FileIO.WriteBytesAsync(file, array);
        }
    }
}

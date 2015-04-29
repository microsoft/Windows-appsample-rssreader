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

using RSSReader.Common;
using RSSReader.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.Web.Syndication;

namespace RSSReader.ViewModel
{
    class FeedsArticlesViewModel : BindableBase
    {

        private StorageFolder storageFolder { get; set; }
        private HashSet<RssArticleModel> favoriteArticlesHash { get; set; }
        public ICommand AddFeedCommand { protected set; get; }
        public ICommand DeleteFeedCommand { protected set; get; }
        public ICommand OpenUrlCommand { protected set; get; }
        public ICommand AddFavoriteCommand { protected set; get; }
        public ICommand DeleteFavoriteCommand { protected set; get; }


        private Visibility invalidRSSFeedMessageVisibility = Visibility.Collapsed;
        public Visibility InvalidRSSFeedMessageVisibility
        {
            get
            {
                return this.invalidRSSFeedMessageVisibility;
            }
            set
            {
                SetProperty<Visibility>(ref invalidRSSFeedMessageVisibility, value);
            }
        }

        private ObservableCollection<RssFeedModel> feedsList;
        public ObservableCollection<RssFeedModel> FeedsList
        {
            get
            {
                return this.feedsList;
            }
            set
            {
                SetProperty<ObservableCollection<RssFeedModel>>(ref feedsList, value);
            }
        }

        private RssFeedModel selectedFeed;
        public RssFeedModel SelectedFeed
        {
            get
            {
                return this.selectedFeed;
            }
            set
            {
                SetProperty<RssFeedModel>(ref selectedFeed, value);

                if (selectedFeed != null)
                {
                    foreach (RssArticleModel article in selectedFeed.ArticleList)
                    {
                        if (this.favoriteArticlesHash.Contains(article))
                        {
                            article.IsFavoritable = Visibility.Collapsed;
                            article.IsDeletable = Visibility.Visible;
                        }
                        else
                        {
                            article.IsFavoritable = Visibility.Visible;
                            article.IsDeletable = Visibility.Collapsed;
                        }
                    }
                }
                
            }
        }

        private int selectedIndex;
        public int SelectedIndex
        {
            get
            {
                return this.selectedIndex;
            }
            set
            {
                SetProperty<int>(ref selectedIndex, value);
            }
        }

        private RssFeedModel favoritesFeed;
        public RssFeedModel FavoritesFeed
        {
            get
            {
                return this.favoritesFeed;
            }
            set
            {
                SetProperty<RssFeedModel>(ref favoritesFeed, value);
            }
        }

        private RssArticleModel selectedArticle;
        public RssArticleModel SelectedArticle
        {
            get
            {

                return this.selectedArticle;
            }
            set
            {
                SetProperty<RssArticleModel>(ref selectedArticle, value);
            }
        }

        private string rssFeedToLoad= "http://blogs.windows.com/buildingapps/feed/";
        public string RssFeedToLoad
        {
            get
            {
                return this.rssFeedToLoad;
            }
            set
            {
                SetProperty<string>(ref rssFeedToLoad, value);
                InvalidRSSFeedMessageVisibility = Visibility.Collapsed;
            }
        }
         
        public FeedsArticlesViewModel()
        {
            storageFolder = ApplicationData.Current.LocalFolder;

            AddFeedCommand = new DelegateCommand(async () =>
            {
                await AddRssFeed(RssFeedToLoad);
                await SaveFeedsData();

            });

            this.DeleteFeedCommand = new DelegateCommand<RssFeedModel>(async feed =>
            {
                if (feed != null)
                {
                    FeedsList.Remove(feed);
                    await SaveFeedsData();
               }
            });

            this.AddFavoriteCommand = new DelegateCommand<RssArticleModel>(async article =>
            {
                if (article != null)
                {
                    FavoritesFeed.ArticleList.Add(article);

                    if (!this.favoriteArticlesHash.Contains(article))
                        this.favoriteArticlesHash.Add(article);

                    article.IsFavoritable = Visibility.Collapsed;
                    article.IsDeletable = Visibility.Visible;

                    await SaveFavoritesData();
                }
            });

            this.DeleteFavoriteCommand = new DelegateCommand<RssArticleModel>(async article =>
            {
                if (article != null)
                {
                    if (this.favoriteArticlesHash.Contains(article))
                        this.favoriteArticlesHash.Remove(article);

                    article.IsDeletable = Visibility.Collapsed;
                    article.IsFavoritable = Visibility.Visible;

                    FavoritesFeed.ArticleList.Remove(article);
                    await SaveFavoritesData();
                }
            });

            this.OpenUrlCommand = new DelegateCommand<RssArticleModel>(async article =>
            {
                if(article != null)
                    await Windows.System.Launcher.LaunchUriAsync(article.Link);
            });

            this.feedsList = new ObservableCollection<RssFeedModel>();
            this.favoriteArticlesHash = new HashSet<RssArticleModel>();
        }


        private async Task AddRssFeed(string url)
        {
            try
            {
                SyndicationClient client = new SyndicationClient();

                Uri myUri;
                if (Uri.TryCreate(url, UriKind.Absolute, out myUri))
                {
                    if ((myUri.Scheme == "http" || myUri.Scheme == "https"))
                    {
                        SyndicationFeed feed = await client.RetrieveFeedAsync(new Uri(url));

                        RssFeedModel feedModel = new RssFeedModel(feed.Title.Text, feed.Subtitle != null ? feed.Subtitle.Text : "", new Uri(url), null);

                        feedModel.ArticleList = new ObservableCollection<RssArticleModel>(feed.Items.Select(f =>
                                 new RssArticleModel(f.Title.Text,
                                     f.Summary != null ? Regex.Replace(Regex.Replace(f.Summary.Text, "\\&.{0,4}\\;", string.Empty), "<.*?>", string.Empty) : "",
                                     f.Authors.Select(a => a.NodeValue).FirstOrDefault(),
                                     f.ItemUri != null ? f.ItemUri : f.Links.Select(l => l.Uri).FirstOrDefault()
                                     )));

                        if (FeedsList.Contains(feedModel))
                        {
                            FeedsList.Remove(feedModel);
                        }

                        FeedsList.Add(feedModel);
                    }
                }
            }
            catch (Exception)
            {
                InvalidRSSFeedMessageVisibility = Visibility.Visible;
            }
        }
        

        public async Task Refresh()
        {
            await RefreshFeedsList();
        }

        private async Task RefreshFeedsList()
        {
            var feedItem = await storageFolder.TryGetItemAsync("feedurls.dat");
            var favoriteItem = await storageFolder.TryGetItemAsync("favorites.dat");

            if (favoriteItem != null)
            {
                var file = await storageFolder.GetFileAsync("favorites.dat");
                var array = await FileIO.ReadBufferAsync(file);

                this.FavoritesFeed = ((RssFeedModel)deserialize(array.ToArray(), typeof(RssFeedModel)));
                this.FeedsList.Add(this.FavoritesFeed);
            }
            else
            {
                this.FavoritesFeed = new RssFeedModel("Favorites list", "This contains your favorite articles", new Uri("http://localhost"), new ObservableCollection<RssArticleModel>());
                this.FavoritesFeed.IsDeletable = Visibility.Collapsed;
                this.FeedsList.Add(this.favoritesFeed);
                await SaveFavoritesData();
            }

            if (feedItem != null)
            {
                var file = await storageFolder.GetFileAsync("feedurls.dat");
                var array = await FileIO.ReadBufferAsync(file);

                string[] feedUrls = ((string[])deserialize(array.ToArray(), typeof(string[])));

                foreach (string url in feedUrls)
                {
                    if (url != "http://localhost/")
                        AddRssFeed(url);
                }
            }

            foreach(RssArticleModel article in this.favoritesFeed.ArticleList)
            {
                this.favoriteArticlesHash.Add(article);
            }
        }

        private async Task SaveFavoritesData()
        {
            RssFeedModel favorites = FeedsList.Where(f => f.Link.Equals(new Uri("http://localhost"))).FirstOrDefault();
            byte[] array = serialize(favorites, typeof(RssFeedModel));
            var file = await storageFolder.CreateFileAsync("favorites.dat", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(file, array);
        }

        private async Task SaveFeedsData()
        {
            byte[] array = serialize(this.FeedsList.Where(f => f.Link.ToString() != "http://localhost").Select(f => f.Link.ToString()).ToArray(), typeof(string[]));
            var file = await storageFolder.CreateFileAsync("feedurls.dat", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(file, array);
        }

        private static byte[] serialize(object obj, Type type)
        {
            MemoryStream stream = new MemoryStream();
            DataContractSerializer dcs = new DataContractSerializer(type);
            dcs.WriteObject(stream, obj);
            return stream.ToArray();
        }

        private static object deserialize(byte[] buffer, Type type)
        {
            MemoryStream stream = new MemoryStream(buffer);
            DataContractSerializer dcs = new DataContractSerializer(type);
            return dcs.ReadObject(stream);
        }        
    }
}

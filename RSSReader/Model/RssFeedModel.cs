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
using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Windows.UI.Xaml;

namespace RSSReader.Model
{
    [DataContract]
    class RssFeedModel : BindableBase
    {
        [DataMember]
        private string feedName;

        [IgnoreDataMember]
        public string FeedName
        {
            get
            {
                return this.feedName;
            }

            set
            {
                SetProperty<string>(ref feedName, value);
            }
        }

        [DataMember]
        private string description;
        [IgnoreDataMember]
        public string Description
        {
            get
            {
                return this.description;
            }

            set
            {
                SetProperty<string>(ref description, value);
            }
        }
        [DataMember]
        private Uri link;
        [IgnoreDataMember]
        public Uri Link
        {
            get
            {
                return this.link;
            }

            set
            {
                SetProperty<Uri>(ref link, value);
            }
        }
        [DataMember]
        private ObservableCollection<RssArticleModel> articleList;
        [IgnoreDataMember]
        public ObservableCollection<RssArticleModel> ArticleList
        {
            get
            {
                return this.articleList;
            }

            set
            {
                SetProperty<ObservableCollection<RssArticleModel>>(ref articleList, value);
            }
        }

        [DataMember]
        private Visibility isDeletable = Visibility.Visible;
        [IgnoreDataMember]
        public Visibility IsDeletable
        {
            get
            {
                return this.isDeletable;
            }

            set
            {
                SetProperty<Visibility>(ref isDeletable, value);
            }
        }

        public RssFeedModel(string name, string desc, Uri url, ObservableCollection<RssArticleModel> articles)
        {
            this.FeedName = name;
            this.Description = desc;
            this.Link = url;
            this.ArticleList = articles;
        }

        public override bool Equals(object obj)
        {
            try
            {
                RssFeedModel other = (RssFeedModel)obj;

                return this.feedName == other.feedName && this.Description == other.Description && this.Link == other.Link;
            }
            catch(InvalidCastException e)
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return (this.feedName + this.Description + this.Link).GetHashCode();
        }
    }
}

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
using System.Runtime.Serialization;
using Windows.UI.Xaml;

namespace RSSReader.Model
{
    [DataContract]
    class RssArticleModel : BindableBase
    {
        [DataMember]
        private string articleName;
        [IgnoreDataMember]
        public string ArticleName
        {
            get
            {
                return this.articleName;
            }

            set
            {
                SetProperty<string>(ref articleName, value);
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
        private string author;
        [IgnoreDataMember]
        public string Author
        {
            get
            {
                return this.author;
            }

            set
            {
                SetProperty<string>(ref author, value);
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

        [IgnoreDataMember]
        private Visibility isDeletable = Visibility.Collapsed;
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

        [IgnoreDataMember]
        private Visibility isFavoritable = Visibility.Visible;
        [IgnoreDataMember]
        public Visibility IsFavoritable
        {
            get
            {
                return this.isFavoritable;
            }

            set
            {
                SetProperty<Visibility>(ref isFavoritable, value);
            }
        }


        public RssArticleModel(string name, string desc, string author, Uri url)
        {
            this.ArticleName = name;
            this.Description = desc;
            this.Author = author;
            this.Link = url;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RssArticleModel))
                return false;
            RssArticleModel ram = obj as RssArticleModel;
            return this.GetHashCode() == ram.GetHashCode();
        }

        public override int GetHashCode()
        {
            return (this.articleName+this.Description+this.Author+this.Link).GetHashCode();
        }
    }
}

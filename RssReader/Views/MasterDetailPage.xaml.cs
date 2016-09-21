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
using RssReader.ViewModels;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace RssReader.Views
{
    public sealed partial class MasterDetailPage : Page
    {
        private MainViewModel ViewModel => AppShell.Current.ViewModel;

        private Canceller Canceller { get; } = new Canceller();

        private bool isCurrentFeedNew = false;

        public MasterDetailPage()
        {
            InitializeComponent();

            ViewModel.PropertyChanged += ViewModel_PropertyChanged; 

            ArticleWebView.NavigationStarting += async (s, e) =>
            {
                if (e.Uri != null && !await e.LaunchBrowserForNonMatchingUriAsync(ViewModel.CurrentArticle.Link))
                {
                    // In-app navigation, so hide the WebView control and display the progress 
                    // animation until the page load is completed.
                    ArticleWebView.Visibility = Visibility.Collapsed;
                    LoadingProgressBar.Visibility = Visibility.Visible;
                }
            };

            ArticleWebView.LoadCompleted += (s, e) =>
            {
                ArticleWebView.Visibility = Visibility.Visible;
                LoadingProgressBar.Visibility = Visibility.Collapsed;
            };
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Set a flag so that, in narrow mode, details-only navigation doesn't occur if 
            // the CurrentArticle is changed solely as a side-effect of changing the CurrentFeed.
            if (e.PropertyName == nameof(ViewModel.CurrentFeed))
            {
                isCurrentFeedNew = true;
            }
            else if (e.PropertyName == nameof(ViewModel.CurrentArticle))
            {
                if (ViewModel.CurrentArticle == null)
                {
                    ArticleWebView.NavigateToString(string.Empty);
                }
                else
                {
                    if (!ViewModel.CurrentArticle.Link.IsEquivalentTo(ArticleWebView.Source))
                    {
                        ArticleWebView.Navigate(ViewModel.CurrentArticle.Link);
                    }
                    if (AdaptiveStates.CurrentState == NarrowState)
                    {
                        bool switchToDetailsView = !isCurrentFeedNew;
                        isCurrentFeedNew = false;
                        if (switchToDetailsView)
                        {
                            // Use "drill in" transition for navigating from master list to detail view
                            Frame.Navigate(typeof(DetailPage), null, new DrillInNavigationTransitionInfo());
                        }
                    }
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Uri || e.Parameter == null || 
                (e.Parameter is string && String.IsNullOrEmpty(e.Parameter as string)))
            {
                if (MasterFrame.CurrentSourcePageType != typeof(FeedView))
                {
                    MasterFrame.Navigate(typeof(FeedView));
                }

                var feedUri = e.Parameter as Uri;
                if (feedUri != null)
                {
                    ViewModel.CurrentFeed = ViewModel.FeedsWithFavorites.FirstOrDefault(f => f.Link == feedUri);
                    Canceller.Cancel();
                    var withoutAwait = ViewModel.CurrentFeed.RefreshAsync(Canceller.Token);
                }
            }
            else
            {
                var viewType = e.Parameter as Type;

                if (viewType != null && MasterFrame.CurrentSourcePageType != viewType)
                {
                    MasterFrame.Navigate(viewType);
                }

                UpdateForVisualState(AdaptiveStates.CurrentState);
            }
        }

        private void AdaptiveStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            UpdateForVisualState(e.NewState, e.OldState);
        }

        private void UpdateForVisualState(VisualState newState, VisualState oldState = null)
        {
            var isNarrow = newState == NarrowState;

            if (isNarrow && oldState == DefaultState && MasterFrame.CurrentSourcePageType == typeof(FeedView))
            {
                // Resize down to the detail item. Don't play a transition.
                Frame.Navigate(typeof(DetailPage), null, new SuppressNavigationTransitionInfo());
            }
        }

    }
}

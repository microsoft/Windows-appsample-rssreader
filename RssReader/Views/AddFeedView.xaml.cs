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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace RssReader.Views
{
    /// <summary>
    /// Represents the UI for adding a new feed to the feeds list.
    /// </summary>
    public sealed partial class AddFeedView : Page, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        /// <summary>
        /// Occurs when a property value changes. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event for the property with the specified
        /// name, or the calling property if no name is specified.
        /// </summary>
        public void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Checks whether the value of the specified field is different than the specified value, and
        /// if they are different, updates the field and raises the PropertyChanged event for
        /// the property with the specified name, or the calling property if no name is specified. 
        /// </summary>
        public bool SetProperty<T>(ref T storage, T value,
            [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion

        /// <summary>
        /// Gets the MainViewModel used by the app. 
        /// </summary>
        private MainViewModel ViewModel => AppShell.Current.ViewModel;

        /// <summary>
        /// Gets a Canceller that enables cancellation of a feed refresh attempt. 
        /// </summary>
        private Canceller Canceller => new Canceller();

        /// <summary>
        /// Gets or sets a value that indicates whether the feed URI value is valid,
        /// meaning that the Name text box and Save buttons can be enabled. 
        /// </summary>
        private bool AreFeedControlsEnabled
        {
            get { return _areFeedControlsEnabled; }
            set { SetProperty(ref _areFeedControlsEnabled, value); }
        }
        private bool _areFeedControlsEnabled;

        /// <summary>
        /// Gets a value that indicates whether the Save buttons are enabled. 
        /// </summary>
        private bool AreSaveButtonsEnabled => 
            AreFeedControlsEnabled && !String.IsNullOrWhiteSpace(NameTextBox.Text);

        /// <summary>
        /// Initializes a new instance of the AddFeedView class. 
        /// </summary>
        public AddFeedView()
        {
            this.InitializeComponent();
            Loaded += (s, e) => LinkTextBox.Focus(FocusState.Programmatic);
            NameTextBox.TextChanged += (s, e) => OnPropertyChanged(nameof(AreSaveButtonsEnabled));
        }

        /// <summary>
        /// Clears the add-feed UI so the user can enter new data. 
        /// </summary>
        private void ResetFeed()
        {
            ViewModel.CurrentFeed = new FeedViewModel();
            ViewModel.CurrentFeed.PropertyChanged += CurrentFeed_PropertyChanged;
            AreFeedControlsEnabled = false;
        }

        /// <summary>
        /// Resets the feed to remove the error state.
        /// </summary>
        private void ClearErrorState()
        {
            ViewModel.CurrentFeed.IsInError = false;
            ViewModel.CurrentFeed.ErrorMessage = string.Empty;
        }

        /// <summary>
        /// Listens for changes to the FeedViewModel in order to enable UI controls
        /// when the feed has been refreshed and to reset the feed or cancel a refresh
        /// attempt when the user modifies the feed link. 
        /// </summary>
        private void CurrentFeed_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(FeedViewModel.Name):
                    AreFeedControlsEnabled = true;
                    ViewModel.CurrentFeed.PropertyChanged -= CurrentFeed_PropertyChanged;
                    LinkTextBox.TextChanged += LinkTextBox_TextChangedAfterFeedRefresh;
                    break;
                case nameof(FeedViewModel.IsInError):
                    LinkTextBox.TextChanged += LinkTextBox_TextChangedAfterFeedError;
                    break;
                case nameof(FeedViewModel.Link):
                    var withoutAwait = ViewModel.CurrentFeed.RefreshAsync(Canceller.Token);
                    LinkTextBox.TextChanged += LinkTextBox_TextChangedAfterFeedLinkChanged;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Resets the form when the user starts to change the feed URL, since at this point
        /// the current feed data is no longer valid. Also removes this handler from the
        /// TextChanged event until the next time the feed data is loaded (via the 
        /// CurrentFeed_PropertyChanged handler attached in the ResetFeed method). 
        /// </summary>
        private void LinkTextBox_TextChangedAfterFeedRefresh(object sender, TextChangedEventArgs e)
        {
            if (LinkTextBox.Text.Trim() == ViewModel.CurrentFeed.LinkAsString) return;

            LinkTextBox.TextChanged -= LinkTextBox_TextChangedAfterFeedRefresh;

            // Preserve the text and cursor position for the user's current edits when 
            // feed is reset, which clears all feed property values including LinkAsString. 
            var currentLinkText = LinkTextBox.Text;
            int selectionStart = LinkTextBox.SelectionStart;

            ResetFeed();

            // Restore the preserved text and cursor position to the new feed instance. 
            LinkTextBox.SetValue(TextBox.TextProperty, currentLinkText);
            LinkTextBox.SelectionStart = selectionStart;
        }

        /// <summary>
        /// Resets the feed to remove the error state when the user starts to change the feed URL.
        /// </summary>
        private void LinkTextBox_TextChangedAfterFeedError(object sender, TextChangedEventArgs e)
        {
            if (LinkTextBox.Text.Trim() == ViewModel.CurrentFeed.LinkAsString) return;
            LinkTextBox.TextChanged -= LinkTextBox_TextChangedAfterFeedError;
            ClearErrorState();
        }

        /// <summary>
        /// Cancels any current feed refresh attempt and clears the error state when the user starts changing the feed URI. 
        /// </summary>
        private void LinkTextBox_TextChangedAfterFeedLinkChanged(object sender, TextChangedEventArgs e)
        {
            if (LinkTextBox.Text.Trim() == ViewModel.CurrentFeed.LinkAsString) return;
            LinkTextBox.TextChanged -= LinkTextBox_TextChangedAfterFeedLinkChanged;
            Canceller.Cancel();
            ClearErrorState();
        }

        /// <summary>
        /// Sets focus to the text box and selects the current contents.
        /// </summary>
        private void InitializeLinkTextBox()
        {
            LinkTextBox.Focus(FocusState.Programmatic);
            LinkTextBox.SelectAll();
        }

        /// <summary>
        /// Resets the UI on navigation to this page. 
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ResetFeed();
            InitializeLinkTextBox();
        }

        /// <summary>
        /// Adds the current feed to the feeds list and navigates to articles list for the feed.
        /// </summary>
        private void SaveAndLeaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.TryAddCurrentFeed())
            {
                AppShell.Current.NavigateToCurrentFeed();
            }
        }

        /// <summary>
        /// Adds the current feed to the feeds list and resets the add-feed UI so the user can add another feed. 
        /// </summary>
        private void SaveAndStayButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.TryAddCurrentFeed())
            {
                ResetFeed();
                InitializeLinkTextBox();
            }
        }

        /// <summary>
        /// Handles ESC and ENTER keypresses for the feed URI text box. 
        /// </summary>
        private void LinkTextBox_KeyDown(object sender, KeyRoutedEventArgs e) => 
            e.HandleEnterAndEscape(sender, ViewModel.CurrentFeed, () => ViewModel.CurrentFeed.LinkAsString);

        /// <summary>
        /// Handles ESC and ENTER keypresses for the feed name text box. 
        /// </summary>
        private void NameTextBox_KeyDown(object sender, KeyRoutedEventArgs e) =>
            e.HandleEnterAndEscape(sender, ViewModel.CurrentFeed, () => ViewModel.CurrentFeed.Name);

    }
}

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

using RssReader.ViewModels;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace RssReader.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddFeedView : Page, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion

        private MainViewModel ViewModel => AppShell.Current.ViewModel;

        private string _nameOfFeedJustAdded;
        public string NameOfFeedJustAdded
        {
            get { return _nameOfFeedJustAdded; }
            set { SetProperty(ref _nameOfFeedJustAdded, value); }
        }

        public AddFeedView()
        {
            this.InitializeComponent();
            Loaded += (s, e) => LinkTextBox.Focus(FocusState.Programmatic);
            NameTextBox.TextChanged += (s, e) =>
            {
                if (String.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    SaveAndStayButton.IsEnabled = false;
                    SaveAndLeaveButton.IsEnabled = false;
                }
                else
                {
                    SaveAndStayButton.IsEnabled = true;
                    SaveAndLeaveButton.IsEnabled = true;
                }
            };
        }

        private void ResetFeed()
        {
            ViewModel.CurrentFeed = new FeedViewModel();
            ViewModel.CurrentFeed.PropertyChanged += this.CurrentFeed_PropertyChanged;
            NameTextBox.IsEnabled = false;
            SaveAndLeaveButton.IsEnabled = false;
            SaveAndStayButton.IsEnabled = false;
        }

        // Enables the NameTextBox and buttons when the feed data is loaded, as indicated
        // by the feed Name value changing. Also removes this handler from the feed
        // PropertyChanged event to enable the user to customize the name, and starts 
        // listening for changes to LinkTextBox.Text in order to reset the form when 
        // the current feed data is no longer valid. 
        private void CurrentFeed_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(FeedViewModel.Name)) return;
            ViewModel.CurrentFeed.PropertyChanged -= CurrentFeed_PropertyChanged;
            LinkTextBox.TextChanged += this.LinkTextBox_TextChanged;
            NameTextBox.IsEnabled = true;
            SaveAndLeaveButton.IsEnabled = true;
            SaveAndStayButton.IsEnabled = true;
        }

        // Resets the form when the user starts to change the feed URL, since at this point
        // the current feed data is no longer valid. Also removes this handler from the
        // TextChanged event until the next time the feed data is loaded (via the 
        // CurrentFeed_PropertyChanged handler attached in the ResetFeed method). 
        private void LinkTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LinkTextBox.TextChanged -= LinkTextBox_TextChanged;

            // Preserve the text and cursor position for the user's current edits when 
            // feed is reset, which clears all feed property values including LinkAsString. 
            var currentLinkText = LinkTextBox.Text;
            int selectionStart = LinkTextBox.SelectionStart;

            ResetFeed();

            // Restore the preserved text and cursor position to the new feed instance. 
            LinkTextBox.SetValue(TextBox.TextProperty, currentLinkText);
            LinkTextBox.SelectionStart = selectionStart;
        }

        private void InitializeLinkTextBox()
        {
            LinkTextBox.Focus(FocusState.Programmatic);
            LinkTextBox.SelectAll();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ResetFeed();
            InitializeLinkTextBox();
        }

        private void SaveAndLeaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (AddCurrentFeed())
            {
                ViewModel.IsFeedAddedMessageShowing = false;
                AppShell.Current.NavigateToCurrentFeed();
            }
        }

        private void SaveAndStayButton_Click(object sender, RoutedEventArgs e)
        {
            if (AddCurrentFeed())
            {
                NameOfFeedJustAdded = ViewModel.CurrentFeed.Name;
                ResetFeed();
                InitializeLinkTextBox();
            }
        }

        private bool AddCurrentFeed()
        {
            if (ViewModel.Feeds.Contains(ViewModel.CurrentFeed))
            {
                ViewModel.CurrentFeed.IsInError = true;
                ViewModel.CurrentFeed.ErrorMessage = "This feed has already been added.";
                return false;
            }
            else
            {
                ViewModel.AddCurrentFeed();
                return true;
            }
        }

        private void HandleEnterAndEscape<T>(object sender, KeyRoutedEventArgs e, 
            object target, Expression<Func<T>> propertyExpression)
        {
            var textbox = sender as TextBox;
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ((propertyExpression.Body as MemberExpression).Member as PropertyInfo) 
                    .SetValue(target, textbox.Text);
            }
            else if (e.Key == Windows.System.VirtualKey.Escape) textbox.Text = string.Empty;
        }

        private void LinkTextBox_KeyDown(object sender, KeyRoutedEventArgs e) => 
            HandleEnterAndEscape(sender, e, ViewModel.CurrentFeed, () => ViewModel.CurrentFeed.LinkAsString);

        private void NameTextBox_KeyDown(object sender, KeyRoutedEventArgs e) =>
            HandleEnterAndEscape(sender, e, ViewModel.CurrentFeed, () => ViewModel.CurrentFeed.Name);

    }
}

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
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace RssReader.Views
{
    /// <summary>
    /// Represents the UI for editing the feeds list. 
    /// </summary>
    public sealed partial class EditFeedsView : Page, INotifyPropertyChanged
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
        /// Initializes a new instance of the EditFeedsView class. 
        /// </summary>
        public EditFeedsView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Navigates to the add-feed view. 
        /// </summary>
        private void AddFeed() => AppShell.Current.NavigateToAddFeedView();

        /// <summary>
        /// Deletes the feeds that are currently selected in the edit feeds list. 
        /// </summary>
        private void DeleteSelectedFeeds() => 
            ViewModel.RemoveFeeds(EditFeedsList.SelectedItems.Cast<FeedViewModel>());

        /// <summary>
        /// Puts the selected feed into edit mode so the user can rename it. 
        /// </summary>
        private void EditFeed()
        {
            (EditFeedsList.SelectedItem as FeedViewModel).IsInEdit = true;
            var item = EditFeedsList.ContainerFromIndex(EditFeedsList.SelectedIndex) as ListViewItem;
            var textbox = (item.ContentTemplateRoot as Grid).FindName("EditTextBox") as TextBox;
            textbox.Focus(FocusState.Programmatic);
            textbox.SelectAll();
        }

        /// <summary>
        /// Leaves edit mode and saves the new name for the feed. 
        /// </summary>
        private void EndEdit(object sender, RoutedEventArgs e)
        {
            (EditFeedsList.SelectedItem as FeedViewModel).IsInEdit = false;
            var withoutAwait = ViewModel.SaveFeedsAsync();
        }

        /// <summary>
        /// Gets a value that indicates whether a single feed has been selected,
        /// meaning that it can be put into edit mode. 
        /// </summary>
        public bool CanEdit => EditFeedsList.SelectedItems.Count == 1;

        /// <summary>
        /// Raises PropertyChanged for the CanEdit property when the selection changes 
        /// in the edit-feeds list; this enables or disables the Edit button, as appropriate. 
        /// </summary>
        private void SelectionChanged() => OnPropertyChanged(nameof(CanEdit));

        /// <summary>
        /// Handles ESC and ENTER keypresses for the feed-rename text box. 
        /// </summary>
        private void EditTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var textbox = sender as TextBox;
            if (e.Key == VirtualKey.Escape) textbox.Text = string.Empty;
            else if (e.Key == VirtualKey.Enter)
            {
                EndEdit(this, null);
                e.Handled = true;
            }
        }
    }
}


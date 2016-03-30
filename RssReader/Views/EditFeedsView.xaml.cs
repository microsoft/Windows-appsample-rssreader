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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace RssReader.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditFeedsView : Page, INotifyPropertyChanged
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

        public ObservableCollection<FeedViewModel> FeedsWithoutFavorites { get; }

        public EditFeedsView()
        {
            this.InitializeComponent();
            FeedsWithoutFavorites = new ObservableCollection<FeedViewModel>(ViewModel.Feeds.Skip(1));

            // List reordering raises this event twice, first with Remove and then with Add.
            // This handler deals with reordering changes only, so it saves the list only after the Add step.  
            FeedsWithoutFavorites.CollectionChanged += (s, e) =>
            {
                if (suppressSync) return;

                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    ViewModel.Feeds.Remove(e.OldItems[0] as FeedViewModel);
                }
                else if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    ViewModel.Feeds.Insert(e.NewStartingIndex + 1, e.NewItems[0] as FeedViewModel);
                    var withoutAwait = ViewModel.SaveFeedsAsync();
                }
            };
        }

        private bool suppressSync;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (ViewModel.Feeds.Count - 1 != FeedsWithoutFavorites.Count)
            {
                suppressSync = true;
                FeedsWithoutFavorites.Clear();
                ViewModel.Feeds.Skip(1).ToList().ForEach(feed => FeedsWithoutFavorites.Add(feed));
                suppressSync = false;
            }
        }

        private void AddFeed() => AppShell.Current.NavigateToAddFeedView();

        private void DeleteFeed()
        {
            foreach (FeedViewModel feed in EditFeedsList.SelectedItems.ToList())
            {
                FeedsWithoutFavorites.Remove(feed);
            }
            var withoutAwait = ViewModel.SaveFeedsAsync();
        }

        private void EditFeed()
        {
            (EditFeedsList.SelectedItem as FeedViewModel).IsInEdit = true;
            var item = EditFeedsList.ContainerFromIndex(EditFeedsList.SelectedIndex) as ListViewItem;
            var textbox = (item.ContentTemplateRoot as Grid).FindName("EditTextBox") as TextBox;
            textbox.Focus(FocusState.Programmatic);
            textbox.SelectAll();
        }

        private void EndEdit(object sender, RoutedEventArgs e)
        {
            (EditFeedsList.SelectedItem as FeedViewModel).IsInEdit = false;
            var withoutAwait = ViewModel.SaveFeedsAsync();
        }

        public bool CanEdit => EditFeedsList.SelectedItems.Count == 1;

        private void SelectionChanged() => OnPropertyChanged(nameof(CanEdit));

        private void EditTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                EndEdit(this, null);
                e.Handled = true;
            }
        }
    }
}


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using RssReader.Views;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace RssReader.Controls
{
    public sealed partial class PageHeader : UserControl
    {
        private static readonly double DEFAULT_LEFT_MARGIN = 24;

        public PageHeader()
        {
            this.InitializeComponent();

            this.Loaded += (s, a) =>
            {
                double leftMargin = AppShell.Current.TogglePaneButtonRect.Right;
                leftMargin = leftMargin > 0 ? leftMargin : DEFAULT_LEFT_MARGIN;
                TitleBar.Margin = new Thickness(leftMargin, 0, 0, 0);
            };
        }

        private void Current_TogglePaneButtonSizeChanged(AppShell sender, Rect e)
        {
            // If there is no adjustment due to the toggle button, use the default left margin. 
            TitleBar.Margin = new Thickness(e.Right == 0 ? DEFAULT_LEFT_MARGIN : e.Right, 0, 0, 0);
        }

        public UIElement HeaderContent
        {
            get { return (UIElement)GetValue(HeaderContentProperty); }
            set { SetValue(HeaderContentProperty, value); }
        }

        public static readonly DependencyProperty HeaderContentProperty =
            DependencyProperty.Register("HeaderContent", typeof(UIElement), typeof(PageHeader), new PropertyMetadata(DependencyProperty.UnsetValue));
    }
}

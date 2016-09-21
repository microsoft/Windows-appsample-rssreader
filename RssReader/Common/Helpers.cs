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

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace RssReader.Common
{
    /// <summary>
    /// Encapsulates handy helper and extension methods. 
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Performs the specified action after the specified delay in milliseconds. 
        /// </summary>
        public static void DoAfterDelay(int millisecondsDelay, Action action)
        {
            var withoutAwait = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                async () => { await Task.Delay(millisecondsDelay); action(); });
        }

        /// <summary>
        /// Processes TextBox.KeyDown event data to determine whether ESC or ENTER have been pressed;
        /// for ESC, clears the text box; for ENTER, updates the target object by committing the 
        /// TextBox.Text value to the property indicated by the specified property expression.
        /// </summary>
        public static void HandleEnterAndEscape<T>(this KeyRoutedEventArgs e, object sender,
            object target, Expression<Func<T>> propertyExpression)
        {
            var textbox = sender as TextBox;
            if (e.Key == VirtualKey.Enter)
            {
                ((propertyExpression.Body as MemberExpression).Member as PropertyInfo)
                    .SetValue(target, textbox.Text);
            }
            else if (e.Key == VirtualKey.Escape) textbox.Text = string.Empty;
        }

        /// <summary>
        /// Removes regular-expression pattern matches from the 
        /// string and returns the result as a new string.
        /// </summary>
        public static String RegexRemove(this string input, string pattern) =>
            Regex.Replace(input, pattern, string.Empty);

        /// <summary>
        /// Gets a value that indicates whether the Uri has the same host and path as the specified Uri. 
        /// </summary>
        public static bool IsEquivalentTo(this Uri uri, Uri uriToMatch) =>
            Uri.Compare(uri, uriToMatch, UriComponents.Host | UriComponents.Path,
                UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) == 0;

        /// <summary>
        /// Compares the URI to the attempted WebView navigation URI.
        /// If they are different, cancels the WebView navigation, opens the URI 
        /// in the browser, and returns true; otherwise, returns false.
        /// </summary>
        public static async Task<bool> LaunchBrowserForNonMatchingUriAsync(
            this WebViewNavigationStartingEventArgs e, Uri uriToMatch)
        {
            if (e.Uri.IsEquivalentTo(uriToMatch)) return false;
            e.Cancel = true;
            await Launcher.LaunchUriAsync(e.Uri);
            return true;
        }

    }
}

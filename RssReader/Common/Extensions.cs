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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml.Controls;

namespace RssReader.Common
{
    public static class Extensions
    {
        /// <summary>
        /// Removes regular-expression pattern matches from the 
        /// string and returns the result as a new string.
        /// </summary>
        public static String RegexRemove(this string input, string pattern) => 
            Regex.Replace(input, pattern, string.Empty);

        /// <summary>
        /// Gets the string representation of a URI without the scheme.
        /// </summary>
        public static string WithoutScheme(this Uri uri) => 
            uri.ToString().Substring(uri.Scheme.Length);

        /// <summary>
        /// Compares the URI to the attempted WebView navigation URI.
        /// If they are different, cancels the WebView navigation, opens the URI 
        /// in the browser, and returns true; otherwise, returns false.
        /// </summary>
        public static async Task<bool> LaunchBrowserForNonMatchingUriAsync(
            this Uri uriToMatch, WebViewNavigationStartingEventArgs e)
        {
            if (e.Uri.WithoutScheme() == uriToMatch.WithoutScheme()) return false;
            e.Cancel = true;
            await Launcher.LaunchUriAsync(e.Uri);
            return true;
        }
    }
}

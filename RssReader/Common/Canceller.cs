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
using System.Threading;

namespace RssReader.Common
{
    /// <summary>
    /// Encapsulates the thread-cancellation mechanism. 
    /// </summary>
    public class Canceller : IDisposable
    {
        /// <summary>
        /// Gets or sets the current token source, which will 
        /// be reset on the next call to the Cancel method. 
        /// </summary>
        private CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();

        /// <summary>
        /// Gets the token from the current token source, which 
        /// you can cancel with the next call to the Cancel method. 
        /// </summary>
        public CancellationToken Token => TokenSource.Token;

        /// <summary>
        /// Cancels the current Token and resets the TokenSource.
        /// </summary>
        public void Cancel()
        {
            TokenSource.Cancel();
            var t = Token;
            TokenSource.Dispose();
            TokenSource = new CancellationTokenSource();
        }

        /// <summary>Releases resources used by the class.</summary>
        ~Canceller() { Dispose(false); }

        /// <summary>Releases resources used by this class.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases resources used by this class.</summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; 
        /// false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed) return;
            if (disposing) TokenSource.Dispose();
            _isDisposed = true;
        }
        private bool _isDisposed = false;
    }
}

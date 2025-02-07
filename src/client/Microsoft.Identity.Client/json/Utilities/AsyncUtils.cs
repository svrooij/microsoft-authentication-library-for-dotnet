﻿#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

#if HAVE_ASYNC

using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Identity.Json.Utilities
{
    internal static class AsyncUtils
    {
        // Pre-allocate to avoid wasted allocations.
        public static readonly Task<bool> False = Task.FromResult(false);
        public static readonly Task<bool> True = Task.FromResult(true);

        internal static Task<bool> ToAsync(this bool value) => value ? True : False;

        public static Task CancelIfRequestedAsync(this CancellationToken cancellationToken)
        {
            return cancellationToken.IsCancellationRequested ? FromCanceled(cancellationToken) : null;
        }

        public static Task<T> CancelIfRequestedAsync<T>(this CancellationToken cancellationToken)
        {
            return cancellationToken.IsCancellationRequested ? FromCanceled<T>(cancellationToken) : null;
        }

        // From 4.6 on we could use Task.FromCanceled(), but we need an equivalent for
        // previous frameworks.
#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods
        public static Task FromCanceled(this CancellationToken cancellationToken)
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
        {
            Debug.Assert(cancellationToken.IsCancellationRequested);
            return new Task(() => {}, cancellationToken);
        }

#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods
        public static Task<T> FromCanceled<T>(this CancellationToken cancellationToken)
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
        {
            Debug.Assert(cancellationToken.IsCancellationRequested);
            return new Task<T>(() => default, cancellationToken);
        }

        // Task.Delay(0) is optimised as a cached task within the framework, and indeed
        // the same cached task that Task.CompletedTask returns as of 4.6, but we'll add
        // our own cached field for previous frameworks.
        internal static readonly Task CompletedTask = Task.Delay(0);

        public static Task WriteAsync(this TextWriter writer, char value, CancellationToken cancellationToken)
        {
            Debug.Assert(writer != null);
            return cancellationToken.IsCancellationRequested ? FromCanceled(cancellationToken) : writer.WriteAsync(value);
        }

        public static Task WriteAsync(this TextWriter writer, string value, CancellationToken cancellationToken)
        {
            Debug.Assert(writer != null);
            return cancellationToken.IsCancellationRequested ? FromCanceled(cancellationToken) : writer.WriteAsync(value);
        }

        public static Task WriteAsync(this TextWriter writer, char[] value, int start, int count, CancellationToken cancellationToken)
        {
            Debug.Assert(writer != null);
            return cancellationToken.IsCancellationRequested ? FromCanceled(cancellationToken) : writer.WriteAsync(value, start, count);
        }

        public static Task<int> ReadAsync(this TextReader reader, char[] buffer, int index, int count, CancellationToken cancellationToken)
        {
            Debug.Assert(reader != null);
            return cancellationToken.IsCancellationRequested ? FromCanceled<int>(cancellationToken) : reader.ReadAsync(buffer, index, count);
        }

        public static bool IsCompletedSucessfully(this Task task)
        {
            // IsCompletedSucessfully is the faster method, but only currently exposed on .NET Core 2.0
#if NETCOREAPP2_0
            return task.IsCompletedSucessfully;
#else
            return task.Status == TaskStatus.RanToCompletion;
#endif
        }
    }
}

#endif

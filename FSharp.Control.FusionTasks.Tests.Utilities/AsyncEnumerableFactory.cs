/////////////////////////////////////////////////////////////////////////////////////////////////
//
// FSharp.Control.FusionTasks - F# Async workflow <--> .NET Task easy seamless interoperability library.
// Copyright (c) 2016-2021 Kouji Matsui (@kozy_kekyo, @kekyo2)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//	http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
/////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS1998
#pragma warning disable CS0162

namespace FSharp.Control.FusionTasks.Tests
{
    public static class AsyncEnumerableFactory
    {
        public static async IAsyncEnumerable<T> DelayEachAsync<T>(
            this IEnumerable<T> enumerable,
            TimeSpan delay)
        {
            foreach (var value in enumerable)
            {
                await Task.Delay(delay).ConfigureAwait(false);
                yield return value;
            }
        }

        public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(
            this IEnumerable<T> enumerable)
        {
            foreach (var value in enumerable)
            {
                yield return value;
            }
        }

        public static async IAsyncEnumerable<T> WillThrowBefore<T>(
            this IEnumerable<T> enumerable)
        {
            throw new Exception("WillThrowBefore");
            foreach (var value in enumerable)
            {
                yield return value;
            }
        }

        public static async IAsyncEnumerable<T> WillThrowAfter<T>(
            this IEnumerable<T> enumerable)
        {
            foreach (var value in enumerable)
            {
                yield return value;
            }
            throw new Exception("WillThrowAfter");
        }

        public static async IAsyncEnumerable<T> WillThrowInterBefore<T>(
            this IEnumerable<T> enumerable)
        {
            foreach (var value in enumerable)
            {
                throw new Exception("WillThrowInterBefore");
                yield return value;
            }
        }

        public static async IAsyncEnumerable<T> WillThrowInterAfter<T>(
            this IEnumerable<T> enumerable)
        {
            foreach (var value in enumerable)
            {
                yield return value;
                throw new Exception("WillThrowInterAfter");
            }
        }

        public static IAsyncEnumerable<T> HookAsyncEnumerable<T>(
            this IEnumerable<T> enumerable, Func<ValueTask> dispose) =>
            new AsyncEnumerable<T>(
                enumerable,
                enumerator => enumerator.MoveNext(),
                enumerator => enumerator.Current,
                dispose);

        private sealed class AsyncEnumerable<T> : IAsyncEnumerable<T>
        {
            private readonly IEnumerable<T> enumerable;
            private readonly Func<IEnumerator<T>, bool> moveNext;
            private readonly Func<IEnumerator<T>, T> current;
            private readonly Func<ValueTask> dispose;

            public AsyncEnumerable(
                IEnumerable<T> enumerable,
                Func<IEnumerator<T>, bool> moveNext,
                Func<IEnumerator<T>, T> current,
                Func<ValueTask> dispose)
            {
                this.enumerable = enumerable;
                this.moveNext = moveNext;
                this.current = current;
                this.dispose = dispose;
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
                new AsyncEnumerator<T>(
                    this.enumerable.GetEnumerator(),
                    this.moveNext,
                    this.current,
                    this.dispose);
        }

        private sealed class AsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> enumerator;
            private readonly Func<IEnumerator<T>, bool> moveNext;
            private readonly Func<IEnumerator<T>, T> current;
            private readonly Func<ValueTask> dispose;

            public AsyncEnumerator(
                IEnumerator<T> enumerator,
                Func<IEnumerator<T>, bool> moveNext,
                Func<IEnumerator<T>, T> current,
                Func<ValueTask> dispose)
            {
                this.enumerator = enumerator;
                this.moveNext = moveNext;
                this.current = current;
                this.dispose = dispose;
            }

            public T Current =>
                this.current(this.enumerator);

            public ValueTask<bool> MoveNextAsync() =>
                new ValueTask<bool>(this.moveNext(this.enumerator));

            public ValueTask DisposeAsync() =>
                this.dispose();
        }
    }
}
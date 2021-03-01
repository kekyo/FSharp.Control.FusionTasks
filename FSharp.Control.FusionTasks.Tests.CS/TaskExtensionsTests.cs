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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;

using NUnit;
using NUnit.Framework;

namespace FSharp.Control.FusionTasks.Tests
{
    [TestFixture]
    public class TaskExtensionsTests
    {
        #region Task.AsAsync
        private static async Task DelayAsync()
        {
            await Task.Delay(500);
            Console.WriteLine("AAA");
        }

        [Test]
        public void TaskAsAsyncTest()
        {
            var task = DelayAsync();
            var asy = task.AsAsync();

            // MSTest not supported FSharpAsync based tests, so run synchronously here. 
            FSharpAsync.RunSynchronously(asy, FSharpOption<int>.None, FSharpOption<CancellationToken>.None);
        }

        [Test]
        public void ConfiguredAsyncAwaiterAsAsyncTest()
        {
            var task = DelayAsync();
            var asy = task.AsAsyncConfigured(false);

            // MSTest not supported FSharpAsync based tests, so run synchronously here. 
            FSharpAsync.RunSynchronously(asy, FSharpOption<int>.None, FSharpOption<CancellationToken>.None);
        }

        private static async Task<int> DelayAndReturnAsync()
        {
            await Task.Delay(500);
            return 123;
        }

        [Test]
        public void TaskTAsAsyncTest()
        {
            var task = DelayAndReturnAsync();
            var asy = task.AsAsync();

            // MSTest not supported FSharpAsync based tests, so run synchronously here. 
            var result = FSharpAsync.RunSynchronously(asy, FSharpOption<int>.None, FSharpOption<CancellationToken>.None);
            Assert.AreEqual(123, result);
        }

        [Test]
        public void ConfiguredAsyncAwaiterTAsAsyncTest()
        {
            var task = DelayAndReturnAsync();
            var asy = task.AsAsyncConfigured(false);

            // MSTest not supported FSharpAsync based tests, so run synchronously here. 
            var result = FSharpAsync.RunSynchronously(asy, FSharpOption<int>.None, FSharpOption<CancellationToken>.None);
            Assert.AreEqual(123, result);
        }

        private static ValueTask<int> DelayAndReturnAsyncByValueTask()
        {
            return new ValueTask<int>(DelayAndReturnAsync());
        }

        [Test]
        public void ValueTaskTAsAsyncTest()
        {
            var task = DelayAndReturnAsyncByValueTask();
            var asy = task.AsAsync();

            // MSTest not supported FSharpAsync based tests, so run synchronously here. 
            var result = FSharpAsync.RunSynchronously(asy, FSharpOption<int>.None, FSharpOption<CancellationToken>.None);
            Assert.AreEqual(123, result);
        }

        [Test]
        public void ConfiguredAsyncAwaiterTValueTaskAsAsyncTest()
        {
            var task = DelayAndReturnAsyncByValueTask();
            var asy = task.AsAsyncConfigured(false);

            // MSTest not supported FSharpAsync based tests, so run synchronously here. 
            var result = FSharpAsync.RunSynchronously(asy, FSharpOption<int>.None, FSharpOption<CancellationToken>.None);
            Assert.AreEqual(123, result);
        }
        #endregion

        #region FSharpAsync<'T>.AsTask
        [Test]
        public async Task AsyncAsTaskTestAsync()
        {
            var asy = FSharpAsync.Sleep(500);
            await asy.AsTask();
        }

        [Test]
        public async Task AsyncTAsTaskTestAsync()
        {
            // C# cannot create FSharpAsync<'T>, so create C#'ed Task<T> and convert to FSharpAsync<'T>.
            var task = DelayAndReturnAsync();
            var asy = task.AsAsync();
            var result = await asy.AsTask();

            Assert.AreEqual(123, result);
        }

        [Test]
        public void AsyncAsTaskWithCancellationTestAsync()
        {
            var cts = new CancellationTokenSource();

            var asy = FSharpAsync.Sleep(500);
            var outerTask = asy.AsTask(cts.Token);

            // Force hard wait.
            Thread.Sleep(100);

            // Task cancelled.
            cts.Cancel();

            // Continuation point. (Will raise exception)
            Assert.ThrowsAsync<TaskCanceledException>(() => outerTask);
        }

        private static async Task<int> DelayAndReturnAsync(CancellationToken token)
        {
            await Task.Delay(500, token); /* explicitly */
            return 123;
        }

        [Test]
        public void AsyncTAsTaskWithCancellationTestAsync()
        {
            // Information:
            //   F#'s CancellationToken is managing contextual token in async computation expression.
            //   But .NET CancellationToken is non contextual/default token, in C# too.
            //   So no automatic implicit derive tokens, token just explicit set to any async operation:
            //     Task.Delay(int, token) <-- DelayAndReturnAsync(token) <-- cts.Token

            var cts = new CancellationTokenSource();

            // C# cannot create FSharpAsync<'T>, so create C#'ed Task<T> and convert to FSharpAsync<'T>.
            var task = DelayAndReturnAsync(/* explicitly */ cts.Token);
            var asy = task.AsAsync();
            var outerTask = asy.AsTask(cts.Token); /* explicitly, but not derived into DelayAndReturnAsync() */

            // Force hard wait.
            Thread.Sleep(100);

            // Task cancelled.
            cts.Cancel();

            // Continuation point. (Will raise exception)
            Assert.ThrowsAsync<TaskCanceledException>(() => outerTask);
        }

        private static ValueTask<int> DelayAndReturnAsyncByValueTask(CancellationToken token)
        {
            return new ValueTask<int>(DelayAndReturnAsync(token));
        }

        [Test]
        public void AsyncTValueTaskAsTaskWithCancellationTestAsync()
        {
            // Information:
            //   F#'s CancellationToken is managing contextual token in async computation expression.
            //   But .NET CancellationToken is non contextual/default token, in C# too.
            //   So no automatic implicit derive tokens, token just explicit set to any async operation:
            //     Task.Delay(int, token) <-- DelayAndReturnAsync(token) <-- cts.Token

            var cts = new CancellationTokenSource();

            // C# cannot create FSharpAsync<'T>, so create C#'ed Task<T> and convert to FSharpAsync<'T>.
            var task = DelayAndReturnAsyncByValueTask(/* explicitly */ cts.Token);
            var asy = task.AsAsync();
            var outerTask = asy.AsTask(cts.Token); /* explicitly, but not derived into DelayAndReturnAsyncByValueTask() */

            // Force hard wait.
            Thread.Sleep(100);

            // Task cancelled.
            cts.Cancel();

            // Continuation point. (Will raise exception)
            Assert.ThrowsAsync<TaskCanceledException>(() => outerTask);
        }
        #endregion

        #region FSharpAsync<'T>.GetAwaiter
        [Test]
        public async Task AsyncGetAwaiterTestAsync()
        {
            var asy = FSharpAsync.Sleep(500);
            await asy;
        }

        [Test]
        public async Task AsyncTGetAwaiterTestAsync()
        {
            // C# cannot create FSharpAsync<'T>, so create C#'ed Task<T> and convert to FSharpAsync<'T>.
            var task = DelayAndReturnAsync();
            var asy = task.AsAsync();
            var result = await asy;

            Assert.AreEqual(123, result);
        }
        #endregion

        [Test]
        public void HoldSynchContextTest()
        {
            var id1 = Thread.CurrentThread.ManagedThreadId;
            var context = new ThreadBoundSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(context);

            async Task ComputationAsync()
            {
                var idStartImmediately = Thread.CurrentThread.ManagedThreadId;
                Assert.AreEqual(id1, idStartImmediately);

                await FSharpAsync.StartImmediateAsTask(
                    FSharpAsync.Sleep(300), FSharpOption<CancellationToken>.None);
                var idAwaitTask = Thread.CurrentThread.ManagedThreadId;
                Assert.AreEqual(id1, idAwaitTask);

                await FSharpAsync.Sleep(300).AsTask();
                var idAwaited1 = Thread.CurrentThread.ManagedThreadId;
                Assert.AreEqual(id1, idAwaited1);

                await FSharpAsync.Sleep(300).AsTask();
                var idAwaited2 = Thread.CurrentThread.ManagedThreadId;
                Assert.AreEqual(id1, idAwaited2);

                context.Quit();
            }

            var _ = ComputationAsync();
            context.Run();
        }
    }
}

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

namespace FSharp.Control.FusionTasks
{
    public sealed class ThreadBoundSynchronizationContext :
        SynchronizationContext
    {
        private readonly struct Entry
        {
            public readonly SendOrPostCallback Callback;
            public readonly object? State;

            public Entry(SendOrPostCallback callback, object? state)
            {
                this.Callback = callback;
                this.State = state;
            }
        }

        private readonly Queue<Entry> entries = new Queue<Entry>();
        private int? boundThreadId;
        private readonly ManualResetEventSlim queued = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim quit = new ManualResetEventSlim(false);

        public void Run()
        {
            this.boundThreadId = Thread.CurrentThread.ManagedThreadId;
            while (true)
            {
                if (WaitHandle.WaitAny(new [] { this.queued.WaitHandle, this.quit.WaitHandle }) == 1)
                {
                    break;
                }

                loop:
                    Entry entry;
                    lock (this.entries)
                    {
                        if (this.entries.Count == 0)
                        {
                            this.queued.Reset();
                            continue;
                        }
                        entry = this.entries.Dequeue();
                    }

                    entry.Callback(entry.State);
                    goto loop;
            }
        }

        public void Quit() =>
            this.quit.Set();
        
        public override void Post(SendOrPostCallback d, object? state)
        {
            lock (this.entries)
            {
                if (this.boundThreadId != Thread.CurrentThread.ManagedThreadId)
                {
                    this.entries.Enqueue(new Entry(d, state));
                    if (this.entries.Count >= 1)
                    {
                        this.queued.Set();
                    }
                    return;
                }
            }
            
            d(state);
        }
    }
}
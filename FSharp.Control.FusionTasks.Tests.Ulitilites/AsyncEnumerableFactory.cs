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
using System.Threading.Tasks;

namespace FSharp.Control.FusionTasks.Tests
{
    public static class AsyncEnumerableFactory
    {
        public static async IAsyncEnumerable<U> DelayAsync<T, U>(
            this IEnumerable<T> enumerable,
            Func<T, U> selector,
            TimeSpan delay)
        {
            foreach (var value in enumerable)
            {
                await Task.Delay(delay);
                yield return selector(value);
            }
        }
    }
}
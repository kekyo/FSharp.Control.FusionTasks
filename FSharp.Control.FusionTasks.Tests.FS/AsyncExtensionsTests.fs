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

namespace FSharp.Control.FusionTasks.Tests

open System
open System.IO
open System.Diagnostics
open System.Threading
open System.Threading.Tasks

open FSharp.Control.FusionTasks
open NUnit.Framework

module AsyncExtensions =

  [<Test>]
  let AsyncBuilderAsAsyncFromTaskTest() =
    let test = async {
      let r = Random()
      let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
      do r.NextBytes data
      use ms = new MemoryStream()
      let computation = async {
          do! ms.WriteAsync(data, 0, data.Length)
          Assert.AreEqual(data, ms.ToArray())
      }
      do! computation
    }
    test.AsTask()
        
  [<Test>]
  let AsyncBuilderAsAsyncFromTaskTTest() =
    let test = async {
      let r = Random()
      let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
      do r.NextBytes data
      let computation = async {
          use ms = new MemoryStream()
          do ms.Write(data, 0, data.Length)
          do ms.Position <- 0L
          let! length = ms.ReadAsync(data, 0, data.Length)
          Assert.AreEqual(data.Length, length)
          
          return ms.ToArray()
      }
      let! results = computation
      Assert.AreEqual(data, results)
    }
    test.AsTask()

  [<Test>]
  let AsyncBuilderAsAsyncCTATest() =
    let test = async {
      let r = Random()
      let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
      do r.NextBytes data
      use ms = new MemoryStream()
      let computation = async {
          do! ms.WriteAsync(data, 0, data.Length).AsyncConfigure(false)
        }
      do! computation
      Assert.AreEqual(data, ms.ToArray())
    }
    test.AsTask()
    
  [<Test>]
  let AsyncBuilderAsAsyncCTATTest() =
    let test = async {
      let r = Random()
      let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
      do r.NextBytes data
      let computation = async {
          use ms = new MemoryStream()
          do ms.Write(data, 0, data.Length)
          do ms.Position <- 0L
          let! length = ms.ReadAsync(data, 0, data.Length).AsyncConfigure(false)
          Assert.AreEqual(data.Length, length)

          return ms.ToArray()
        }
      let! results = computation
      Assert.AreEqual(data, results)
    }
    test.AsTask()

  [<Test>]
  let AsyncBuilderAsAsyncFromValueTaskTest() =
    let test = async {
      let r = Random()
      let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
      do r.NextBytes data
      use ms = new MemoryStream()
      let computation = async {
          do! new ValueTask(ms.WriteAsync(data, 0, data.Length))
        }
      do! computation
      Assert.AreEqual(data, ms.ToArray())
    }
    test.AsTask()
            
  [<Test>]
  let AsyncBuilderAsAsyncFromValueTaskTTest() =
    let test = async {
      let r = Random()
      let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
      do r.NextBytes data
      let computation = async {
          use ms = new MemoryStream()
          do ms.Write(data, 0, data.Length)
          do ms.Position <- 0L
          let! length = new ValueTask<int>(ms.ReadAsync(data, 0, data.Length))
          Assert.AreEqual(data.Length, length)

          return ms.ToArray()
      }
      let! results = computation
      Assert.AreEqual(data, results)
    }
    test.AsTask()

  [<Test>]
  let AsyncBuilderAsAsyncCVTATest() =
    let test = async {
      let r = Random()
      let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
      do r.NextBytes data
      use ms = new MemoryStream()
      let computation = async {
          do! (new ValueTask(ms.WriteAsync(data, 0, data.Length))).AsyncConfigure(false)
        }
      do! computation
      Assert.AreEqual(data, ms.ToArray())
    }
    test.AsTask()
    
  [<Test>]
  let AsyncBuilderAsAsyncCVTATTest() =
    let test = async {
      let r = Random()
      let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
      do r.NextBytes data
      let computation = async {
          use ms = new MemoryStream()
          do ms.Write(data, 0, data.Length)
          do ms.Position <- 0L
          let! length = (new ValueTask<int>(ms.ReadAsync(data, 0, data.Length))).AsyncConfigure(false)
          Assert.AreEqual(data.Length, length)

          return ms.ToArray()
        }
      let! results = computation
      Assert.AreEqual(data, results)
    }
    test.AsTask()

  [<Test>]
  let AsyncBuilderAsAsyncVTest() =
    let test = async {
      let r = Random()
      let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
      do r.NextBytes data
      let computation = async {
          use ms = new MemoryStream()
          do! new ValueTask(ms.WriteAsync(data, 0, data.Length))
          do ms.Position <- 0L
          let length = ms.Read(data, 0, data.Length)
          Assert.AreEqual(data.Length, length)

          return ms.ToArray()
      }
      let! results = computation
      Assert.AreEqual(data, results)
    }
    test.AsTask()

  [<Test>]
  let AsyncBuilderAsAsyncVTTest() =
    let test = async {
      let r = Random()
      let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
      do r.NextBytes data
      let computation = async {
          use ms = new MemoryStream()
          do ms.Write(data, 0, data.Length)
          do ms.Position <- 0L
          let! length = new ValueTask<int>(ms.ReadAsync(data, 0, data.Length))
          Assert.AreEqual(data.Length, length)
          return ms.ToArray()
        }
      let! results = computation
      Assert.AreEqual(data, results)
    }
    test.AsTask()

  [<Test>]
  let AsyncBuilderWithAsyncAndTaskCombinationTest() =
    let asyncGenData() = async {
        let r = Random()
        let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
        do r.NextBytes data
        return data
      }
    let computation = async {
        let! data = asyncGenData()
        use ms = new MemoryStream()
        do! ms.WriteAsync(data, 0, data.Length)
        Assert.AreEqual(data, ms.ToArray())
      }
    computation.AsTask()

  [<Test>]
  let AsyncBuilderCompilesForInTest() =
    let computation = async {
        let mutable result = 0
        for i in {1..10} do
          result <- result + i
        Assert.AreEqual(55, result)
      }

    computation.AsTask()

  [<Test>]
  let HoldSynchContextTest() =
    let id1 = Thread.CurrentThread.ManagedThreadId
    let context = new ThreadBoundSynchronizationContext()
    SynchronizationContext.SetSynchronizationContext(context)
    let computation = async {
        let idStartImmediately = Thread.CurrentThread.ManagedThreadId
        Assert.AreEqual(id1, idStartImmediately)

        do! Task.Delay 300 |> Async.AwaitTask
        let idAwaitTask = Thread.CurrentThread.ManagedThreadId
        Assert.AreEqual(id1, idAwaitTask)

        do! Task.Delay 300
        let idAwaited1 = Thread.CurrentThread.ManagedThreadId
        Assert.AreEqual(id1, idAwaited1)

        do! Task.Delay 300
        let idAwaited2 = Thread.CurrentThread.ManagedThreadId
        Assert.AreEqual(id1, idAwaited2)
        
        context.Quit()
      }
    computation |> Async.StartImmediate
    context.Run()

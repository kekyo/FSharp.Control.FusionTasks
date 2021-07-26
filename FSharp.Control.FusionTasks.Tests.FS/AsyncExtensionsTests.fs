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

open NUnit.Framework

#nowarn "44"

module AsyncExtensions =

  ////////////////////////////////////////////////////////////////////////
  // Basis

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

  // #11
  //[<TestCase(true, 111)>]
  //[<TestCase(false, 222)>]
  //let AsyncBuilderCompilesIfInTest(flag, expected) =
  //  let computation = async {
  //      if flag
  //      then
  //        Assert.IsTrue flag
  //        return ()
  //      do! Task.Delay 100
  //      Assert.IsFalse flag
  //    }
  //  computation |> Async.RunSynchronously
  
  ////////////////////////////////////////////////////////////////////////
  // Configured async monad.
  
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
  
  ////////////////////////////////////////////////////////////////////////
  // SynchronizationContext

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

  ////////////////////////////////////////////////////////////////////////
  // IAsyncEnumerable

  let private delay = TimeSpan.FromMilliseconds 100.0

  [<Test>]
  let AsyncEnumerableIterationTest() =
    let computation = async {
      let values = [ 2; 5; 3; 7; 1 ]
      let results = new System.Collections.Generic.List<int>()
      for value in values.DelayEachAsync(delay) do
        results.Add value
        do! Async.Sleep delay
      Assert.AreEqual(values, results)
    }
    computation.AsTask()

  [<Test>]
  let AsyncEnumerableIterationNoDelayingBodyTest() =
    let computation = async {
      let values = [ 2; 5; 3; 7; 1 ]
      let results = new System.Collections.Generic.List<int>()
      for value in values.DelayEachAsync(delay) do
        results.Add value
      Assert.AreEqual(values, results)
    }
    computation.AsTask()
    
  [<Test>]
  let AsyncEnumerableSynchIterationTest() =
    let computation = async {
      let values = [ 2; 5; 3; 7; 1 ]
      let results = new System.Collections.Generic.List<int>()
      for value in values.AsAsyncEnumerable() do
        results.Add value
        do! Async.Sleep delay
      Assert.AreEqual(values, results)
    }
    computation.AsTask()

  [<Test>]
  let AsyncEnumerableSynchIterationNoDelayingBodyTest() =
    let computation = async {
      let values = [ 2; 5; 3; 7; 1 ]
      let results = new System.Collections.Generic.List<int>()
      for value in values.AsAsyncEnumerable() do
        results.Add value
      Assert.AreEqual(values, results)
    }
    computation.AsTask()

  [<Test>]
  let AsyncEnumerableIterationThrowingTest() =
    let computation = async {
      let values = [ 2; 5; 3; 7; 1 ]
      let results = new System.Collections.Generic.List<int>()
      let exn = new Exception()
      let mutable index = 0
      try
        for value in values.DelayEachAsync(delay) do
          results.Add value
          index <- index + 1
          if index >= 3 then
            raise exn
          do! Async.Sleep delay
        Assert.Fail()
      with
      | exn2 ->
        Assert.AreSame(exn, exn2)
        Assert.AreEqual(values |> Seq.take 3, results)
    }
    computation.AsTask()

  [<Test>]
  let AsyncEnumerableIterationThrowingNoDelayingBodyTest() =
    let computation = async {
      let values = [ 2; 5; 3; 7; 1 ]
      let results = new System.Collections.Generic.List<int>()
      let exn = new Exception()
      let mutable index = 0
      try
        for value in values.DelayEachAsync(delay) do
          results.Add value
          index <- index + 1
          if index >= 3 then
            raise exn
        Assert.Fail()
      with
      | exn2 ->
        Assert.AreSame(exn, exn2)
        Assert.AreEqual(values |> Seq.take 3, results)
    }
    computation.AsTask()

  [<Test>]
  let AsyncEnumerableSynchIterationThrowingTest() =
    let computation = async {
      let values = [ 2; 5; 3; 7; 1 ]
      let results = new System.Collections.Generic.List<int>()
      let exn = new Exception()
      let mutable index = 0
      try
        for value in values.AsAsyncEnumerable() do
          results.Add value
          index <- index + 1
          if index >= 3 then
            raise exn
          do! Async.Sleep delay
        Assert.Fail()
      with
      | exn2 ->
        Assert.AreSame(exn, exn2)
        Assert.AreEqual(values |> Seq.take 3, results)
    }
    computation.AsTask()

  [<Test>]
  let AsyncEnumerableSynchIterationThrowingNoDelayingBodyTest() =
    let computation = async {
      let values = [ 2; 5; 3; 7; 1 ]
      let results = new System.Collections.Generic.List<int>()
      let exn = new Exception()
      let mutable index = 0
      try
        for value in values.AsAsyncEnumerable() do
          results.Add value
          index <- index + 1
          if index >= 3 then
            raise exn
        Assert.Fail()
      with
      | exn2 ->
        Assert.AreSame(exn, exn2)
        Assert.AreEqual(values |> Seq.take 3, results)
    }
    computation.AsTask()

  [<Test>]
  let AsyncEnumerableIterationThrowingBeforeTest() =
    let computation = async {
      let values = [ 2; 5; 3; 7; 1 ]
      let results = new System.Collections.Generic.List<int>()
      try
        for value in values.WillThrowBefore() do
          results.Add value
          do! Async.Sleep delay
        Assert.Fail()
      with
      | exn ->
        Assert.AreEqual("WillThrowBefore", exn.Message)
        Assert.AreEqual(0, results.Count)
    }
    computation.AsTask()

  [<Test>]
  let AsyncEnumerableIterationThrowingAfterTest() =
    let computation = async {
      let values = [ 2; 5; 3; 7; 1 ]
      let results = new System.Collections.Generic.List<int>()
      try
        for value in values.WillThrowAfter() do
          results.Add value
          do! Async.Sleep delay
        Assert.Fail()
      with
      | exn ->
        Assert.AreEqual("WillThrowAfter", exn.Message)
        Assert.AreEqual(values, results)
    }
    computation.AsTask()
    
  [<Test>]
  let AsyncEnumerableIterationThrowingInterBeforeTest() =
    let computation = async {
      let values = [ 2; 5; 3; 7; 1 ]
      let results = new System.Collections.Generic.List<int>()
      try
        for value in values.WillThrowInterBefore() do
          results.Add value
          do! Async.Sleep delay
        Assert.Fail()
      with
      | exn ->
        Assert.AreEqual("WillThrowInterBefore", exn.Message)
        Assert.AreEqual(0, results.Count)
    }
    computation.AsTask()

  [<Test>]
  let AsyncEnumerableIterationThrowingInterAfterTest() =
    let computation = async {
      let values = [ 2; 5; 3; 7; 1 ]
      let results = new System.Collections.Generic.List<int>()
      try
        for value in values.WillThrowInterAfter() do
          results.Add value
          do! Async.Sleep delay
        Assert.Fail()
      with
      | exn ->
        Assert.AreEqual("WillThrowInterAfter", exn.Message)
        Assert.AreEqual(values |> Seq.take 1, results)
    }
    computation.AsTask()
    
  [<Test>]
  let AsyncEnumerableWillCallAsyncDisposeTest() =
    let computation = async {
      let values = [ 2; 5; 3; 7; 1 ]
      let results = new System.Collections.Generic.List<int>()
      let mutable called = false
      for value in values.HookAsyncEnumerable(fun () ->
          called <- true
          new ValueTask()) do
        results.Add value
        do! Async.Sleep delay
      Assert.IsTrue(called)
      Assert.AreEqual(values, results)
    }
    computation.AsTask()

  [<Test>]
  let AsyncEnumerableWillDelayCallAsyncDisposeTest() =
    let computation = async {
      let values = [ 2; 5; 3; 7; 1 ]
      let results = new System.Collections.Generic.List<int>()
      let mutable called = false
      for value in values.HookAsyncEnumerable(fun () ->
        new ValueTask(
          Async.StartImmediateAsTask (async {
            do! Async.Sleep delay
            called <- true }))) do
        results.Add value
        do! Async.Sleep delay
      Assert.IsTrue(called)
      Assert.AreEqual(values, results)
    }
    computation.AsTask()

  [<Test>]
  let AsyncEnumerableWillThrowAsyncDisposeTest() =
    let computation = async {
      let values = [ 2; 5; 3; 7; 1 ]
      let results = new System.Collections.Generic.List<int>()
      let exn = new Exception()
      try
        for value in values.HookAsyncEnumerable(fun () -> raise exn) do
          results.Add value
          do! Async.Sleep delay
        Assert.Fail()
      with
      | exn2 ->
        Assert.AreSame(exn, exn2)
        Assert.AreEqual(values, results)
    }
    computation.AsTask()

  [<Test>]
  let AsyncEnumerableWillDelayedThrowAsyncDisposeTest() =
    let computation = async {
      let values = [ 2; 5; 3; 7; 1 ]
      let results = new System.Collections.Generic.List<int>()
      let exn = new Exception()
      try
        for value in values.HookAsyncEnumerable(fun () ->
          new ValueTask(
            Async.StartImmediateAsTask (async {
              do! Async.Sleep delay
              raise exn }))) do
          results.Add value
          do! Async.Sleep delay
        Assert.Fail()
      with
      | exn2 ->
        Assert.AreSame(exn, exn2)
        Assert.AreEqual(values, results)
    }
    computation.AsTask()

  [<Test>]
  let AsyncEnumerableIterationConfiguredTest() =
    let computation = async {
      let values = [ 2; 5; 3; 7; 1 ]
      let results = new System.Collections.Generic.List<int>()
      let tid = Thread.CurrentThread.ManagedThreadId
      for value in values.DelayEachAsync(delay).ConfigureAwait(false) do
        Assert.AreNotEqual(tid, Thread.CurrentThread.ManagedThreadId)
        results.Add value
        do! Async.Sleep delay
      Assert.AreEqual(values, results)
    }
    computation.AsTask()

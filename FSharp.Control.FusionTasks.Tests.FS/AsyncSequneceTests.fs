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

module AsyncSequenceTests =

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

  [<TestCase(true)>]
  [<TestCase(false)>]
  let AsyncEnumerableIterationCaptureContextTest(capture: bool) =
    let context = new ThreadBoundSynchronizationContext()
    SynchronizationContext.SetSynchronizationContext context
    let computation = async {
      let values = [ 2; 5; 3; 7; 1 ]
      let results = new System.Collections.Generic.List<int>()
      let tid = Thread.CurrentThread.ManagedThreadId
      for value in values.DelayEachAsync(delay).ConfigureAwait(capture) do
        let tid2 = Thread.CurrentThread.ManagedThreadId
        if capture then
          Assert.AreEqual(tid, tid2)
        else
          Assert.AreNotEqual(tid, tid2)
        results.Add value
        do! Async.Sleep delay
      let tid3 = Thread.CurrentThread.ManagedThreadId
      if capture then
        Assert.AreEqual(tid, tid3)
      else
        Assert.AreNotEqual(tid, tid3)
      Assert.AreEqual(values, results)
    }
    context.Run(computation.AsTask())

  ////////////////////////////////////////////////////////////////////////
  // IAsyncDisposable

  [<Test>]
  let AsyncDisposableTest() =
    let mutable index = 0
    let computation = async {
      let inner = async {
        use _ = AsyncDisposableFactory.CreateDelegatedAsyncDisposable(fun () ->
          (async {
            do! Async.Sleep delay
            index <- index + 1
          }).AsTask())
        Assert.AreEqual(0, index)
        return ()
      }
      do! inner
      Assert.AreEqual(1, index)
    }
    computation.AsTask()

  [<Test>]
  let AsyncDisposableThrowedTest() =
    let ex = new Exception()
    let computation = async {
      let inner = async {
        use _ = AsyncDisposableFactory.CreateDelegatedAsyncDisposable(fun () ->
          (async {
            do! Async.Sleep delay
          }).AsTask())
        raise ex
        return ()
      }
      try
        do! inner
        Assert.Fail()
      with
      | exn -> Assert.AreSame(ex, exn)
    }
    computation.AsTask()

  [<Test>]
  let AsyncDisposableThrowedDisposingTest() =
    let ex = new Exception()
    let computation = async {
      let inner = async {
        use _ = AsyncDisposableFactory.CreateDelegatedAsyncDisposable(fun () ->
          (async {
            do! Async.Sleep delay
            raise ex
          }).AsTask())
        return ()
      }
      try
        do! inner
        Assert.Fail()
      with
      | exn -> Assert.AreSame(ex, exn)
    }
    computation.AsTask()

  //[<Test>]
  //let AsyncDisposableCaptureContextTest() =
    // In currently C#, lacks a feature for detaching synch context doing at just after asynchronous disposer.
    // https://github.com/dotnet/csharplang/discussions/2661

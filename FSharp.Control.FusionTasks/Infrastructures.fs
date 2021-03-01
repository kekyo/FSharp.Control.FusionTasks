/////////////////////////////////////////////////////////////////////////////////////////////////
//
// FSharp.Control.FusionTasks - F# Async workflow <--> .NET Task easy seamless interoperability library.
// Copyright (c) 2016-2018 Kouji Matsui (@kozy_kekyo)
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

namespace Microsoft.FSharp.Control

open System
open System.Runtime.CompilerServices
open System.Threading
open System.Threading.Tasks

///////////////////////////////////////////////////////////////////////////////////
// Internal implementations.

module internal Infrastructures =

  let private (|IsFaulted|IsCanceled|IsCompleted|) (task: Task) =
    if task.IsFaulted then IsFaulted task.Exception
    else if task.IsCanceled then IsCanceled
    else IsCompleted

  let private safeToken (ct: CancellationToken option) =
    match ct with
    | Some token -> token
    | None -> Async.DefaultCancellationToken

  let createCanceledException(token: CancellationToken option) =
    // TODO: Constructed stack traces. require?
    try
      match token with
      | Some t -> new OperationCanceledException(t) |> raise
      | None -> new OperationCanceledException() |> raise
    with :? OperationCanceledException as e -> e

  let asTaskT(async: Async<'T>, ct: CancellationToken option) =
    let tcs = TaskCompletionSource<'T>()
    Async.StartWithContinuations(
      async,
      tcs.SetResult,
      tcs.SetException,
      tcs.SetException, // Derived from original OperationCancelledException
      safeToken ct)
    tcs.Task

  let asValueTask(async: Async<Unit>, ct: CancellationToken option) =
    let tcs = TaskCompletionSource<Unit>()
    Async.StartWithContinuations(
      async,
      tcs.SetResult,
      tcs.SetException,
      tcs.SetException, // Derived from original OperationCancelledException
      safeToken ct)
    ValueTask(tcs.Task)

  let asValueTaskT(async: Async<'T>, ct: CancellationToken option) =
    let tcs = TaskCompletionSource<'T>()
    Async.StartWithContinuations(
      async,
      tcs.SetResult,
      tcs.SetException,
      tcs.SetException, // Derived from original OperationCancelledException
      safeToken ct)
    ValueTask<'T>(tcs.Task)

  let asAsync(task: Task, ct: CancellationToken option) =
    Async.FromContinuations(
      fun (completed, caught, canceled) ->
        task.ContinueWith(
          new Action<Task>(fun _ ->
            match task with  
            | IsFaulted exn -> caught(exn)
            | IsCanceled -> canceled(createCanceledException ct) // TODO: how to extract implicit caught exceptions from task?
            | IsCompleted -> completed(())),
          safeToken ct)
        |> ignore)

  let asAsyncT(task: Task<'T>, ct: CancellationToken option) =
    Async.FromContinuations(
      fun (completed, caught, canceled) ->
        task.ContinueWith(
          new Action<Task<'T>>(fun _ ->
            match task with  
            | IsFaulted exn -> caught(exn)
            | IsCanceled -> canceled(createCanceledException ct) // TODO: how to extract implicit caught exceptions from task?
            | IsCompleted -> completed(task.Result)),
          safeToken ct)
        |> ignore)

  let asAsyncV(task: ValueTask, ct: CancellationToken option) =
    Async.FromContinuations(
      fun (completed, caught, canceled) ->
        let task = task.AsTask()
        task.ContinueWith(
          new Action<Task>(fun _ ->
            match task with  
            | IsFaulted exn -> caught(exn)
            | IsCanceled -> canceled(createCanceledException ct) // TODO: how to extract implicit caught exceptions from task?
            | IsCompleted -> completed()),
          safeToken ct)
        |> ignore)

  let asAsyncVT(task: ValueTask<'T>, ct: CancellationToken option) =
    Async.FromContinuations(
      fun (completed, caught, canceled) ->
        let task = task.AsTask()
        task.ContinueWith(
          new Action<Task<'T>>(fun _ ->
            match task with  
            | IsFaulted exn -> caught(exn)
            | IsCanceled -> canceled(createCanceledException ct) // TODO: how to extract implicit caught exceptions from task?
            | IsCompleted -> completed(task.Result)),
          safeToken ct)
        |> ignore)

  let asAsyncCTA(cta: ConfiguredTaskAsyncAwaitable) =
    Async.FromContinuations(
      fun (completed, caught, canceled) ->
        let awaiter = cta.GetAwaiter()
        awaiter.OnCompleted(
          new Action(fun _ ->
            try
              awaiter.GetResult()
              completed()
            with exn -> caught(exn)))
        |> ignore)

  let asAsyncCTAT(cta: ConfiguredTaskAsyncAwaitable<'T>) =
    Async.FromContinuations(
      fun (completed, caught, canceled) ->
        let awaiter = cta.GetAwaiter()
        awaiter.OnCompleted(
          new Action(fun _ ->
            try completed(awaiter.GetResult())
            with exn -> caught(exn)))
        |> ignore)

  let asAsyncCVTA(cta: ConfiguredValueTaskAsyncAwaitable) =
    Async.FromContinuations(
      fun (completed, caught, canceled) ->
        let awaiter = cta.GetAwaiter()
        awaiter.OnCompleted(
          new Action(fun _ ->
            try completed(awaiter.GetResult())
            with exn -> caught(exn)))
        |> ignore)

  let asAsyncCVTAT(cta: ConfiguredValueTaskAsyncAwaitable<'T>) =
    Async.FromContinuations(
      fun (completed, caught, canceled) ->
        let awaiter = cta.GetAwaiter()
        awaiter.OnCompleted(
          new Action(fun _ ->
            try completed(awaiter.GetResult())
            with exn -> caught(exn)))
        |> ignore)

///////////////////////////////////////////////////////////////////////////////////

/// <summary>
/// Delegation F#'s async continuation.
/// </summary>
/// <description>
/// Simulate TaskCompletionSource&lt;'T&gt; for F#'s Async&lt;'T&gt;.
/// </description>
/// <typeparam name="'T">Computation result type</typeparam> 
[<Sealed; NoEquality; NoComparison; AutoSerializable(false)>]
type AsyncCompletionSource<'T> =

  [<DefaultValue>]
  val mutable private _completed : 'T -> unit
  [<DefaultValue>]
  val mutable private _caught : exn -> unit
  [<DefaultValue>]
  val mutable private _canceled : OperationCanceledException -> unit

  val private _async : Async<'T>

  /// <summary>
  /// Constructor.
  /// </summary>
  new () as this = {
    _async = Async.FromContinuations<'T>(fun (completed, caught, canceled) ->
      this._completed <- completed
      this._caught <- caught
      this._canceled <- canceled)
  }

  /// <summary>
  /// Target Async&lt;'T&gt; instance.
  /// </summary>
  member this.Async = this._async

  /// <summary>
  /// Set result value and continue continuation.
  /// </summary>
  /// <param name="value">Result value</param>
  member this.SetResult value = this._completed value

  /// <summary>
  /// Set exception and continue continuation.
  /// </summary>
  /// <param name="exn">Exception instance</param>
  member this.SetException exn = this._caught exn

  /// <summary>
  /// Cancel async computation.
  /// </summary>
  member this.SetCanceled() =
    this._canceled(Infrastructures.createCanceledException(None))

  /// <summary>
  /// Cancel async computation.
  /// </summary>
  /// <param name="token">CancellationToken</param>
  member this.SetCanceled token =
    this._canceled(Infrastructures.createCanceledException(Some token))

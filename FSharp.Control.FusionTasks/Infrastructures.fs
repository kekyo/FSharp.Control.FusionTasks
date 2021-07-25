﻿/////////////////////////////////////////////////////////////////////////////////////////////////
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

namespace Microsoft.FSharp.Control

open System
open System.Threading
open System.Threading.Tasks
open System.Collections.Generic

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

  let asTaskT(async: Async<'T>, ct: CancellationToken option) =
    Async.StartImmediateAsTask(async, safeToken ct)

  let asValueTask(async: Async<Unit>, ct: CancellationToken option) =
    ValueTask(Async.StartImmediateAsTask(async, safeToken ct))

  let asValueTaskT(async: Async<'T>, ct: CancellationToken option) =
    ValueTask<'T>(Async.StartImmediateAsTask(async, safeToken ct))

  let asAsync(task: Task, ct: CancellationToken option) =
    let scheduler = Utilities.getScheduler()
    Async.FromContinuations(
      fun (completed, caught, canceled) ->
        task.ContinueWith(
          new Action<Task>(fun _ ->
            match task with  
            | IsFaulted exn -> caught(exn)
            | IsCanceled -> canceled(Utilities.createCanceledException ct) // TODO: how to extract implicit caught exceptions from task?
            | IsCompleted -> completed(())),
            safeToken ct,
            TaskContinuationOptions.AttachedToParent,
            scheduler)
        |> ignore)

  let asAsyncT(task: Task<'T>, ct: CancellationToken option) =
    let scheduler = Utilities.getScheduler()
    Async.FromContinuations(
      fun (completed, caught, canceled) ->
        task.ContinueWith(
          new Action<Task<'T>>(fun _ ->
            match task with  
            | IsFaulted exn -> caught(exn)
            | IsCanceled -> canceled(Utilities.createCanceledException ct) // TODO: how to extract implicit caught exceptions from task?
            | IsCompleted -> completed(task.Result)),
            safeToken ct,
            TaskContinuationOptions.AttachedToParent,
            scheduler)
        |> ignore)

  let asAsyncV(task: ValueTask, ct: CancellationToken option) =
    let scheduler = Utilities.getScheduler()
    Async.FromContinuations(
      fun (completed, caught, canceled) ->
        match task.IsCompletedSuccessfully with
        | true -> completed()
        | false ->
          let task = task.AsTask()
          task.ContinueWith(
            new Action<Task>(fun _ ->
              match task with  
              | IsFaulted exn -> caught(exn)
              | IsCanceled -> canceled(Utilities.createCanceledException ct) // TODO: how to extract implicit caught exceptions from task?
              | IsCompleted -> completed()),
              safeToken ct,
              TaskContinuationOptions.AttachedToParent,
              scheduler)
          |> ignore)

  let asAsyncVT(task: ValueTask<'T>, ct: CancellationToken option) =
    let scheduler = Utilities.getScheduler()
    Async.FromContinuations(
      fun (completed, caught, canceled) ->
        match task.IsCompletedSuccessfully with
        | true -> completed(task.Result)
        | false ->
          let task = task.AsTask()
          task.ContinueWith(
            new Action<Task<'T>>(fun _ ->
              match task with  
              | IsFaulted exn -> caught(exn)
              | IsCanceled -> canceled(Utilities.createCanceledException ct) // TODO: how to extract implicit caught exceptions from task?
              | IsCompleted -> completed(task.Result)),
              safeToken ct,
              TaskContinuationOptions.AttachedToParent,
              scheduler)
          |> ignore)

  let asAsyncCTA(cta: ConfiguredTaskAsyncAwaitable) =
    Async.FromContinuations(
      fun (completed, caught, _) ->
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
      fun (completed, caught, _) ->
        let awaiter = cta.GetAwaiter()
        awaiter.OnCompleted(
          new Action(fun _ ->
            try completed(awaiter.GetResult())
            with exn -> caught(exn)))
        |> ignore)

  let asAsyncCVTA(cta: ConfiguredValueTaskAsyncAwaitable) =
    Async.FromContinuations(
      fun (completed, caught, _) ->
        let awaiter = cta.GetAwaiter()
        awaiter.OnCompleted(
          new Action(fun _ ->
            try completed(awaiter.GetResult())
            with exn -> caught(exn)))
        |> ignore)

  let asAsyncCVTAT(cta: ConfiguredValueTaskAsyncAwaitable<'T>) =
    Async.FromContinuations(
      fun (completed, caught, _) ->
        let awaiter = cta.GetAwaiter()
        awaiter.OnCompleted(
          new Action(fun _ ->
            try completed(awaiter.GetResult())
            with exn -> caught(exn)))
        |> ignore)

#if !NET45 && !NETSTANDARD1_6 && !NETCOREAPP2_0
  let asAsyncE(enumerable: IAsyncEnumerable<'T>, body: 'T -> Async<'U>, ct: CancellationToken option) =
    let enumerator = enumerable.GetAsyncEnumerator(Utilities.unwrap ct)
    Async.FromContinuations(
      fun (completed, caught, canceled) ->
        let mutable finalValue = Unchecked.defaultof<'U>
        let rec whileLoop() =
          try
            let moveNextAwaiter = enumerator.MoveNextAsync().GetAwaiter()
            let getResultContinuation() =
              let moveNextResult = moveNextAwaiter.GetResult()
              if moveNextResult then
                let resultAsync = body enumerator.Current
                Async.StartWithContinuations(
                  resultAsync,
                  (fun result ->
                    finalValue <- result
                    // Maybe will not cause stack overflow, because async workflow will be scattered recursive calls...
                    whileLoop()),
                  caught,
                  canceled)
              else
                completed(finalValue)
            if moveNextAwaiter.IsCompleted then
              getResultContinuation()
            else
              moveNextAwaiter.OnCompleted(new Action(getResultContinuation))
          with
          | exn -> caught exn
        whileLoop())
#endif

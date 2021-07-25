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

namespace Microsoft.FSharp.Control

open System

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
    this._canceled(Utilities.createCanceledException(None))

  /// <summary>
  /// Cancel async computation.
  /// </summary>
  /// <param name="token">CancellationToken</param>
  member this.SetCanceled token =
    this._canceled(Utilities.createCanceledException(Some token))

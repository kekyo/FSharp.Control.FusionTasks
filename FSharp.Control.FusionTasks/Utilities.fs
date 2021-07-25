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
open System.Threading
open System.Threading.Tasks

module internal Utilities =

  let createCanceledException(token: CancellationToken option) =
    // TODO: Constructed stack traces. require?
    try
      match token with
      | Some t -> new OperationCanceledException(t) |> raise
      | None -> new OperationCanceledException() |> raise
    with :? OperationCanceledException as e -> e

  let getScheduler() =
    match SynchronizationContext.Current with
    | null -> TaskScheduler.Current
    | _ -> TaskScheduler.FromCurrentSynchronizationContext()

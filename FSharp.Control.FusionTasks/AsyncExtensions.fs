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
open System.Collections.Generic
open System.Runtime.CompilerServices
open System.Threading
open System.Threading.Tasks

#nowarn "44"

[<AutoOpen>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module AsyncExtensions =

  ///////////////////////////////////////////////////////////////////////////////////
  // F# side Async class extensions.

  type Async with

    /// <summary>
    /// Seamless conversion from F# Async to .NET Task.
    /// </summary>
    /// <param name="async">F# Async</param>
    /// <param name="token">Cancellation token (optional)</param>
    /// <returns>.NET Task instance</returns>
    static member AsTask(async: Async<unit>, ?token: CancellationToken) =
      Infrastructures.asTaskT(async, token) :> Task

    /// <summary>
    /// Seamless conversion from F# Async to .NET Task.
    /// </summary>
    /// <typeparam name="'T">Computation result type</typeparam> 
    /// <param name="async">F# Async</param>
    /// <param name="token">Cancellation token (optional)</param>
    /// <returns>.NET Task instance</returns>
    static member AsTask(async: Async<'T>, ?token: CancellationToken) =
      Infrastructures.asTaskT(async, token)

    ////////////////////////////////////////////////////////////////////
    // Operators
    
    /// <summary>
    /// Seamless conversion operator from .NET Task to F# Async.
    /// </summary>
    /// <param name="task">.NET Task instance</param>
    /// <returns>F# Async</returns>
    static member AsAsync (task: Task) =
      Infrastructures.asAsync(task, None)

    /// <summary>
    /// Seamless conversion operator from .NET Task to F# Async.
    /// </summary>
    /// <typeparam name="'T">Computation result type</typeparam> 
    /// <param name="task">.NET Task instance</param>
    /// <returns>F# Async</returns>
    static member AsAsync (task: Task<'T>) =
      Infrastructures.asAsyncT(task, None)

    /// <summary>
    /// Seamless conversion operator from .NET ValueTask to F# Async.
    /// </summary>
    /// <param name="task">.NET ValueTask instance</param>
    /// <returns>F# Async</returns>
    static member AsAsync (task: ValueTask) =
      Infrastructures.asAsyncV(task, None)

    /// <summary>
    /// Seamless conversion operator from .NET ValueTask to F# Async.
    /// </summary>
    /// <typeparam name="'T">Computation result type</typeparam> 
    /// <param name="task">.NET ValueTask instance</param>
    /// <returns>F# Async</returns>
    static member AsAsync (task: ValueTask<'T>) =
      Infrastructures.asAsyncVT(task, None)

    /// <summary>
    /// Seamless conversion operator from .NET Task to F# Async.
    /// </summary>
    /// <param name="ct">.NET Configured Task instance</param>
    /// <returns>F# Async</returns>
    static member AsAsync (ct: ConfiguredTaskAwaitable) =
      Infrastructures.asAsyncCT(ct)

    /// <summary>
    /// Seamless conversion operator from .NET Task to F# Async.
    /// </summary>
    /// <typeparam name="'T">Computation result type</typeparam> 
    /// <param name="ctt">.NET Configured Task instance</param>
    /// <returns>F# Async</returns>
    static member AsAsync (ctt: ConfiguredTaskAwaitable<'T>) =
      Infrastructures.asAsyncCTT(ctt)

    /// <summary>
    /// Seamless conversion operator from .NET ValueTask to F# Async.
    /// </summary>
    /// <param name="cvt">.NET Configured ValueTask instance</param>
    /// <returns>F# Async</returns>
    static member AsAsync (cvt: ConfiguredValueTaskAwaitable) =
      Infrastructures.asAsyncCVT(cvt)

    /// <summary>
    /// Seamless conversion operator from .NET ValueTask to F# Async.
    /// </summary>
    /// <typeparam name="'T">Computation result type</typeparam> 
    /// <param name="cvtt">.NET Configured ValueTask instance</param>
    /// <returns>F# Async</returns>
    static member AsAsync (cvtt: ConfiguredValueTaskAwaitable<'T>) =
      Infrastructures.asAsyncCVTT(cvtt)

  ///////////////////////////////////////////////////////////////////////////////////
  // F# side Task class extensions.

  type Task with
  
    /// <summary>
    /// Seamless conversion from .NET Task to F# Async.
    /// </summary>
    /// <param name="token">Cancellation token (optional)</param>
    /// <returns>F# Async instance</returns>
    member task.AsAsync(?token: CancellationToken) =
      Infrastructures.asAsync(task, token)

    /// <summary>
    /// Seamless conversionable substitution Task.ConfigureAwait()
    /// </summary>
    /// <param name="continueOnCapturedContext">True if continuation running on captured SynchronizationContext</param>
    /// <returns>ConfiguredAsyncAwaitable instance</returns>
    [<Obsolete("AsyncConfigure is compatibility on PCL environment. Use ConfigureAwait instead.")>]
    member task.AsyncConfigure(continueOnCapturedContext: bool) =
      ConfiguredTaskAsyncAwaitable(task.ConfigureAwait(continueOnCapturedContext))

  type Task<'T> with

    /// <summary>
    /// Seamless conversion from .NET Task to F# Async.
    /// </summary>
    /// <typeparam name="'T">Computation result type</typeparam> 
    /// <param name="token">Cancellation token (optional)</param>
    /// <returns>F# Async instance</returns>
    member task.AsAsync(?token: CancellationToken) =
      Infrastructures.asAsyncT(task, token)

    /// <summary>
    /// Seamless conversionable substitution Task.ConfigureAwait()
    /// </summary>
    /// <typeparam name="'T">Computation result type</typeparam> 
    /// <param name="continueOnCapturedContext">True if continuation running on captured SynchronizationContext</param>
    /// <returns>ConfiguredAsyncAwaitable instance</returns>
    [<Obsolete("AsyncConfigure is compatibility on PCL environment. Use ConfigureAwait instead.")>]
    member task.AsyncConfigure(continueOnCapturedContext: bool) =
      ConfiguredTaskAsyncAwaitable<'T>(task.ConfigureAwait(continueOnCapturedContext))

  type ValueTask with

    /// <summary>
    /// Seamless conversion from .NET ValueTask to F# Async.
    /// </summary>
    /// <param name="token">Cancellation token (optional)</param>
    /// <returns>F# Async instance</returns>
    member task.AsAsync(?token: CancellationToken) =
      Infrastructures.asAsyncV(task, token)

    /// <summary>
    /// Seamless conversionable substitution ValueTask.ConfigureAwait()
    /// </summary>
    /// <param name="continueOnCapturedContext">True if continuation running on captured SynchronizationContext</param>
    /// <returns>ConfiguredAsyncAwaitable instance</returns>
    [<Obsolete("AsyncConfigure is compatibility on PCL environment. Use ConfigureAwait instead.")>]
    member task.AsyncConfigure(continueOnCapturedContext: bool) =
      ConfiguredValueTaskAsyncAwaitable(task.ConfigureAwait(continueOnCapturedContext))

  type ValueTask<'T> with

    /// <summary>
    /// Seamless conversion from .NET ValueTask to F# Async.
    /// </summary>
    /// <typeparam name="'T">Computation result type</typeparam> 
    /// <param name="token">Cancellation token (optional)</param>
    /// <returns>F# Async instance</returns>
    member task.AsAsync(?token: CancellationToken) =
      Infrastructures.asAsyncVT(task, token)

    /// <summary>
    /// Seamless conversionable substitution ValueTask.ConfigureAwait()
    /// </summary>
    /// <typeparam name="'T">Computation result type</typeparam> 
    /// <param name="continueOnCapturedContext">True if continuation running on captured SynchronizationContext</param>
    /// <returns>ConfiguredAsyncAwaitable instance</returns>
    [<Obsolete("AsyncConfigure is compatibility on PCL environment. Use ConfigureAwait instead.")>]
    member task.AsyncConfigure(continueOnCapturedContext: bool) =
      ConfiguredValueTaskAsyncAwaitable<'T>(task.ConfigureAwait(continueOnCapturedContext))
  
  ///////////////////////////////////////////////////////////////////////////////////
  // F# side ConfiguredAsyncAwaitable class extensions.

  type ConfiguredTaskAsyncAwaitable with
  
    /// <summary>
    /// Seamless conversion from .NET Task to F# Async.
    /// </summary>
    /// <returns>F# Async instance.</returns>
    [<Obsolete("AsyncConfigure is compatibility on PCL environment. Use ConfigureAwait instead.")>]
    member cta.AsAsync() =
      Infrastructures.asAsyncCTA(cta)
 
  type ConfiguredTaskAsyncAwaitable<'T> with

    /// <summary>
    /// Seamless conversion from .NET Task to F# Async.
    /// </summary>
    /// <typeparam name="'T">Computation result type</typeparam> 
    /// <returns>F# Async instance.</returns>
    [<Obsolete("AsyncConfigure is compatibility on PCL environment. Use ConfigureAwait instead.")>]
    member cta.AsAsync() =
      Infrastructures.asAsyncCTAT(cta)

  type ConfiguredValueTaskAsyncAwaitable with

    /// <summary>
    /// Seamless conversion from .NET Task to F# Async.
    /// </summary>
    /// <returns>F# Async instance.</returns>
    [<Obsolete("AsyncConfigure is compatibility on PCL environment. Use ConfigureAwait instead.")>]
    member cta.AsAsync() =
      Infrastructures.asAsyncCVTA(cta)

  type ConfiguredValueTaskAsyncAwaitable<'T> with

    /// <summary>
    /// Seamless conversion from .NET Task to F# Async.
    /// </summary>
    /// <typeparam name="'T">Computation result type</typeparam> 
    /// <returns>F# Async instance.</returns>
    [<Obsolete("AsyncConfigure is compatibility on PCL environment. Use ConfigureAwait instead.")>]
    member cta.AsAsync() =
      Infrastructures.asAsyncCVTAT(cta)

  ///////////////////////////////////////////////////////////////////////////////////
  // F# side async computation builder extensions.

  type AsyncBuilder with

    /// <summary>
    /// Bypass default Async binder.
    /// </summary>
    /// <param name="computation">F# Async instance.</param>
    /// <returns>F# Async instance.</returns>
    member __.Source(computation: Async<'T>) =
      computation

    /// <summary>
    /// Seamless conversion from .NET Task to F# Async in Async workflow.
    /// </summary>
    /// <param name="task">.NET Task (expression result)</param>
    /// <returns>F# Async instance.</returns>
    member __.Source(task: Task) =
      Infrastructures.asAsync(task, None)

    /// <summary>
    /// Seamless conversion from .NET Task to F# Async in Async workflow.
    /// </summary>
    /// <typeparam name="'T">Computation result type</typeparam> 
    /// <param name="task">.NET Task&lt;'T&gt; (expression result)</param>
    /// <returns>F# Async instance.</returns>
    member __.Source(task: Task<'T>) =
      Infrastructures.asAsyncT(task, None)
      
    /// <summary>
    /// Seamless conversion from .NET Task to F# Async in Async workflow.
    /// </summary>
    /// <param name="ct">.NET ConfiguredTaskAwaitable (expr.ConfigureAwait(...))</param>
    /// <returns>F# Async instance.</returns>
    member __.Source(ct: ConfiguredTaskAwaitable) =
      Infrastructures.asAsyncCT(ct)

    /// <summary>
    /// Seamless conversion from .NET Task to F# Async in Async workflow.
    /// </summary>
    /// <typeparam name="'T">Computation result type</typeparam> 
    /// <param name="ctt">.NET ConfiguredTaskAwaitable&lt;'T&gt; (expr.ConfigureAwait(...))</param>
    /// <returns>F# Async instance.</returns>
    member __.Source(ctt: ConfiguredTaskAwaitable<'T>) =
      Infrastructures.asAsyncCTT(ctt)
      
    /// <summary>
    /// Seamless conversion from .NET Task to F# Async in Async workflow.
    /// </summary>
    /// <param name="task">.NET ValueTask (expression result)</param>
    /// <returns>F# Async instance.</returns>
    member __.Source(task: ValueTask) =
      Infrastructures.asAsyncV(task, None)
      
    /// <summary>
    /// Seamless conversion from .NET Task to F# Async in Async workflow.
    /// </summary>
    /// <typeparam name="'T">Computation result type</typeparam> 
    /// <param name="task">.NET ValueTask&lt;'T&gt; (expression result)</param>
    /// <returns>F# Async instance.</returns>
    member __.Source(task: ValueTask<'T>) =
      Infrastructures.asAsyncVT(task, None)

    /// <summary>
    /// Seamless conversion from .NET Task to F# Async in Async workflow.
    /// </summary>
    /// <param name="cvt">.NET ConfiguredValueTaskAwaitable (expr.ConfigureAwait(...))</param>
    /// <returns>F# Async instance.</returns>
    member __.Source(cvt: ConfiguredValueTaskAwaitable) =
      Infrastructures.asAsyncCVT(cvt)
      
    /// <summary>
    /// Seamless conversion from .NET Task to F# Async in Async workflow.
    /// </summary>
    /// <typeparam name="'T">Computation result type</typeparam> 
    /// <param name="cvtt">.NET ConfiguredValueTaskAwaitable&lt;'T&gt; (expr.ConfigureAwait(...))</param>
    /// <returns>F# Async instance.</returns>
    member __.Source(cvtt: ConfiguredValueTaskAwaitable<'T>) =
      Infrastructures.asAsyncCVTT(cvtt)
      
    /// <summary>
    /// Accept any sequence type to support `for .. in` expressions in Async workflows.
    /// </summary>
    /// <typeparam name="'E">The element type of the sequence</typeparam> 
    /// <param name="s">The sequence.</param>
    /// <returns>The sequence.</returns>
    member __.Source(s: 'E seq) =
      s

    /// <summary>
    /// Seamless conversion from .NET Task to F# Async in Async workflow.
    /// </summary>
    /// <param name="cta">.NET ConfiguredTaskAsyncAwaitable (expr.AsyncConfigure(...))</param>
    /// <returns>F# Async instance.</returns>
    [<Obsolete("AsyncConfigure is compatibility on PCL environment. Use ConfigureAwait instead.")>]
    member __.Source(cta: ConfiguredTaskAsyncAwaitable) =
      Infrastructures.asAsyncCTA(cta)

    /// <summary>
    /// Seamless conversion from .NET Task to F# Async in Async workflow.
    /// </summary>
    /// <typeparam name="'T">Computation result type</typeparam> 
    /// <param name="cta">.NET ConfiguredTaskAsyncAwaitable&lt;'T&gt; (expr.AsyncConfigure(...))</param>
    /// <returns>F# Async instance.</returns>
    [<Obsolete("AsyncConfigure is compatibility on PCL environment. Use ConfigureAwait instead.")>]
    member __.Source(cta: ConfiguredTaskAsyncAwaitable<'T>) =
      Infrastructures.asAsyncCTAT(cta)
 
    /// <summary>
    /// Seamless conversion from .NET Task to F# Async in Async workflow.
    /// </summary>
    /// <param name="cta">.NET ConfiguredValueTaskAsyncAwaitable (expr.AsyncConfigure(...))</param>
    /// <returns>F# Async instance.</returns>
    [<Obsolete("AsyncConfigure is compatibility on PCL environment. Use ConfigureAwait instead.")>]
    member __.Source(cta: ConfiguredValueTaskAsyncAwaitable) =
      Infrastructures.asAsyncCVTA(cta)
 
    /// <summary>
    /// Seamless conversion from .NET Task to F# Async in Async workflow.
    /// </summary>
    /// <typeparam name="'T">Computation result type</typeparam> 
    /// <param name="cta">.NET ConfiguredValueTaskAsyncAwaitable&lt;'T&gt; (expr.AsyncConfigure(...))</param>
    /// <returns>F# Async instance.</returns>
    [<Obsolete("AsyncConfigure is compatibility on PCL environment. Use ConfigureAwait instead.")>]
    member __.Source(cta: ConfiguredValueTaskAsyncAwaitable<'T>) =
      Infrastructures.asAsyncCVTAT(cta)

#if !NET45 && !NETSTANDARD1_6 && !NETCOREAPP2_0

    /// <summary>
    /// Seamless conversion from .NET Async disposer (IAsyncDisposable) to F# Async in Async workflow.
    /// </summary>
    /// <param name="disposable">.NET IAsyncDisposable</param>
    /// <param name="body">use expression body</param>
    /// <returns>F# Async instance.</returns>
    member __.Using(disposable: 'T :> IAsyncDisposable, body: 'T -> Async<'R>) : Async<'R> =
      Infrastructures.asAsyncAD(disposable, body)

    /// <summary>
    /// Seamless conversion from .NET Async sequence (IAsyncEnumerable&lt;'E&gt;) to F# Async in Async workflow.
    /// </summary>
    /// <typeparam name="'E">The element type of the sequence</typeparam> 
    /// <param name="enumerable">.NET IAsyncEnumerable&lt;'E&gt; (expression result)</param>
    /// <param name="body">for expression body</param>
    /// <returns>F# Async instance.</returns>
    member __.For(enumerable: IAsyncEnumerable<'E>, body: 'E -> Async<'R>) =
      Infrastructures.asAsyncAE(enumerable, body, None)

    /// <summary>
    /// Seamless conversion from .NET Async sequence (IAsyncEnumerable&lt;'E&gt;) to F# Async in Async workflow.
    /// </summary>
    /// <typeparam name="'E">The element type of the sequence</typeparam> 
    /// <param name="enumerable">.NET IAsyncEnumerable&lt;'E&gt; (expression result)</param>
    /// <param name="body">for expression body</param>
    /// <returns>F# Async instance.</returns>
    member __.For(enumerable: ConfiguredCancelableAsyncEnumerable<'E>, body: 'E -> Async<'R>) =
      Infrastructures.asAsyncCCAE(enumerable, body)

    /// <summary>
    /// Accept any async sequence type to support `for .. in` expressions in Async workflows.
    /// </summary>
    /// <typeparam name="'E">The element type of the sequence</typeparam> 
    /// <param name="enumerable">.NET IAsyncEnumerable&lt;'E&gt; (expression result)</param>
    /// <returns>The sequence.</returns>
    member __.Source(enumerable: IAsyncEnumerable<'E>) =
      enumerable

    /// <summary>
    /// Accept any async sequence type to support `for .. in` expressions in Async workflows.
    /// </summary>
    /// <typeparam name="'E">The element type of the sequence</typeparam> 
    /// <param name="enumerable">.NET ConfiguredCancelableAsyncEnumerable&lt;'E&gt; (expression result)</param>
    /// <returns>The sequence.</returns>
    member __.Source(enumerable: ConfiguredCancelableAsyncEnumerable<'E>) =
      enumerable
#endif

/////////////////////////////////////////////////////////////////////////////////////////////////
//
// FSharp.Control.FusionTasks - F# Async workflow <--> .NET Task easy seamless interoperability library.
// Copyright (c) 2016 Kouji Matsui (@kekyo2)
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

///////////////////////////////////////////////////////////////////////////////////

namespace System.Threading.Tasks

open System.Runtime.CompilerServices
open System.Threading
open Microsoft.FSharp.Control

/// <summary>
/// Seamless conversion extensions in standard .NET Task based infrastructure.
/// </summary>
[<Extension; Sealed; AbstractClass; NoEquality; NoComparison; AutoSerializable(false)>]
type TaskExtensions =

  ///////////////////////////////////////////////////////////////////////////////////
  // .NET (C#) side Task --> Async conversion extensions.

  /// <summary>
  /// Seamless conversion from .NET Task to F# Async.
  /// </summary>
  /// <param name="task">.NET Task</param>
  /// <returns>F# Async (FSharpAsync&lt;Unit&gt;)</returns>
  [<Extension>]
  static member AsAsync (task: Task) =
    Infrastructures.asAsync (task, None)

  /// <summary>
  /// Seamless conversion from .NET Task to F# Async.
  /// </summary>
  /// <param name="task">.NET Task</param>
  /// <param name="token">Cancellation token</param>
  /// <returns>F# Async (FSharpAsync&lt;Unit&gt;)</returns>
  [<Extension>]
  static member AsAsync (task: Task, token: CancellationToken) =
    Infrastructures.asAsync (task, Some token)

  /// <summary>
  /// Seamless conversion from .NET Task to F# Async.
  /// </summary>
  /// <param name="task">.NET Task</param>
  /// <param name="continueOnCapturedContext">True if continuation running on captured SynchronizationContext</param>
  /// <returns>F# Async (FSharpAsync&lt;Unit&gt;)</returns>
  [<Extension>]
  static member AsAsyncConfigured (task: Task, continueOnCapturedContext: bool) =
    Infrastructures.asAsyncCTA (ConfiguredTaskAsyncAwaitable(task.ConfigureAwait(continueOnCapturedContext)))

  /// <summary>
  /// Seamless conversion from .NET Task to F# Async.
  /// </summary>
  /// <typeparam name="'T">Computation result type</typeparam> 
  /// <param name="task">.NET Task&lt;'T&gt;</param>
  /// <returns>F# Async&lt;'T&gt; (FSharpAsync&lt;'T&gt;)</returns>
  [<Extension>]
  static member AsAsync (task: Task<'T>) =
    Infrastructures.asAsyncT (task, None)

  /// <summary>
  /// Seamless conversion from .NET Task to F# Async.
  /// </summary>
  /// <typeparam name="'T">Computation result type</typeparam> 
  /// <param name="task">.NET Task&lt;'T&gt;</param>
  /// <param name="token">Cancellation token</param>
  /// <returns>F# Async&lt;'T&gt; (FSharpAsync&lt;'T&gt;)</returns>
  [<Extension>]
  static member AsAsync (task: Task<'T>, token: CancellationToken) =
    Infrastructures.asAsyncT (task, Some token)

  /// <summary>
  /// Seamless conversion from .NET Task to F# Async.
  /// </summary>
  /// <typeparam name="'T">Computation result type</typeparam> 
  /// <param name="task">.NET Task&lt;'T&gt;</param>
  /// <param name="continueOnCapturedContext">True if continuation running on captured SynchronizationContext</param>
  /// <returns>F# Async&lt;'T&gt; (FSharpAsync&lt;'T&gt;)</returns>
  [<Extension>]
  static member AsAsyncConfigured (task: Task<'T>, continueOnCapturedContext: bool) =
    Infrastructures.asAsyncCTAT (ConfiguredTaskAsyncAwaitable<'T>(task.ConfigureAwait(continueOnCapturedContext)))

#if NET45 || PCL7 || PCL78 || PCL259 || NETSTANDARD1_6
/// <summary>
/// Seamless conversion extensions in standard .NET ValueTask based infrastructure.
/// </summary>
[<Extension; Sealed; AbstractClass; NoEquality; NoComparison; AutoSerializable(false)>]
type ValueTaskExtensions =

  ///////////////////////////////////////////////////////////////////////////////////
  // .NET (C#) side ValueTask --> Async conversion extensions.

  /// <summary>
  /// Seamless conversion from .NET ValueTask to F# Async.
  /// </summary>
  /// <typeparam name="'T">Computation result type</typeparam> 
  /// <param name="task">.NET ValueTask&lt;'T&gt;</param>
  /// <returns>F# Async&lt;'T&gt; (FSharpAsync&lt;'T&gt;)</returns>
  [<Extension>]
  static member AsAsync (task: ValueTask<'T>) =
    Infrastructures.asAsyncVT (task, None)

  /// <summary>
  /// Seamless conversion from .NET ValueTask to F# Async.
  /// </summary>
  /// <typeparam name="'T">Computation result type</typeparam> 
  /// <param name="task">.NET ValueTask&lt;'T&gt;</param>
  /// <param name="token">Cancellation token</param>
  /// <returns>F# Async&lt;'T&gt; (FSharpAsync&lt;'T&gt;)</returns>
  [<Extension>]
  static member AsAsync (task: ValueTask<'T>, token: CancellationToken) =
    Infrastructures.asAsyncVT (task, Some token)

  /// <summary>
  /// Seamless conversion from .NET ValueTask to F# Async.
  /// </summary>
  /// <typeparam name="'T">Computation result type</typeparam> 
  /// <param name="task">.NET ValueTask&lt;'T&gt;</param>
  /// <param name="continueOnCapturedContext">True if continuation running on captured SynchronizationContext</param>
  /// <returns>F# Async&lt;'T&gt; (FSharpAsync&lt;'T&gt;)</returns>
  [<Extension>]
  static member AsAsyncConfigured (task: ValueTask<'T>, continueOnCapturedContext: bool) =
    Infrastructures.asAsyncCVTAT (ConfiguredValueTaskAsyncAwaitable<'T>(task.ConfigureAwait(continueOnCapturedContext)))
#endif

///////////////////////////////////////////////////////////////////////////////////

namespace Microsoft.FSharp.Control

open System.Runtime.CompilerServices
open System.Threading
open System.Threading.Tasks

/// <summary>
/// Seamless conversion extensions in standard .NET Task based infrastructure.
/// </summary>
[<Extension; Sealed; AbstractClass; NoEquality; NoComparison; AutoSerializable(false)>]
type AsyncExtensions =

  ///////////////////////////////////////////////////////////////////////////////////
  // .NET (C#) side Async --> Task conversion extensions.

  /// <summary>
  /// Seamless conversion from F# Async to .NET Task.
  /// </summary>
  /// <param name="async">F# Async (FSharpAsync&lt;Unit&gt;)</param>
  /// <returns>.NET Task</returns>
  [<Extension>]
  static member AsTask (async: Async<unit>) =
    Infrastructures.asTask (async, None) :> Task

  /// <summary>
  /// Seamless conversion from F# Async to .NET Task.
  /// </summary>
  /// <param name="async">F# Async (FSharpAsync&lt;Unit&gt;)</param>
  /// <param name="token">Cancellation token</param>
  /// <returns>.NET Task</returns>
  [<Extension>]
  static member AsTask (async: Async<unit>, token: CancellationToken) =
    Infrastructures.asTask (async, Some token) :> Task

  /// <summary>
  /// Seamless conversion from F# Async to .NET Task.
  /// </summary>
  /// <typeparam name="'T">Computation result type</typeparam> 
  /// <param name="async">F# Async&lt;'T&gt; (FSharpAsync&lt;'T&gt;)</param>
  /// <returns>.NET Task&lt;'T&gt;</returns>
  [<Extension>]
  static member AsTask (async: Async<'T>) =
    Infrastructures.asTask (async, None)

  /// <summary>
  /// Seamless conversion from F# Async to .NET Task.
  /// </summary>
  /// <typeparam name="'T">Computation result type</typeparam> 
  /// <param name="async">F# Async&lt;'T&gt; (FSharpAsync&lt;'T&gt;)</param>
  /// <param name="token">Cancellation token</param>
  /// <returns>.NET Task&lt;'T&gt;</returns>
  [<Extension>]
  static member AsTask (async: Async<'T>, token: CancellationToken) =
    Infrastructures.asTask (async, Some token)

#if NET45 || PCL7 || PCL78 || PCL259 || NETSTANDARD1_6
  ///////////////////////////////////////////////////////////////////////////////////
  // .NET (C#) side Async --> ValueTask conversion extensions.

  /// <summary>
  /// Seamless conversion from F# Async to .NET ValueTask.
  /// </summary>
  /// <typeparam name="'T">Computation result type</typeparam> 
  /// <param name="async">F# Async&lt;'T&gt; (FSharpAsync&lt;'T&gt;)</param>
  /// <returns>.NET ValueTask&lt;'T&gt;</returns>
  [<Extension>]
  static member AsValueTask (async: Async<'T>) =
    Infrastructures.asValueTask (async, None)

  /// <summary>
  /// Seamless conversion from F# Async to .NET ValueTask.
  /// </summary>
  /// <typeparam name="'T">Computation result type</typeparam> 
  /// <param name="async">F# Async&lt;'T&gt; (FSharpAsync&lt;'T&gt;)</param>
  /// <param name="token">Cancellation token</param>
  /// <returns>.NET ValueTask&lt;'T&gt;</returns>
  [<Extension>]
  static member AsValueTask (async: Async<'T>, token: CancellationToken) =
    Infrastructures.asValueTask (async, Some token)
#endif

  ///////////////////////////////////////////////////////////////////////////////////
  // .NET (C#) side Async configurable extensions.

  /// <summary>
  /// Seamless configuring context support for F# Async.
  /// </summary>
  /// <param name="async">F# Async (FSharpAsync&lt;Unit&gt;)</param>
  /// <param name="continueOnCapturedContext">True if continuation running on captured SynchronizationContext</param>
  /// <returns>.NET TaskAwaiter</returns>
  [<Extension>]
  static member ConfigureAwait (async: Async<unit>, continueOnCapturedContext: bool) =
    let task = Infrastructures.asTask (async, None) :> Task
    ConfiguredTaskAsyncAwaitable(task.ConfigureAwait(continueOnCapturedContext))

  /// <summary>
  /// Seamless configuring context support for F# Async.
  /// </summary>
  /// <typeparam name="'T">Computation result type</typeparam> 
  /// <param name="async">F# Async&lt;'T&gt; (FSharpAsync&lt;'T&gt;)</param>
  /// <param name="continueOnCapturedContext">True if continuation running on captured SynchronizationContext</param>
  /// <returns>.NET TaskAwaiter&lt;'T&gt;</returns>
  [<Extension>]
  static member ConfigureAwait (async: Async<'T>, continueOnCapturedContext: bool) =
    let task = Infrastructures.asTask (async, None)
    ConfiguredTaskAsyncAwaitable<'T>(task.ConfigureAwait(continueOnCapturedContext))

  ///////////////////////////////////////////////////////////////////////////////////
  // .NET (C#) side Async awaitabler extensions.

  /// <summary>
  /// Seamless awaiter support for F# Async.
  /// </summary>
  /// <param name="async">F# Async (FSharpAsync&lt;Unit&gt;)</param>
  /// <returns>.NET TaskAwaiter</returns>
  [<Extension>]
  static member GetAwaiter (async: Async<unit>) =
    let task = Infrastructures.asTask (async, None) :> Task
    AsyncAwaiter(task.GetAwaiter())

  /// <summary>
  /// Seamless awaiter support for F# Async.
  /// </summary>
  /// <typeparam name="'T">Computation result type</typeparam> 
  /// <param name="async">F# Async&lt;'T&gt; (FSharpAsync&lt;'T&gt;)</param>
  /// <returns>.NET TaskAwaiter&lt;'T&gt;</returns>
  [<Extension>]
  static member GetAwaiter (async: Async<'T>) =
    let task = Infrastructures.asTask (async, None)
    AsyncAwaiter<'T>(task.GetAwaiter())

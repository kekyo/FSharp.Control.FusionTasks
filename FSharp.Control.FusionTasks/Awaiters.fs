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
open System.Threading
open System.Threading.Tasks

#if PCL47
open Microsoft.Runtime.CompilerServices
#else
open System.Runtime.CompilerServices
#endif

// Provide Awaitable/Awaiter on custom codes:
//   PCL's Awaitable/Awaiter on "Microsoft.Runtime.CompilerServices",
//   but not decorated TypeForwardedTo attributes.
//   So if referenced Awaitable/Awaiter types, cause cannot found Awaitable/Awaiter types.
//   (Awaitable/Awaiter types defined on "System.Runtime.CompilerServices" real assembly)
//   FusionTasks redefined custom Awaitable/Awaiter types, exclude CompilerServices's type references.

///////////////////////////////////////////////////////////////////////////////////
// AsyncAwaiter.

/// <summary>
/// F# Async's awaiter implementation. This structure using implicitly.
/// </summary>
[<Struct; NoEquality; NoComparison; AutoSerializable(false)>]
type AsyncAwaiter internal (ta: TaskAwaiter) =

    member __.IsCompleted = ta.IsCompleted
    member __.OnCompleted(continuation: Action) = ta.OnCompleted(continuation)
    member __.UnsafeOnCompleted(continuation: Action) = ta.UnsafeOnCompleted(continuation)
    member __.GetResult() = ta.GetResult()

    interface System.Runtime.CompilerServices.INotifyCompletion with
       member __.OnCompleted(continuation: Action) = ta.OnCompleted(continuation)
  
/// <summary>
/// F# Async's awaiter implementation. This structure using implicitly.
/// </summary>
[<Struct; NoEquality; NoComparison; AutoSerializable(false)>]
type AsyncAwaiter<'T> internal (ta: TaskAwaiter<'T>) =

    member __.IsCompleted = ta.IsCompleted
    member __.OnCompleted(continuation: Action) = ta.OnCompleted(continuation)
    member __.UnsafeOnCompleted(continuation: Action) = ta.UnsafeOnCompleted(continuation)
    member __.GetResult() = ta.GetResult()
    
    interface System.Runtime.CompilerServices.INotifyCompletion with
       member __.OnCompleted(continuation: Action) = ta.OnCompleted(continuation)

///////////////////////////////////////////////////////////////////////////////////
// ConfiguredTaskAsyncAwaiter.

/// <summary>
/// F# Async's awaiter implementation. This structure using implicitly.
/// </summary>
[<Struct; NoEquality; NoComparison; AutoSerializable(false)>]
type ConfiguredTaskAsyncAwaiter internal (ctacta: ConfiguredTaskAwaitable.ConfiguredTaskAwaiter) =

    member __.IsCompleted = ctacta.IsCompleted
    member __.OnCompleted(continuation: Action) = ctacta.OnCompleted(continuation)
    member __.UnsafeOnCompleted(continuation: Action) = ctacta.UnsafeOnCompleted(continuation)
    member __.GetResult() = ctacta.GetResult()
    
    interface System.Runtime.CompilerServices.INotifyCompletion with
       member __.OnCompleted(continuation: Action) = ctacta.OnCompleted(continuation)

/// <summary>
/// F# Async's awaitable implementation. This structure using implicitly.
/// </summary>
[<Struct; NoEquality; NoComparison; AutoSerializable(false)>]
type ConfiguredTaskAsyncAwaitable internal (cta: ConfiguredTaskAwaitable) =

    member __.GetAwaiter() = ConfiguredTaskAsyncAwaiter(cta.GetAwaiter())
  
/// <summary>
/// F# Async's awaiter implementation. This structure using implicitly.
/// </summary>
[<Struct; NoEquality; NoComparison; AutoSerializable(false)>]
type ConfiguredTaskAsyncAwaiter<'T> internal (ctacta: ConfiguredTaskAwaitable<'T>.ConfiguredTaskAwaiter) =

    member __.IsCompleted = ctacta.IsCompleted
    member __.OnCompleted(continuation: Action) = ctacta.OnCompleted(continuation)
    member __.UnsafeOnCompleted(continuation: Action) = ctacta.UnsafeOnCompleted(continuation)
    member __.GetResult() = ctacta.GetResult()
        
    interface System.Runtime.CompilerServices.INotifyCompletion with
       member __.OnCompleted(continuation: Action) = ctacta.OnCompleted(continuation)

/// <summary>
/// F# Async's awaitable implementation. This structure using implicitly.
/// </summary>
[<Struct; NoEquality; NoComparison; AutoSerializable(false)>]
type ConfiguredTaskAsyncAwaitable<'T> internal (cta: ConfiguredTaskAwaitable<'T>) =

    member __.GetAwaiter() = ConfiguredTaskAsyncAwaiter<'T>(cta.GetAwaiter())

#if NET45 || PCL7 || PCL78 || PCL259 || NETSTANDARD1_6 || NETSTANDARD2_0 || NETCOREAPP2_0
///////////////////////////////////////////////////////////////////////////////////
// ConfiguredValueTaskAsyncAwaiter.

/// <summary>
/// F# Async's awaiter implementation. This structure using implicitly.
/// </summary>
[<Struct; NoEquality; NoComparison; AutoSerializable(false)>]
type ConfiguredValueTaskAsyncAwaiter<'T> internal (ctacta: ConfiguredValueTaskAwaitable<'T>.ConfiguredValueTaskAwaiter) =

    member __.IsCompleted = ctacta.IsCompleted
    member __.OnCompleted(continuation: Action) = ctacta.OnCompleted(continuation)
    member __.UnsafeOnCompleted(continuation: Action) = ctacta.UnsafeOnCompleted(continuation)
    member __.GetResult() = ctacta.GetResult()
        
    interface System.Runtime.CompilerServices.INotifyCompletion with
       member __.OnCompleted(continuation: Action) = ctacta.OnCompleted(continuation)

/// <summary>
/// F# Async's awaitable implementation. This structure using implicitly.
/// </summary>
[<Struct; NoEquality; NoComparison; AutoSerializable(false)>]
type ConfiguredValueTaskAsyncAwaitable<'T> internal (cta: ConfiguredValueTaskAwaitable<'T>) =

    member __.GetAwaiter() = ConfiguredValueTaskAsyncAwaiter<'T>(cta.GetAwaiter())
#endif

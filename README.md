# F# FusionTasks
![FusionTasks](https://raw.githubusercontent.com/kekyo/FSharp.Control.FusionTasks/master/Images/FSharp.Control.FusionTasks.128.png)

## Status
| | Current |
|:---|:--:|
| NuGet Package (F# 4.5) | [![NuGet FusionTasks (F# 4.5)](https://img.shields.io/nuget/v/FSharp.Control.FusionTasks.svg?style=flat)](https://www.nuget.org/packages/FSharp.Control.FusionTasks) |
| (Older below, archived) | ---
| NuGet Package (F# 4.1) | [![NuGet FusionTasks (F# 4.1)](https://img.shields.io/nuget/v/FSharp.Control.FusionTasks.FS41.svg?style=flat)](https://www.nuget.org/packages/FSharp.Control.FusionTasks.FS41) |
| NuGet Package (F# 4.0) | [![NuGet FusionTasks (F# 4.0)](https://img.shields.io/nuget/v/FSharp.Control.FusionTasks.FS40.svg?style=flat)](https://www.nuget.org/packages/FSharp.Control.FusionTasks.FS40) |
| NuGet Package (F# 3.1) | [![NuGet FusionTasks (F# 3.1)](https://img.shields.io/nuget/v/FSharp.Control.FusionTasks.FS31.svg?style=flat)](https://www.nuget.org/packages/FSharp.Control.FusionTasks.FS31) |
| NuGet Package (F# 3.0) | [![NuGet FusionTasks (F# 3.0)](https://img.shields.io/nuget/v/FSharp.Control.FusionTasks.FS30.svg?style=flat)](https://www.nuget.org/packages/FSharp.Control.FusionTasks.FS30) |
| NuGet Package (F# 2.0) | [![NuGet FusionTasks (F# 2.0)](https://img.shields.io/nuget/v/FSharp.Control.FusionTasks.FS20.svg?style=flat)](https://www.nuget.org/packages/FSharp.Control.FusionTasks.FS20) |
| Continuous integration | [![AppVeyor FusionTasks](https://img.shields.io/appveyor/ci/kekyo/fsharp-control-fusiontasks.svg?style=flat)](https://ci.appveyor.com/project/kekyo/fsharp-control-fusiontasks) |
| Gitter | [![Gitter FusionTasks](https://badges.gitter.im/kekyo/FSharp.Control.FusionTasks.svg)](https://gitter.im/kekyo/FSharp.Control.FusionTasks?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge) |

## What is this?
* F# Async workflow <--> .NET Task/ValueTask easy seamless interoperability library.
* Sample codes (F# side):

``` fsharp
let asyncTest = async {
  use ms = new MemoryStream()

  // FusionTasks directly interpreted System.Threading.Tasks.Task class in F# async-workflow block.
  do! ms.WriteAsync(data, 0, data.Length)
  do ms.Position <- 0L

  // FusionTasks directly interpreted System.Threading.Tasks.Task<T> class in F# async-workflow block.
  let! length = ms.ReadAsync(data2, 0, data2.Length)
  do length |> should equal data2.Length
}
```

* Sample codes (C# side):

``` csharp
using System.Threading.Tasks;
using Microsoft.FSharp.Control;

public async Task AsyncTest(FSharpAsync<int> asyncIntComp)
{
  // FusionTasks simple usage F#'s Async<unit> direct awaitable.
  await FSharpAsync.Sleep(500);
  Console.WriteLine("Awaited F# async function (unit).");

  // FusionTasks simple usage F#'s Async<int> direct awaitable.
  var result = await asyncIntComp;
  Console.WriteLine("Awaited F# async function: Result=" + result);
}
```

## Features
* Easy interoperability .NET Task/ValueTask <--> F#'s Async.
* F# async workflow block now support direct .NET Task/ValueTask handle with let!, do! and use!.
* .NET (C# async-await) now support directly F#'s Async.
* SyncronizationContext capture operation support (F#: AsyncConfigure method / .NET (C#) AsAsyncConfigured method)

## Benefits
* Easy interoperability, combination and relation standard .NET OSS packages using Task/ValueTask and F#'s Async.
* F# 4.5 with .NET Framework 4.5 (or higher), .NET Core 2.0 and .NET Standard 1.6/2.0 (or higher).
* Ready to LINQPad 5.

## Environments
* .NET Framework 4.5 or higher
* .NET Core 2.0 or higher
* .NET Standard 1.6/2.0 or higher
* F# 4.5 or higher

## How to use
* Search NuGet package and install "FSharp.Control.FusionTasks".
* F# use, autoopen'd namespace "FSharp.Control". "System.Threading.Tasks" is optional.
* C# use, using namespace "System.Threading.Tasks". "Microsoft.FSharp.Control" is optional.

## Samples

### Basic async workflow:

``` fsharp
let asyncTest = async {
  use ms = new MemoryStream()

  // FusionTasks directly interpreted System.Threading.Tasks.Task class in F# async-workflow block.
  // Sure, non-generic Task mapping to Async<unit>.
  do! ms.WriteAsync(data, 0, data.Length)
  do ms.Position <- 0L

  // FusionTasks directly interpreted System.Threading.Tasks.Task<T> class in F# async-workflow block.
  // Standard usage, same as manually used Async.AwaitTask.
  let! length = ms.ReadAsync(data2, 0, data2.Length)
  do length |> should equal data2.Length
}
```

### Without async workflow:

``` fsharp
use ms = new MemoryStream()

// Manually conversion by "AsAsync" : Task<T> --> Async<'T>
let length = ms.ReadAsync(data, 0, data.Length).AsAsync() |> Async.RunSynchronosly
```

### Without async workflow (CancellationToken):

``` fsharp
use ms = new MemoryStream()
let cts = new CancellationTokenSource()

// Produce with CancellationToken:
// TIPS: FusionTasks cannot handle directly CancellationToken IN ASYNC WORKFLOW.
//   Because async workflow semantics implicitly handled CancellationToken with Async.DefaultCancellationToken, CancellationToken and CancelDefaultToken().
//   (CancellationToken derived from Async.StartWithContinuations() in async workflow.)
let length = ms.ReadAsync(data, 0, data.Length).AsAsync(cts.Token) |> Async.RunSynchronosly
```

### Handle Task.ConfigureAwait(...)  (Capture/release SynchContext):

``` fsharp
let asyncTest = async {
  use ms = new MemoryStream()

  // Task<T> --> ConfiguredAsyncAwaitable<'T> :
  // Why use AsyncConfigure() instead ConfigureAwait() ?
  //   Because the "ConfiguredTaskAwaitable<T>" lack declare the TypeForwardedTo attribute in some PCL.
  //   If use AsyncConfigure(), complete hidden refer ConfiguredTaskAwaitable into FusionTasks assembly,
  //   avoid strange linking errors.
  let! length = ms.ReadAsync(data, 0, data.Length).AsyncConfigure(false)
}
```

### Delegate async continuation - works like TaskCompletionSource&lt;T&gt;:

``` fsharp
open System.Threading

let asyncCalculate() =
  // Create AsyncCompletionSource<'T>.
  let acs = new AsyncCompletionSource<int>()

  // Execution with completely independent another thread...
  let thread = new Thread(new ThreadStart(fun _ ->
    Thread.Sleep(5000)
    // If you captured thread context (normally continuation or callbacks),
    // can delegation async continuation using AsyncCompletionSource<'T>.
    acs.SetResult(123 * 456)))
  thread.Start()

  // Async<'T> instance
  acs.Async
```

### TIPS: Must adding annotation for arguments if using it async workflow:

``` fsharp
let asyncInner arg0 = async {
  // Cause FS0041:
  //   A unique overload for method 'Source' could not be determined based on type information prior to this program point.
  //   A type annotation may be needed.
  //  --> Because F# compiler conflict arg0 type inferences: Async<int> or Task<int>.
  let! result = arg0
  let calculated = result + 1
  printfn "%d" calculated
}

// Fixed with type annotation Async<'T> or Task<'T>:
let asyncInner (arg0:Async<_>) = async {
  let! result = arg0
  let calculated = result + 1
  printfn "%d" calculated
}
```

### In C# side:
* Really need sample codes? huh? :)

### Easy LINQPad 5 driven:
* Before setup NuGet package (FSharp.Control.FusionTasks.FS40) the LINQPad NuGet Manager.

``` fsharp
open System.IO

// Result is Async<byte[]>
let asyncSequenceData =
  let r = new Random()
  let data = [| for i = 1 to 100 do yield byte (r.Next()) |]
  async {
    use fs = new MemoryStream()
    do! fs.WriteAsync(data, 0, data.Length)
    do! fs.FlushAsync()
    return fs.ToArray()
  }

// Convert to Task<byte[]> and dump:
asyncSequenceData.AsTask().Dump()
```

![LINQPad 5 driven](https://raw.githubusercontent.com/kekyo/FSharp.Control.FusionTasks/master/Images/linqpad5.png)

## "task-like" and ValueTask appendix

* .NET add new "task-like" type. "task-like" means applied a attribute "System.Runtime.CompilerServices.AsyncMethodBuilderAttribute" and declared the async method builder.
* ValueTask overview:
  * New standard "task-like" type named for "ValueTask&lt;T&gt;" for C#. FusionTasks supported ValueTask&lt;T&gt; on 1.0.20.
  * ValueTask&lt;T&gt; declared by struct (Value type) for goal is improvement performance. But this type has the Task&lt;T&gt; instance inside and finally continuation handle by Task&lt;T&gt;.
  * ValueTask&lt;T&gt; performance effective situation maybe chatty-call fragments using both caller C# and awaiter C# codes...
  * ValueTask&lt;T&gt; a little bit or no effect improvement performance, because usage of senario for FusionTasks.
* "task-like" augumenting is difficult:
  * The attribute "AsyncMethodBuilderAttribute" must apply TARGET task-like type.
  * Means if already declared type (Sure, we have FSharpAsync&lt;'T&gt;) cannot augument and cannot turn to task-like type.
  * Therefore cannot directly return for FSharpAsync&lt;'T&gt; from C#'s async-await method.
  * And cannot auto handle task-like type by FusionTasks, because no type safe declaration for task-like type...
    * For example, if force support task-like type, FusionTasks require augument "Source: taskLike: obj -> FSharpAsync&lt;'T&gt;" overload on FSharpAsync&lt;'T&gt;. This cannot type safe.
* Conclusion:
  * So FusionTasks support only "ValueTask&lt;T&gt;" type and cannot support any other "task-like" types.

## Additional resources
* Source codes available only "FSharp.Control.FusionTasks" folder.
* The slides: "How to meets Async and Task" in Seattle F# Users group "MVP Summit Special: A Night of Lightning Talks" 2016.11.09 http://www.slideshare.net/kekyo/asyncs-vs-tasks

<iframe src="https://www.slideshare.net/slideshow/embed_code/68424602" width="800" height="500" frameborder="0" marginwidth="0" marginheight="0" scrolling="no"></iframe>

## TODO
 Improvements more easier/effective interfaces.

## License
* Copyright (c) 2016-2018 Kouji Matsui (@kozy_kekyo)
* Under Apache v2 http://www.apache.org/licenses/LICENSE-2.0

## History
* 2.0.0:
  * Supported F# 4.5, .NET Standard 2.0 and .NET Core 2.0.
  * Sorry, archived all PCL's libraries. Now FusionTasks supports only .NET Framework 4.5, .NET Core 2.0 and .NET Standard 1.6/2.0.
  * Solution structure refablished. Changed to .NET Core modern-style.
* 1.1.1:
  * Add ValueTask<'T> bind source overload.
* 1.1.0:
  * Supported F# 4.1 and .NET Standard 1.6. (Unfortunately deprecated FS40.netcore (netstandard1.4) package, try to migrate to F# 4.1 :)
* 1.0.20:
  * Support ValueTask&lt;T&gt; (Exclude net40 and Profile 47 platform, added dependency for System.Threading.Tasks.Extensions).
  * Update version for .NET Core F# (1.0.0-alpha-161205).
* 1.0.13:
  * Reduce to only contains .NET Core's assembly in FS40.netcore package.
  * Refactor folder structures.
* 1.0.12:
  * Add .NET Core support (Separated package: FSharp.Control.FusionTasks.FS40.netcore with -Pre option required)
* 1.0.2:
  * Support 'for .. in' expressions. (Thx Armin!)
* 1.0.1:
  * Fixed cause undefined Async&lt;'T&gt; using combination Async&lt;'T&gt; and Task/Task&lt;T&gt; in async workflow. (Thx Honza!)
* 1.0.0:
  * RTM release :clap:
  * Add FSharp.Core NuGet references.
  * Temporary disable support .NET Core. If reached F# RTM, continue development... (PR welcome!!)
  * Add sample codes.
* 0.9.6:
  * WIP release.
* 0.9.5:
  * WIP release.
* 0.9.4:
  * Fixed nuspec reference System, System.Core
* 0.9.3:
  * Fixed nuspec frameworkAssemblies.
* 0.9.2:
  * Add package targetFramework.
  * Updated RelaxVersioner.
* 0.9.1:
  * Remove strongly-signed (Unit test doesn't work...)
  * Omit synchronizers (AsyncLock, AsyncLazy). Thats moving to FSharp.Control.AsyncPrimitives project (https://github.com/kekyo/FSharp.Control.AsyncPrimitives).
  * Add target dnxcore50 into F# 4.0 (for .NET Core 1.0)
  * Source codes and documents bit changed.
* 0.5.8:
  * Add strongly-signed.
* 0.5.7:
  * Add PCL Profile 7.
* 0.5.6:
  * Add PCL Profile 78.
  * Fixed minor PCL moniker fragments.
* 0.5.5:
  * Fixed version number.
  * Fixed icon image url.
* 0.5.4:
  * Auto open FSharp.Control.
  * Manage AppVeyor CI.
* 0.5.3: Implement awaiter classes.
* 0.5.2: Add dependency assemblies.
* 0.5.1: NuGet package support.

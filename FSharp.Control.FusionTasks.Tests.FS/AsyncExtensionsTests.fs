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

#if FS45NETCore20
// Validate for .NET Core 2.0 (F# 4.5)
namespace FSharp.Control.FusionTasks.Tests.FS45NETCore20
#endif

open System
open System.IO
open System.Diagnostics
open System.Threading.Tasks

open NUnit.Framework
open FsUnit

module AsyncExtensions =

  [<Test>]
  let AsyncBuilderAsAsyncTest() =
    let r = Random()
    let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
    do r.NextBytes data
    use ms = new MemoryStream()
    let computation = async {
        do! ms.WriteAsync(data, 0, data.Length)
      }
    do computation |> Async.RunSynchronously  // FSUnit not supported Async/Task based tests, so run synchronously here. 
    ms.ToArray() |> should equal data

  [<Test>]
  let AsyncBuilderAsAsyncCTATest() =
    let r = Random()
    let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
    do r.NextBytes data
    use ms = new MemoryStream()
    let computation = async {
        do! ms.WriteAsync(data, 0, data.Length).AsyncConfigure(false)
      }
    do computation |> Async.RunSynchronously  // FSUnit not supported Async/Task based tests, so run synchronously here. 
    ms.ToArray() |> should equal data
    
  [<Test>]
  let AsyncBuilderAsAsyncCTATTest() =
    let r = Random()
    let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
    do r.NextBytes data
    let computation = async {
        use ms = new MemoryStream()
        do ms.Write(data, 0, data.Length)
        do ms.Position <- 0L
        let! length = ms.ReadAsync(data, 0, data.Length).AsyncConfigure(false)
        do length |> should equal data.Length
        return ms.ToArray()
      }
    let results = computation |> Async.RunSynchronously  // FSUnit not supported Async/Task based tests, so run synchronously here. 
    results |> should equal data

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
        do ms.Write(data, 0, data.Length)
        do ms.ToArray() |> should equal data
      }
    computation |> Async.RunSynchronously  // FSUnit not supported Async/Task based tests, so run synchronously here. 

#if FS41NET45 || FS4NET45 || FS4PCL259 || FS41NETStandard16 || FS45NETCore20
  [<Test>]
  let AsyncBuilderAsAsyncVTTest() =
    let r = Random()
    let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
    do r.NextBytes data
    let computation = async {
        use ms = new MemoryStream()
        do ms.Write(data, 0, data.Length)
        do ms.Position <- 0L
        let! length = new ValueTask<int>(ms.ReadAsync(data, 0, data.Length))
        do length |> should equal data.Length
        return ms.ToArray()
      }
    let results = computation |> Async.RunSynchronously  // FSUnit not supported Async/Task based tests, so run synchronously here. 
    results |> should equal data

  [<Test>]
  let AsyncBuilderAsAsyncCVTATTest() =
    let r = Random()
    let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
    do r.NextBytes data
    let computation = async {
        use ms = new MemoryStream()
        do ms.Write(data, 0, data.Length)
        do ms.Position <- 0L
        let! length = (new ValueTask<int>(ms.ReadAsync(data, 0, data.Length))).AsyncConfigure(false)
        do length |> should equal data.Length
        return ms.ToArray()
      }
    let results = computation |> Async.RunSynchronously  // FSUnit not supported Async/Task based tests, so run synchronously here. 
    results |> should equal data
#endif

  [<Test>]
  let AsyncBuilderCompilesForInTest() =
    let computation = async {
        let mutable result = 0
        for i in {1..10} do
          result <- result + i
        do result |> should equal 55
      }

    computation |> Async.RunSynchronously  // FSUnit not supported Async/Task based tests, so run synchronously here. 

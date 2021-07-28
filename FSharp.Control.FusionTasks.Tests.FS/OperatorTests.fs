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

module OperatorTests =

  ////////////////////////////////////////////////////////////////////////
  // Explicitly operators

  [<Test>]
  let OperatorAsAsyncFromTaskTest() =
    let test = async {
      let r = Random()
      let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
      do r.NextBytes data
      use ms = new MemoryStream()
      let computation = async {
          do! ms.WriteAsync(data, 0, data.Length) |> Async.AsAsync
          Assert.AreEqual(data, ms.ToArray())
      }
      do! computation
    }
    test.AsTask()

  [<Test>]
  let OperatorAsAsyncFromTaskTTest() =
    let test = async {
      let r = Random()
      let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
      do r.NextBytes data
      let computation = async {
        use ms = new MemoryStream()
        do ms.Write(data, 0, data.Length)
        do ms.Position <- 0L
        let! length = ms.ReadAsync(data, 0, data.Length) |> Async.AsAsync
        Assert.AreEqual(data.Length, length)
      
        return ms.ToArray()
      }
      let! results = computation
      Assert.AreEqual(data, results)
    }
    test.AsTask()

  [<Test>]
  let OperatorAsAsyncFromValueTaskTest() =
    let test = async {
      let r = Random()
      let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
      do r.NextBytes data
      use ms = new MemoryStream()
      let computation = async {
          do! new ValueTask(ms.WriteAsync(data, 0, data.Length)) |> Async.AsAsync
          Assert.AreEqual(data, ms.ToArray())
      }
      do! computation
    }
    test.AsTask()

  [<Test>]
  let OperatorAsAsyncFromValueTaskTTest() =
    let test = async {
      let r = Random()
      let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
      do r.NextBytes data
      let computation = async {
        use ms = new MemoryStream()
        do ms.Write(data, 0, data.Length)
        do ms.Position <- 0L
        let! length = new ValueTask<int>(ms.ReadAsync(data, 0, data.Length)) |> Async.AsAsync
        Assert.AreEqual(data.Length, length)
    
        return ms.ToArray()
      }
      let! results = computation
      Assert.AreEqual(data, results)
    }
    test.AsTask()
    
  [<Test>]
  let OperatorAsAsyncFromTaskConfiguredTest() =
    let test = async {
      let r = Random()
      let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
      do r.NextBytes data
      use ms = new MemoryStream()
      let computation = async {
          do! ms.WriteAsync(data, 0, data.Length).ConfigureAwait(false) |> Async.AsAsync
          Assert.AreEqual(data, ms.ToArray())
      }
      do! computation
    }
    test.AsTask()

      
  [<Test>]
  let OperatorAsAsyncFromTaskTConfiguredTest() =
    let test = async {
      let r = Random()
      let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
      do r.NextBytes data
      let computation = async {
        use ms = new MemoryStream()
        do ms.Write(data, 0, data.Length)
        do ms.Position <- 0L
        let! length = ms.ReadAsync(data, 0, data.Length).ConfigureAwait(false) |> Async.AsAsync
        Assert.AreEqual(data.Length, length)
        
        return ms.ToArray()
      }
      let! results = computation
      Assert.AreEqual(data, results)
    }
    test.AsTask()

  [<Test>]
  let OperatorAsAsyncFromValueTaskConfiguredTest() =
    let test = async {
      let r = Random()
      let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
      do r.NextBytes data
      use ms = new MemoryStream()
      let computation = async {
          do! (new ValueTask(ms.WriteAsync(data, 0, data.Length))).ConfigureAwait(false) |> Async.AsAsync
          Assert.AreEqual(data, ms.ToArray())
      }
      do! computation
    }
    test.AsTask()

    
  [<Test>]
  let OperatorAsAsyncFromValueTaskTConfiguredTest() =
    let test = async {
      let r = Random()
      let data = Seq.init 100000 (fun i -> 0uy) |> Seq.toArray
      do r.NextBytes data
      let computation = async {
        use ms = new MemoryStream()
        do ms.Write(data, 0, data.Length)
        do ms.Position <- 0L
        let! length = (new ValueTask<int>(ms.ReadAsync(data, 0, data.Length))).ConfigureAwait(false) |> Async.AsAsync
        Assert.AreEqual(data.Length, length)
      
        return ms.ToArray()
      }
      let! results = computation
      Assert.AreEqual(data, results)
    }
    test.AsTask()

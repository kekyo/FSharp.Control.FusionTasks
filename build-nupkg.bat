@echo off

rem FSharp.Control.FusionTasks - F# Async workflow <--> .NET Task easy seamless interoperability library.
rem Copyright (c) 2016 Kouji Matsui (@kekyo2)
rem 
rem Licensed under the Apache License, Version 2.0 (the "License");
rem you may not use this file except in compliance with the License.
rem You may obtain a copy of the License at
rem 
rem http://www.apache.org/licenses/LICENSE-2.0
rem 
rem Unless required by applicable law or agreed to in writing, software
rem distributed under the License is distributed on an "AS IS" BASIS,
rem WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
rem See the License for the specific language governing permissions and
rem limitations under the License.

set nupkg_version=1.1.0

.nuget\nuget pack src\FSharp.Control.FusionTasks.FS20.nuspec -Prop Version=%nupkg_version% -Prop Configuration=Release
.nuget\nuget pack src\FSharp.Control.FusionTasks.FS30.nuspec -Prop Version=%nupkg_version% -Prop Configuration=Release
.nuget\nuget pack src\FSharp.Control.FusionTasks.FS31.nuspec -Prop Version=%nupkg_version% -Prop Configuration=Release
.nuget\nuget pack src\FSharp.Control.FusionTasks.FS40.nuspec -Prop Version=%nupkg_version% -Prop Configuration=Release
.nuget\nuget pack src\FSharp.Control.FusionTasks.FS41.nuspec -Prop Version=%nupkg_version% -Prop Configuration=Release

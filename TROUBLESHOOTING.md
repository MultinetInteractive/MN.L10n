# Troubleshooting
### I cannot compile and I must scream
Set your build verbosity to detailed, and see if it is the `StackExchange.Precompilation.Build`-package that fails to load DLLs when it tries to build.

It seems that the `tools`-folder in that package not always get it's dependencies.
So I solved it by running this, in the same package-folder that `StackExchange.Precompilation.Build` is.

```bat
REM Installning SE Precompilation Build
nuget install StackExchange.Precompilation.Build -Version 4.0.0

REM Installing dependencies, in case the nuget-install didn't take
nuget install Microsoft.CodeAnalysis.Common -Version 2.2.0
copy Microsoft.CodeAnalysis.Common.2.2.0\lib\netstandard1.3\Microsoft.CodeAnalysis.dll StackExchange.Precompilation.Build.4.0.0\tools\

nuget install Microsoft.CodeAnalysis.CSharp -Version 2.2.0
copy Microsoft.CodeAnalysis.CSharp.2.2.0\lib\netstandard1.3\Microsoft.CodeAnalysis.CSharp.dll StackExchange.Precompilation.Build.4.0.0\tools\

nuget install StackExchange.Precompilation.Metaprogramming -Version 4.0.0
copy StackExchange.Precompilation.Metaprogramming.4.0.0\lib\net462\* StackExchange.Precompilation.Build.4.0.0\tools\

nuget install System.Collections.Immutable -Version 1.3.1
copy System.Collections.Immutable.1.3.1\lib\netstandard1.0\System.Collections.Immutable.dll StackExchange.Precompilation.Build.4.0.0\tools\

nuget install System.Composition.AttributedModel -Version 1.0.31
copy System.Composition.AttributedModel.1.0.31\lib\netstandard1.0\System.Composition.AttributedModel.dll StackExchange.Precompilation.Build.4.0.0\tools\

nuget install System.Composition.Hosting -Version 1.0.31
copy System.Composition.Hosting.1.0.31\lib\netstandard1.0\System.Composition.Hosting.dll StackExchange.Precompilation.Build.4.0.0\tools\

nuget install System.Composition.Runtime -Version 1.0.31
copy System.Composition.Runtime.1.0.31\lib\netstandard1.0\System.Composition.Runtime.dll StackExchange.Precompilation.Build.4.0.0\tools\

nuget install System.Composition.TypedParts -Version 1.0.31
copy System.Composition.TypedParts.1.0.31\lib\netstandard1.0\System.Composition.TypedParts.dll StackExchange.Precompilation.Build.4.0.0\tools\

nuget install System.IO.FileSystem -Version 4.3.0
copy System.IO.FileSystem.4.3.0\lib\net46\System.IO.FileSystem.dll StackExchange.Precompilation.Build.4.0.0\tools\

nuget install System.Reflection -Version 4.3.0
copy System.Reflection.4.3.0\lib\net462\System.Reflection.dll StackExchange.Precompilation.Build.4.0.0\tools\

nuget install System.Reflection.Metadata -Version 1.4.2
copy System.Reflection.Metadata.1.4.2\lib\netstandard1.1\System.Reflection.Metadata.dll StackExchange.Precompilation.Build.4.0.0\tools\

nuget install System.Runtime -Version 4.3.0
copy System.Runtime.4.3.0\lib\net462\System.Runtime.dll StackExchange.Precompilation.Build.4.0.0\tools\

nuget install System.Runtime.Extensions -Version 4.3.0
copy System.Runtime.Extensions.4.3.0\lib\net462\System.Runtime.Extensions.dll StackExchange.Precompilation.Build.4.0.0\tools\

nuget install System.ValueTuple -Version 4.3.0
copy System.ValueTuple.4.3.0\lib\netstandard1.0\System.ValueTuple.dll StackExchange.Precompilation.Build.4.0.0\tools\
```

And replace the contents in `StackExchange.Precompiler.exe.config` with the `StackExchange.Precompilation.Build.cfg`
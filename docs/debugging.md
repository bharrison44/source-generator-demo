# Debugging

## Source Generator

To allow a relatively seamless debugging experience, make the following additions.

To the generator project, add an additional build configuration specifically for generator debugging.

```xml
<PropertyGroup>
    <Configurations>Debug;Release;DebugGen</Configurations>
</PropertyGroup>
```

To the source generator add this code as the first thing in the `Initialize` method.

```C#
#if DEBUGGEN
if (!Debugger.IsAttached) { Debugger.Launch(); }
#endif
```

Also add the necessary `using` statement.

```c#
#if DEBUGGEN
using System.Diagnostics;
#endif
```

Ensure Visual Studios is **not** using the added generator debugging build configuration. It should keep using the standard `Debug` or `Release` configurations.

With this done, to debug the source generator, open a terminal and run this command, directed to a project which uses the source generator as a project reference.

```bash
dotnet build -c DebugGen --no-incremental <entrypoint>
```

A popup will appear, in which the instance of Visual Studios with the source generator open in should be select. The debugger will break on either the `Debugger.Launch()` line or in a location  without source, which can simply be advanced past. The source generator is now being debugged. Any breakpoints set should work as usual.

## Generated Code Usage

When a project uses a source generator, Visual Studies will only ever use the generator as it was at start up. If changes are made,  compilation may success, but Intellisense will report errors. This can be rectified by simply ensuring the generator project has been built using the `Debug` build configuration since the last changes made, and restarting Visual Studios.

While the generator debugging process described above should reduce the number of times this is necessary, it will most likely still be required occasionally. For this reason, it's recommended to create a small solution containing only the generator and an entrypoint project (such as a console application) to make the build and restart process as quick as possible.

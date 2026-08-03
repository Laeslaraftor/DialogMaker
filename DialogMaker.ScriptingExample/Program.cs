using DialogMaker.Core.Scripting;
using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Runtime;
using DialogMaker.Core.Scripting.Runtime.Executor;
using DialogMaker.ScriptingExample;

DSharpAssemblyBuilder assembly = Projects.CompileStandardLibrary(true);
IDSharpType entryType;

try
{
    entryType = assembly.GetType("Program");
}
catch (Exception error)
{
    Console.WriteLine($"Unable to find entry type: {error.Message}");
    return;
}

if (entryType == null)
{
    Console.WriteLine("Entry type \"Program\" not found");
    return;
}

IDSharpMethodInfo? entryPoint = entryType.GetMethodOrDefault("Main");

if (entryPoint == null)
{
    Console.WriteLine($"Entry method \"Main\" not found at \"{entryType}\"");
    return;
}

DSharpVm vm = new(assembly);
DSharpThread thread = vm.CreateThread();

Console.WriteLine($"Starting with \"{entryPoint}\": ");
Console.WriteLine();

try
{
    unsafe
    {
        thread.Start(null, entryPoint);
    }

    while (thread.IsExecuting)
    {
        await Task.Delay(50);
    }

    if (thread.LastException != null)
    {
        throw thread.LastException;
    }
}
catch (DSharpException exception)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(exception);
    Console.ResetColor();
}

Console.WriteLine();
Console.WriteLine();
Console.WriteLine("Execution completed...");

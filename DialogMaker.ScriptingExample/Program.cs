using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Runtime;
using DialogMaker.Core.Scripting.Runtime.Executor;
using DialogMaker.ScriptingExample;

StandardExternalMethodsProvider externalMethodsProvider = new();
var assembly = Projects.CompileStandardLibrary();
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
    Console.WriteLine("Entry type not found");
    return;
}

var entryPoint = entryType.GetMethodOrDefault("Main");

if (entryPoint == null)
{
    Console.WriteLine($"Entry method \"Main\" not found at \"{entryType}\"");
    return;
}

if (entryPoint is DSharpMethodBuilder builder)
{
    var bytecode = builder.GetBytecodeBuilder();
    Console.WriteLine("Bytecode: ");
    Console.WriteLine(bytecode.ToString());
}

DSharpVm vm = new(assembly);
vm.ExternalMethodsProviders.Add(externalMethodsProvider);
var thread = vm.CreateThread();

Console.WriteLine("Start executing: ");

unsafe
{
    thread.Start(null, entryPoint);
}

Console.WriteLine();
Console.WriteLine();
Console.WriteLine("Execution completed...");

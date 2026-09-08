using DialogMaker.Core.Scripting.Compiler;
using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Runtime;
using System.Diagnostics;
using static DialogMaker.Core.Scripting.Compiler.Builders.DSharpBytecodeBuilder;

namespace DialogMaker.Core.Tests
{
    internal class ScriptBytecodeTests
    {
        [Test]
        [TestCase(ScriptCompilerTests.SimpleScript, null, "repeat")]
        [TestCase(ScriptCompilerTests.SimpleScript, null, "sum")]
        [TestCase(ScriptCompilerTests.SimpleScript, null, "getTextColor")]
        [TestCase(ScriptCompilerTests.SimpleScript, null, "getGenericValue")]
        [TestCase(ScriptCompilerTests.SimpleScript, null, "getLines")]
        [TestCase(ScriptCompilerTests.SimpleScript, null, "foreachTest")]
        [TestCase(ScriptCompilerTests.SimpleScript, "System.Threading.Thread", "Increment")]
        [TestCase(ScriptCompilerTests.SimpleScript, "System.Double", "GetSquared")]
        [TestCase(ScriptCompilerTests.SimpleScript, "System.List`1", "Add")]
        [TestCase(ScriptCompilerTests.SimpleScript, "System.List`1", "Remove")]
        [TestCase(ScriptCompilerTests.SimpleScript, "System.List`1", "Expand")]
        [TestCase(ScriptCompilerTests.TypeScript, "System.String", "GetFirstSymbol")]
        [TestCase(ScriptCompilerTests.TypeScript, "Character", "PrintName")]
        [TestCase(ScriptCompilerTests.TypeScript, "Player", "ToString")]
        [TestCase(ScriptCompilerTests.TypeScript, "Player", "SetValues")]
        [TestCase(ScriptCompilerTests.TypeScript, "Player", "GetValues")]
        [TestCase(ScriptCompilerTests.MathScript, null, "globalFunction")]
        [TestCase(ScriptCompilerTests.OperatorsScript, null, "castTypes")]
        [TestCase(ScriptCompilerTests.OperatorsScript, null, "sumTypes")]
        [TestCase(ScriptCompilerTests.OperatorsScript, null, "unaryTest")]
        [TestCase(ScriptCompilerTests.OperatorsScript, null, "customBinaryOperatorWithAssignment")]
        [TestCase(ScriptCompilerTests.GenericMethodsScript, null, "GetHudoeName")]
        [TestCase(ScriptCompilerTests.GenericMethodsScript, "MegaClass", "CreateInstance")]
        [TestCase(ScriptCompilerTests.TryCatchFinallyScript, null, "CatchException")]
        public static void PrintSimpleFunctionBytecode(string scriptName, string? typeName, string functionName)
        {
            var assembly = ScriptCompilerTests.CompileScript(scriptName);
            ReadFunctionOrMethod(assembly, typeName, functionName);
        }
        [TestCase("System.Array`1.Enumerator", "MoveNext")]
        [TestCase("System.Array<System.String>", "init")]
        [TestCase("System.Array<System.String>.Enumerator", "init")]
        [TestCase("System.Span`1", "get_Length")]
        [TestCase("System.Native.Pointer<Internal.System.Runtime.RuntimeTypeInfo>", "get_Item")]
        [TestCase("System.Native.Pointer<Internal.System.Runtime.RuntimeTypeInfo>", "get_IsNull")]
        [TestCase("System.Object", "Equals")]
        [TestCase("System.Byte", "init")]
        [TestCase("System.String", "Equals")]
        [TestCase("System.String", "Split")]
        [TestCase("System.String", "IsNullOrEmpty")]
        [TestCase("System.Type", "get_Name")]
        [TestCase("System.Type", "get_FullName")]
        [TestCase("System.Type", "get_Namespace")]
        [TestCase("System.Type", "get_DeclaringType")]
        [TestCase("System.Collections.Generic.List`1", "Add")]
        [TestCase("System.Collections.Generic.List`1", "set_Count")]
        [TestCase("System.Collections.Generic.List`1", "get_Capacity")]
        [TestCase("System.Collections.Generic.List<System.Exception>", "ctor")]
        [TestCase("System.Linq.EnumeratorExtensions", "Union")]
        [TestCase("Program", "TestPlayersArray")]
        [TestCase("Program", "TestExceptionHandling")]
        [TestCase("Program", "Main")]
        [TestCase("Program", "MainImpl")]
        [TestCase("Program", "TestArray")]
        [TestCase("Program", "GetSize")]
        [TestCase("Program", "GetGenericObject")]
        [TestCase("System.Reflection.MetadataTokenType", "ctor")]
        [TestCase("System.Reflection.MetadataTokenType", "init")]
        [TestCase("ValuePlayer", "PrintMessage")]
        [TestCase("ValuePlayer", "ctor")]
        public static void PrintMethodBytecode(string? typeName, string methodName)
        {
            var assembly = ScriptCompilerTests.CompileStandardLibrary();
            ReadFunctionOrMethod(assembly, typeName, methodName);
        }
        [TestCase("System.Array`1.Enumerator.MoveNext")]
        public static void TestStackValues(string methodName)
        {
            var assembly = ScriptCompilerTests.CompileStandardLibrary();
            bool isAnyMethodFound = false;

            foreach (var method in GetMethods(assembly, methodName))
            {
                isAnyMethodFound = true;
                TestStackValues(method);
            }

            if (!isAnyMethodFound)
            {
                Debug.Fail($"Invalid method name: \"{methodName}\"");
            }
        }

        #region Поиск методов и чтение кода

        private static void ReadFunctionOrMethod(DSharpAssemblyBuilder assembly, string? typeName, string methodName)
        {
            if (typeName == null)
            {
                ReadFunction(assembly, methodName);
            }
            else
            {
                var type = assembly.GetType(typeName);

                if (type is DSharpTypeBuilder builder)
                {
                    ReadMethod(builder, methodName);
                }
                else
                {
                    Debug.Fail($"Type \"{type}\" is not builder");
                }
            }
        }
        private static void ReadFunction(DSharpAssemblyBuilder assembly, string functionName)
        {
            var function = assembly.GlobalFunctions.FirstOrDefault(f => f.Name == functionName);

            if (function == null)
            {
                Debug.Fail($"Unable to find function \"{functionName}\"");
                return;
            }

            ReadCode(function);
        }
        private static IEnumerable<DSharpMethodBuilder> GetMethods(DSharpAssemblyBuilder assembly, string fullName)
        {
            string[] parts = fullName.Split('.');
            string methodName = string.Empty;
            DSharpTypeBuilder? typeBuilder = null;

            if (parts.Length == 1)
            {
                var function = assembly.GlobalFunctions.FirstOrDefault(f => f.Name == parts[0]);

                if (function != null)
                {
                    yield return function; 
                }

                yield break;
            }
            else if (parts.Length > 1)
            {
                var typeName = fullName.Replace("." + parts[^1], string.Empty);
                var type = assembly.GetType(typeName);

                if (type is DSharpTypeBuilder builder)
                {
                    methodName = parts[^1];
                    typeBuilder = builder;
                }
                else
                {
                    Debug.Fail($"Type \"{type}\" is not builder");
                    yield break;
                }
            }
            else
            {
                Debug.Fail($"Invalid function or method name \"{fullName}\"");
                yield break;
            }

            foreach (var method in typeBuilder.Methods.Union(typeBuilder.Constructors).Where(f => f.Name == methodName))
            {
                yield return method;
            }
        }
        private static void ReadMethod(DSharpTypeBuilder type, string methodName)
        {
            bool isAnyMethodFound = false;

            foreach (var method in type.Methods.Union(type.Constructors).Where(f => f.Name == methodName))
            {
                isAnyMethodFound = true;

                Console.WriteLine();
                Console.WriteLine(method);
                ReadCode(method);
            }

            if (!isAnyMethodFound)
            {
                Debug.Fail($"Unable to find method \"{methodName}\" at \"{type}\"");
            }
        }
        private static void ReadCode(DSharpMethodBuilder method)
        {
            var code = method.GetBytecodeBuilder();
            Console.WriteLine("Raw bytecode:");
            Console.WriteLine(code.ToString());

            DSharpBytecodeOptimizer.Optimize(method.Assembly);

            Console.WriteLine();
            Console.WriteLine("Optimized:");
            Console.WriteLine(code.ToString());
        }

        #endregion

        #region Проверка стека

        private static readonly DSharpBytecodeOperation[] _loadInstructions = 
        [
            DSharpBytecodeOperation.LoadField,
            DSharpBytecodeOperation.LoadInstanceField,
            DSharpBytecodeOperation.LoadProperty,
            DSharpBytecodeOperation.LoadInstanceProperty,
            DSharpBytecodeOperation.LoadBaseInstanceProperty,
            DSharpBytecodeOperation.LoadIndexer,
            DSharpBytecodeOperation.LoadBaseIndexer
        ];
        private static readonly DSharpBytecodeOperation[] _callInstructions =
        [
            DSharpBytecodeOperation.Call,
            DSharpBytecodeOperation.CallInstance,
            DSharpBytecodeOperation.CallBaseInstance,
            DSharpBytecodeOperation.AwaitCall,
            DSharpBytecodeOperation.AwaitCallInstance,
            DSharpBytecodeOperation.AwaitCallBaseInstance,
            DSharpBytecodeOperation.GenericCall,
            DSharpBytecodeOperation.GenericCallInstance,
            DSharpBytecodeOperation.GenericCallBaseInstance,
            DSharpBytecodeOperation.AwaitGenericCall,
            DSharpBytecodeOperation.AwaitGenericCallInstance,
            DSharpBytecodeOperation.AwaitGenericCallBaseInstance
        ];
        private static readonly DSharpBytecodeOperation[] _returnInstructions =
        [
            DSharpBytecodeOperation.Add,
            DSharpBytecodeOperation.Subtract,
            DSharpBytecodeOperation.Multiply,
            DSharpBytecodeOperation.Divide,
            DSharpBytecodeOperation.Mod,
            DSharpBytecodeOperation.And,
            DSharpBytecodeOperation.Or,
            DSharpBytecodeOperation.Less,
            DSharpBytecodeOperation.LessOrEqual,
            DSharpBytecodeOperation.Greater,
            DSharpBytecodeOperation.GreaterOrEqual,
            DSharpBytecodeOperation.Equals,
            DSharpBytecodeOperation.NotEquals
        ];

        private static void TestStackValues(DSharpMethodBuilder method)
        {
            Console.WriteLine();
            Console.WriteLine($"{method}: ");

            var code = method.GetBytecodeBuilder();
            List<IDSharpType> elements = [];
            
            void Add(IDSharpType type)
            {
                elements.Add(type);
                Console.WriteLine($"Добавлен объект \"{type}\". Общее количество: {elements.Count}");
            }
            bool Remove(uint offset, int count)
            {
                if (offset + count > elements.Count)
                {
                    Console.WriteLine($"Невозможно удалить объекты: {count} со смещением {offset}");
                    return false;
                }

                int index = elements.Count - 1 - (int)offset;
                var element = elements[index];
                elements.RemoveAt(index);

                Console.WriteLine($"Тип \"{element}\" удалён со смещением {offset}");

                if (count > 1)
                {
                    return Remove(offset, count - 1);
                }

                return true;
            }

            foreach (var instruction in code.Instructions)
            {
                Console.WriteLine(instruction);

                if (instruction.Operation == DSharpBytecodeOperation.Push &&
                    instruction is LiteralInstruction literal)
                {
                    Add(method.Assembly.GetType(literal.Value.Type));
                }
                else if (instruction.Operation == DSharpBytecodeOperation.Pop)
                {
                    Remove(0, 1);
                }
                else if (instruction.Operation == DSharpBytecodeOperation.PopOffset &&
                         instruction is IndexInstruction popOffsetInstruction)
                {
                    Remove(popOffsetInstruction.Index, 1);
                }
                else if (instruction.Operation == DSharpBytecodeOperation.PopRepeat &&
                         instruction is IndexInstruction popRepeatInstruction)
                {
                    Remove(popRepeatInstruction.Index, 1);
                }
                else if (instruction.Operation == DSharpBytecodeOperation.PopOffsetRepeat &&
                         instruction is OffsetCountInstruction popOffsetRepeatInstruction)
                {
                    Remove(popOffsetRepeatInstruction.Offset, popOffsetRepeatInstruction.Count);
                }
                else if (instruction.Operation == DSharpBytecodeOperation.PopPreviousTwo)
                {
                    Remove(1, 2);
                }
                else if (instruction is SizeInstruction)
                {
                    Add(method.Assembly.Int32Type);
                }
                else if (instruction.Operation == DSharpBytecodeOperation.LoadLocal &&
                         instruction is ParameterInstruction loadLocalInstruction)
                {
                    Add(loadLocalInstruction.Parameter.Type);
                }
                else if (_loadInstructions.Contains(instruction.Operation) &&
                         instruction is TypeInstruction loadInstruction &&
                         loadInstruction.MemberInfo.TryGetReturnType(out var loadReturnType))
                {
                    Add(loadReturnType);
                }
                else if (_callInstructions.Contains(instruction.Operation) &&
                         instruction is TypeInstruction callInstruction &&
                         callInstruction.MemberInfo.TryGetReturnType(out var callReturnType))
                {
                    Add(callReturnType);
                }
                else if (instruction.Operation == DSharpBytecodeOperation.LoadInstance)
                {
                    Add(method.DeclaringType!);
                }
                else if (_returnInstructions.Contains(instruction.Operation))
                {
                    Add(elements[^1]);
                }
            }
        }

        #endregion
    }
}

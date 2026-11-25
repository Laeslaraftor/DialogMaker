using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using static DialogMaker.Core.Scripting.CodeAnalyzer.CodeGeneratorHelper;

namespace DialogMaker.Core.Scripting.CodeAnalyzer
{
    public static class BinaryMathInstructionExecutorsGenerator
    {
        private const string EnumName = "DSharpBinaryOperator";

        public static void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var enumDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
                                                             predicate: IsBinaryOperatorsEnum,
                                                             transform: static (ctx, ct) => (EnumDeclarationSyntax)ctx.Node)
                                                         .Where(static eds => eds != null);

            var collected = enumDeclarations.Combine(context.CompilationProvider)
                                            .Select(static (pair, token) => BuildModel(pair.Left, pair.Right, token))
                                            .Where(static m => m != null).Select((m, _) => m!.Value);

            static void Generate(Func<ItemModel, string?> generator, string fileName, ItemModel item, SourceProductionContext source)
            {
                var executorSource = generator(item);

                if (executorSource == null)
                {
                    return;
                }

                source.AddSource(fileName, SourceText.From(executorSource, Encoding.UTF8));
            }

            context.RegisterSourceOutput(collected, static (productionContext, model) =>
            {
                foreach (var item in model!.Items)
                {
                    Generate(CreateDefaultExecutor, $"DSharp{item.Name}InstructionExecutor.g.cs", item, productionContext);
                    Generate(CreateVariablesExecutor, $"DSharp{item.Name}VariablesInstructionExecutor.g.cs", item, productionContext);
                    Generate(CreateFieldsExecutor, $"DSharp{item.Name}FieldsInstructionExecutor.g.cs", item, productionContext);
                }
            });
        }

        private static bool IsBinaryOperatorsEnum(SyntaxNode node, CancellationToken token)
        {
            return node is EnumDeclarationSyntax enumDeclaration &&
                   enumDeclaration.Identifier.ValueText == EnumName;
        }
        private static EnumModel? BuildModel(EnumDeclarationSyntax syntax, Compilation compilation, CancellationToken token)
        {
            var semantic = compilation.GetSemanticModel(syntax.SyntaxTree);

            if (semantic.GetDeclaredSymbol(syntax, token) is not INamedTypeSymbol enumSymbol)
            {
                return null;
            }

            var items = ImmutableArray.CreateBuilder<ItemModel>();

            foreach (var member in enumSymbol.GetMembers().OfType<IFieldSymbol>())
            {
                if (!member.HasConstantValue ||
                    member.TryGetAttribute(IgnoreAttributeFullName, out _))
                {
                    continue;
                }

                var name = GetNameAttribute(member) ?? member.Name;

                items.Add(new(name, member.Name));
            }

            return new(items.ToImmutable());
        }
        private static string? GetNameAttribute(ISymbol symbol)
        {
            if (!symbol.TryGetAttribute(NameAttributeFullName, out var attribute) ||
                attribute.ConstructorArguments.Length == 0)
            {
                return null;
            }

            return attribute.ConstructorArguments[0].Value as string;
        }

        private static void GenerateControls(StringBuilder builder, ItemModel item)
        {
            var indent2 = GetIndent(2);
            var indent3 = GetIndent(3);

            builder.AppendLine($"{indent2}#region Controls");
            builder.AppendLine();
            builder.AppendLine($"{indent2}public override void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)");
            builder.AppendLine($"{indent2}{{");
            builder.AppendLine($"{indent3}InstanceExecute(instruction, ref context);");
            builder.AppendLine($"{indent2}}}");
            builder.AppendLine();
            builder.AppendLine($"{indent2}public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer()");
            builder.AppendLine($"{indent2}{{");
            builder.AppendLine($"{indent3}return &InstanceExecute;");
            builder.AppendLine($"{indent2}}}");
            builder.AppendLine();
            builder.AppendLine($"{indent2}#endregion");
            builder.AppendLine();
        }

        private static string? CreateDefaultExecutor(ItemModel item)
        {
            // this executors have special implementation
            if (item.Name == "Equals" || item.Name == "NotEquals")
            {
                return null;
            }

            StringBuilder builder = new();
            var indent1 = GetIndent(1);
            var indent2 = GetIndent(2);
            var indent3 = GetIndent(3);
            var indent4 = GetIndent(4);
            var executorName = $"DSharp{item.Name}InstructionExecutor";

            if (!GeneratedTypes.ContainsKey(executorName))
            {
                GeneratedTypes.Add(executorName, $"global::DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions.{executorName}");
            }

            builder.AppendLine("// <auto-generated/>");
            builder.AppendLine();
            builder.AppendLine($"using System;");
            builder.AppendLine($"using System.Runtime.CompilerServices;");
            builder.AppendLine($"using DialogMaker.Core.Scripting.Runtime.Numbers.MathExecutors;");
            builder.AppendLine();
            builder.AppendLine("#nullable enable");
            builder.AppendLine();
            builder.AppendLine("namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions");
            builder.AppendLine("{");
            builder.AppendLine($"{indent1}/// <summary>");
            builder.AppendLine($"{indent1}/// Executor of <see cref=\"DSharpBytecodeOperation.{item.Name}\"/> operation");
            builder.AppendLine($"{indent1}/// </summary>");
            builder.AppendLine($"{indent1}public sealed unsafe class {executorName} : DSharpMathInstructionExecutor");
            builder.AppendLine($"{indent1}{{");

            GenerateControls(builder, item);

            builder.AppendLine($"{indent2}#region Static");
            builder.AppendLine();
            builder.AppendLine($"{indent2}/// <summary>");
            builder.AppendLine($"{indent2}/// Global instance of <see cref=\"DSharpBytecodeOperation.{item.Name}\"/> operation executor");
            builder.AppendLine($"{indent2}/// </summary>");
            builder.AppendLine($"{indent2}public static readonly {executorName} Instance = new();");
            builder.AppendLine();
            builder.AppendLine($"{indent2}[MethodImpl(MethodImplOptions.AggressiveInlining)]");
            builder.AppendLine($"{indent2}private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)");
            builder.AppendLine($"{indent2}{{");
            builder.AppendLine($"#if DEBUG");
            builder.AppendLine($"{indent3}if (CheckStackValues(instruction, ref context, 2))");
            builder.AppendLine($"{indent3}{{");
            builder.AppendLine($"{indent4}return;");
            builder.AppendLine($"{indent3}}}");
            builder.AppendLine($"#endif");
            builder.AppendLine();
            builder.AppendLine($"{indent3}var stack = context.Stack;");
            builder.AppendLine($"{indent3}var left = stack.FastNumberPeek(1);");
            builder.AppendLine($"{indent3}var right =  stack.FastNumberPeek();");

            if (item.Name == "Divide")
            {
                builder.AppendLine($"{indent3}DSharpNumber number;");
                builder.AppendLine();
                builder.AppendLine($"{indent3}try");
                builder.AppendLine($"{indent3}{{");
                builder.AppendLine($"{indent4}number = DSharpNumberMathExecutor.{item.Name}(ref left, ref right);");
                builder.AppendLine($"{indent3}}}");
                builder.AppendLine($"{indent3}catch (DivideByZeroException)");
                builder.AppendLine($"{indent3}{{");
                builder.AppendLine($"{indent4}*context.Callback = context.ThrowDivideByZeroException();");
                builder.AppendLine($"{indent4}return;");
                builder.AppendLine($"{indent3}}}");
            }
            else
            {
                builder.AppendLine();
                builder.AppendLine($"{indent3}var number = DSharpNumberMathExecutor.{item.Name}(ref left, ref right);");
            }
            builder.AppendLine();
            builder.AppendLine($"{indent3}context.Stack.Push(&number);");
            builder.AppendLine($"{indent2}}}");
            builder.AppendLine();
            builder.AppendLine($"{indent2}#endregion");
            builder.AppendLine($"{indent1}}}");
            builder.AppendLine("}");

            return builder.ToString();
        }
        private static string CreateVariablesExecutor(ItemModel item)
        {
            StringBuilder builder = new();
            var indent1 = GetIndent(1);
            var indent2 = GetIndent(2);
            var indent3 = GetIndent(3);
            var indent4 = GetIndent(4);
            var executorName = $"DSharp{item.Name}VariablesInstructionExecutor";

            if (!GeneratedTypes.ContainsKey(executorName))
            {
                GeneratedTypes.Add(executorName, $"global::DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions.{executorName}");
            }

            builder.AppendLine("// <auto-generated/>");
            builder.AppendLine();
            builder.AppendLine($"using System.Runtime.CompilerServices;");
            builder.AppendLine($"using DialogMaker.Core.Scripting.Runtime.Numbers.MathExecutors;");
            builder.AppendLine();
            builder.AppendLine("#nullable enable");
            builder.AppendLine();
            builder.AppendLine("namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions");
            builder.AppendLine("{");
            builder.AppendLine($"{indent1}/// <summary>");
            builder.AppendLine($"{indent1}/// Executor of <see cref=\"DSharpBytecodeOperation.{item.Name}Variables\"/> operation");
            builder.AppendLine($"{indent1}/// </summary>");
            builder.AppendLine($"{indent1}public sealed unsafe class {executorName} : DSharpTwoVariablesMathInstructionExecutor");
            builder.AppendLine($"{indent1}{{");

            GenerateControls(builder, item);

            builder.AppendLine($"{indent2}#region Static");
            builder.AppendLine();
            builder.AppendLine($"{indent2}/// <summary>");
            builder.AppendLine($"{indent2}/// Global instance of <see cref=\"DSharpBytecodeOperation.{item.Name}Variables\"/> operation executor");
            builder.AppendLine($"{indent2}/// </summary>");
            builder.AppendLine($"{indent2}public static readonly {executorName} Instance = new();");
            builder.AppendLine();
            builder.AppendLine($"{indent2}[MethodImpl(MethodImplOptions.AggressiveInlining)]");
            builder.AppendLine($"{indent2}private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)");
            builder.AppendLine($"{indent2}{{");
            builder.AppendLine($"{indent3}var leftVariable = context.LocalVariables->GetItemReference((int)instruction->Arguments[0]);");
            builder.AppendLine($"{indent3}var rightVariable = context.LocalVariables->GetItemReference((int)instruction->Arguments[1]);");
            builder.AppendLine($"{indent3}var left = leftVariable->Buffer.ReadAsNumber();");
            builder.AppendLine($"{indent3}var right = rightVariable->Buffer.ReadAsNumber();");

            if (item.Name == "Divide")
            {
                builder.AppendLine($"{indent3}DSharpNumber number;");
                builder.AppendLine();
                builder.AppendLine($"{indent3}try");
                builder.AppendLine($"{indent3}{{");
                builder.AppendLine($"{indent4}number = DSharpNumberMathExecutor.{item.Name}(ref left, ref right);");
                builder.AppendLine($"{indent3}}}");
                builder.AppendLine($"{indent3}catch (DivideByZeroException)");
                builder.AppendLine($"{indent3}{{");
                builder.AppendLine($"{indent4}*context.Callback = context.ThrowDivideByZeroException();");
                builder.AppendLine($"{indent4}return;");
                builder.AppendLine($"{indent3}}}");
            }
            else
            {
                builder.AppendLine();
                builder.AppendLine($"{indent3}var number = DSharpNumberMathExecutor.{item.Name}(ref left, ref right);");
            }
            builder.AppendLine();
            builder.AppendLine($"{indent3}context.Stack.Push(&number);");
            builder.AppendLine($"{indent2}}}");
            builder.AppendLine();
            builder.AppendLine($"{indent2}#endregion");
            builder.AppendLine($"{indent1}}}");
            builder.AppendLine("}");

            return builder.ToString();
        }
        private static string CreateFieldsExecutor(ItemModel item)
        {
            StringBuilder builder = new();
            var indent1 = GetIndent(1);
            var indent2 = GetIndent(2);
            var indent3 = GetIndent(3);
            var indent4 = GetIndent(4);
            var indent5 = GetIndent(5);
            var executorName = $"DSharp{item.Name}FieldsInstructionExecutor";

            if (!GeneratedTypes.ContainsKey(executorName))
            {
                GeneratedTypes.Add(executorName, $"global::DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions.{executorName}");
            }

            builder.AppendLine("// <auto-generated/>");
            builder.AppendLine();
            builder.AppendLine($"using System.Runtime.CompilerServices;");
            builder.AppendLine($"using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;");
            builder.AppendLine($"using DialogMaker.Core.Scripting.Runtime.Numbers.MathExecutors;");
            builder.AppendLine();
            builder.AppendLine("#nullable enable");
            builder.AppendLine();
            builder.AppendLine("namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions");
            builder.AppendLine("{");
            builder.AppendLine($"{indent1}/// <summary>");
            builder.AppendLine($"{indent1}/// Executor of <see cref=\"DSharpBytecodeOperation.{item.Name}Fields\"/> operation");
            builder.AppendLine($"{indent1}/// </summary>");
            builder.AppendLine($"{indent1}public sealed unsafe class {executorName} : DSharpTwoFieldsMathInstructionExecutor");
            builder.AppendLine($"{indent1}{{");

            GenerateControls(builder, item);

            builder.AppendLine($"{indent2}#region Static");
            builder.AppendLine();
            builder.AppendLine($"{indent2}/// <summary>");
            builder.AppendLine($"{indent2}/// Global instance of <see cref=\"DSharpBytecodeOperation.{item.Name}Fields\"/> operation executor");
            builder.AppendLine($"{indent2}/// </summary>");
            builder.AppendLine($"{indent2}public static readonly {executorName} Instance = new();");
            builder.AppendLine();
            builder.AppendLine($"{indent2}[MethodImpl(MethodImplOptions.AggressiveInlining)]");
            builder.AppendLine($"{indent2}private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)");
            builder.AppendLine($"{indent2}{{");
            builder.AppendLine($"{indent3}var leftField = (DSharpRuntimeFieldInfo*)context.ReplaceMember((DSharpMetadataToken*)instruction->Arguments[0]);");
            builder.AppendLine($"{indent3}var rightField = (DSharpRuntimeFieldInfo*)context.ReplaceMember((DSharpMetadataToken*)instruction->Arguments[1]);");
            builder.AppendLine($"{indent3}DSharpObject* leftInstance = null;");
            builder.AppendLine($"{indent3}DSharpObject* rightInstance = null;");
            builder.AppendLine($"{indent3}uint offset = 0;");
            builder.AppendLine();
            builder.AppendLine($"{indent3}if (!rightField->IsStatic)");
            builder.AppendLine($"{indent3}{{");
            builder.AppendLine($"{indent4}rightInstance = GetInstance(ref context, 0, throwOnError: true);");
            builder.AppendLine();
            builder.AppendLine($"{indent4}if (rightInstance == null)");
            builder.AppendLine($"{indent4}{{");
            builder.AppendLine($"{indent5}return;");
            builder.AppendLine($"{indent4}}}");
            builder.AppendLine();
            builder.AppendLine($"{indent4}offset++;");
            builder.AppendLine($"{indent3}}}");
            builder.AppendLine($"{indent3}if (!leftField->IsStatic)");
            builder.AppendLine($"{indent3}{{");
            builder.AppendLine($"{indent4}leftInstance = GetInstance(ref context, offset, throwOnError: true);");
            builder.AppendLine();
            builder.AppendLine($"{indent4}if (leftInstance == null)");
            builder.AppendLine($"{indent4}{{");
            builder.AppendLine($"{indent5}return;");
            builder.AppendLine($"{indent4}}}");
            builder.AppendLine($"{indent3}}}");
            builder.AppendLine();
            builder.AppendLine($"{indent3}var left = leftField->ReadAsNumber(leftInstance);");
            builder.AppendLine($"{indent3}var right = rightField->ReadAsNumber(rightInstance);");

            if (item.Name == "Divide")
            {
                builder.AppendLine($"{indent3}DSharpNumber number;");
                builder.AppendLine();
                builder.AppendLine($"{indent3}try");
                builder.AppendLine($"{indent3}{{");
                builder.AppendLine($"{indent4}number = DSharpNumberMathExecutor.{item.Name}(ref left, ref right);");
                builder.AppendLine($"{indent3}}}");
                builder.AppendLine($"{indent3}catch (DivideByZeroException)");
                builder.AppendLine($"{indent3}{{");
                builder.AppendLine($"{indent4}*context.Callback = context.ThrowDivideByZeroException();");
                builder.AppendLine($"{indent4}return;");
                builder.AppendLine($"{indent3}}}");
            }
            else
            {
                builder.AppendLine($"{indent3}var number = DSharpNumberMathExecutor.{item.Name}(ref left, ref right);");
            }

            builder.AppendLine();
            builder.AppendLine($"{indent3}context.Stack.Push(&number);");
            builder.AppendLine($"{indent2}}}");
            builder.AppendLine();
            builder.AppendLine($"{indent2}#endregion");
            builder.AppendLine($"{indent1}}}");
            builder.AppendLine("}");

            return builder.ToString();
        }

        #region Structs

        private record struct EnumModel(ImmutableArray<ItemModel> Items);
        private record struct ItemModel(string Name, string EnumMemberName);

        #endregion
    }
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using static DialogMaker.Core.Scripting.CodeAnalyzer.CodeGeneratorHelper;

namespace DialogMaker.Core.Scripting.CodeAnalyzer
{
    public abstract class ExecutorsBasedOnEnumGenerator
    {
        protected abstract string EnumName { get; }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var enumDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
                                                             predicate: IsBinaryOperatorsEnum,
                                                             transform: static (ctx, ct) => (EnumDeclarationSyntax)ctx.Node)
                                                         .Where(static eds => eds != null);

            var collected = enumDeclarations.Combine(context.CompilationProvider)
                                            .Select(static (pair, token) => BuildModel(pair.Left, pair.Right, token))
                                            .Where(static m => m != null).Select((m, _) => m!.Value);


            context.RegisterSourceOutput(collected, (productionContext, model) =>
            {
                foreach (var item in model!.Items)
                {
                    GenerateExecutor(item, productionContext);
                }
            });
        }

        protected abstract void GenerateExecutor(ItemModel item, SourceProductionContext context);

        private bool IsBinaryOperatorsEnum(SyntaxNode node, CancellationToken token)
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

        protected static void Generate(Func<ItemModel, string?> generator, string fileName, ItemModel item, SourceProductionContext source)
        {
            var executorSource = generator(item);

            if (executorSource == null)
            {
                return;
            }

            source.AddSource(fileName, SourceText.From(executorSource, Encoding.UTF8));
        }
        protected static void GenerateControls(StringBuilder builder, ItemModel item, bool addZeroArguments = false)
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

            if (addZeroArguments)
            {
                builder.AppendLine($"{indent2}public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)");
                builder.AppendLine($"{indent2}{{");
                builder.AppendLine($"{indent3}return 0;");
                builder.AppendLine($"{indent2}}}");
                builder.AppendLine($"{indent2}public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)");
                builder.AppendLine($"{indent2}{{");
                builder.AppendLine($"{indent2}}}");
            }

            builder.AppendLine();
            builder.AppendLine($"{indent2}#endregion");
            builder.AppendLine();
        }

        #region Structs

        protected record struct EnumModel(ImmutableArray<ItemModel> Items);
        protected record struct ItemModel(string Name, string EnumMemberName);

        #endregion
    }
}

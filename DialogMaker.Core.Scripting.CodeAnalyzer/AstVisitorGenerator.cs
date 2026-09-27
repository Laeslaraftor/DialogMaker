using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace DialogMaker.Core.Scripting.CodeAnalyzer
{
    [Generator(LanguageNames.CSharp)]
    public sealed class AstVisitorGenerator : IIncrementalGenerator
    {
        private const string AstNodeFullName = "DialogMaker.Core.Scripting.Compiler.Ast.Nodes.AstNode";
        private const string IEnumerableFullName = "System.Collections.Generic.IEnumerable`1";
        private const string VisitorInterfaceName = "IDSharpAstVisitor";
        private const string VisitorBaseClassName = "DSharpAstVisitor";
        private const string Namespace = "DialogMaker.Core.Scripting.Compiler.Ast";

        private INamedTypeSymbol? _iEnumerable;

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var nodeTypes = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => IsCandidateClass(s),
                    transform: static (ctx, ct) => GetNodeInfo(ctx, ct))
                .Where(static info => info is not null)
                .Select(static (info, _) => info!.Value)
                .Collect();

            context.RegisterSourceOutput(nodeTypes, (context, nodes) =>
            {
                if (nodes.IsDefaultOrEmpty)
                {
                    return;
                }

                var distinct = nodes
                    .GroupBy(n => n.FullName)
                    .Select(g => g.First())
                    .OrderBy(n => n.FullName)
                    .ToImmutableArray();

                context.AddSource($"{VisitorInterfaceName}.g.cs", SourceText.From(GenerateVisitorInterface(distinct), Encoding.UTF8));
                context.AddSource($"{VisitorBaseClassName}.g.cs", SourceText.From(GenerateVisitorBaseClass(distinct), Encoding.UTF8));

                foreach (var node in distinct)
                {
                    context.AddSource($"{node.TypeName}.Accept.g.cs", SourceText.From(GenerateAcceptImplementation(node), Encoding.UTF8));
                }
            });
        }

        private static bool IsCandidateClass(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax classDeclaration
                   && !classDeclaration.Modifiers.Any(m => m.ValueText == "abstract")
                   && !classDeclaration.Modifiers.Any(m => m.ValueText == "static");
        }
        private static AstNodeInfo? GetNodeInfo(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            var classSyntax = (ClassDeclarationSyntax)context.Node;
            var symbol = context.SemanticModel.GetDeclaredSymbol(classSyntax, cancellationToken);

            if (symbol is not INamedTypeSymbol namedTypeSymbol ||
                namedTypeSymbol.IsAbstract || namedTypeSymbol.IsStatic ||
                !InheritsFromAstNode(namedTypeSymbol))
            {
                return null;
            }

            bool isPartial = classSyntax.Modifiers.Any(m => m.ValueText == "partial");

            var ns = symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
            var typeName = symbol.Name;
            var visitMethodName = "Visit" + typeName;

            return new AstNodeInfo(
                TypeSymbol: namedTypeSymbol,
                DeclaringType: GetDeclaringType(namedTypeSymbol),
                Compilation: context.SemanticModel.Compilation,
                Namespace: ns,
                TypeName: typeName,
                FullName: namedTypeSymbol.FullName,
                VisitMethodName: visitMethodName,
                IsPartial: isPartial);
        }

        private static bool InheritsFromAstNode(ITypeSymbol symbol)
        {
            var baseType = symbol.BaseType;

            while (baseType is not null)
            {
                if (baseType.ToDisplayString() == AstNodeFullName)
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            return false;
        }
        private bool InheritsFromIEnumerableAstNode(AstNodeInfo info, ITypeSymbol symbol, bool simple = false)
        {
            _iEnumerable ??= info.Compilation.GetTypeByMetadataName(IEnumerableFullName);

            if (_iEnumerable == null)
            {
                return false;
            }

            INamedTypeSymbol? constructedEnumerable = null;

            if (symbol is INamedTypeSymbol named &&
                SymbolEqualityComparer.Default.Equals(symbol.OriginalDefinition, _iEnumerable))
            {
                constructedEnumerable = named;
            }
            else
            {
                constructedEnumerable = symbol.AllInterfaces.FirstOrDefault(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, _iEnumerable));
            }
            if (constructedEnumerable == null || constructedEnumerable.TypeArguments.Length == 0)
            {
                return false;
            }

            var elementType = constructedEnumerable.TypeArguments[0];

            return InheritsFromAstNode(elementType);
        }
        private static ITypeSymbol? GetDeclaringType(ITypeSymbol symbol)
        {
            if (symbol.ContainingSymbol is ITypeSymbol typeSymbol)
            {
                return typeSymbol;
            }

            return null;
        }
        private static IEnumerable<ISymbol> GetAllMembers(ITypeSymbol type)
        {
            while (type != null)
            {
                foreach (var member in type.GetMembers())
                {
                    yield return member;
                }

                type = type.BaseType!;
            }
        }

        private static string GenerateVisitorInterface(ImmutableArray<AstNodeInfo> nodes)
        {
            string indent = CodeGeneratorHelper.GetIndent(1);
            string indent2 = CodeGeneratorHelper.GetIndent(2);
            StringBuilder builder = new();
            builder.AppendLine("// <auto-generated/>");
            builder.AppendLine("#nullable enable");
            builder.AppendLine();
            builder.AppendLine($"namespace {Namespace}");
            builder.AppendLine("{");
            builder.AppendLine($"{indent}/// <summary>");
            builder.AppendLine($"{indent}/// Visitor for D# abstract syntax tree nodes.");
            builder.AppendLine($"{indent}/// </summary>");
            builder.AppendLine($"{indent}public interface {VisitorInterfaceName}");
            builder.AppendLine($"{indent}{{");

            builder.AppendLine($"{indent2}/// <summary>");
            builder.AppendLine($"{indent2}/// Start visiting node.");
            builder.AppendLine($"{indent2}/// </summary>");
            builder.AppendLine($"{indent2}public void StartVisiting(global::{AstNodeFullName} node);");
            builder.AppendLine($"{indent2}/// <summary>");
            builder.AppendLine($"{indent2}/// End visiting node.");
            builder.AppendLine($"{indent2}/// </summary>");
            builder.AppendLine($"{indent2}public void EndVisiting(global::{AstNodeFullName} node);");
            builder.AppendLine();

            foreach (var node in nodes)
            {
                builder.AppendLine($"{indent2}/// <summary>");
                builder.AppendLine($"{indent2}/// Visits <see cref=\"{node.TypeSymbol.FullName}\"/>.");
                builder.AppendLine($"{indent2}/// </summary>");
                builder.AppendLine($"{indent2}public void {node.VisitMethodName}({node.TypeSymbol.FullName} node);");
            }

            builder.AppendLine($"{indent}}}");
            builder.AppendLine("}");
            return builder.ToString();
        }
        private static string GenerateVisitorBaseClass(ImmutableArray<AstNodeInfo> nodes)
        {
            string indent = CodeGeneratorHelper.GetIndent(1);
            string indent2 = CodeGeneratorHelper.GetIndent(2);
            StringBuilder builder = new();
            builder.AppendLine("// <auto-generated/>");
            builder.AppendLine("#nullable enable");
            builder.AppendLine();
            builder.AppendLine($"namespace {Namespace}");
            builder.AppendLine("{");
            builder.AppendLine($"{indent}/// <summary>");
            builder.AppendLine($"{indent}/// Default D# ast visitor implementation. All visit methods do nothing.");
            builder.AppendLine($"{indent}/// Override only methods you need.");
            builder.AppendLine($"{indent}/// </summary>");
            builder.AppendLine($"{indent}public abstract class {VisitorBaseClassName} : {VisitorInterfaceName}");
            builder.AppendLine($"{indent}{{");

            builder.AppendLine($"{indent2}public virtual void StartVisiting(global::{AstNodeFullName} node)");
            builder.AppendLine($"{indent2}{{");
            builder.AppendLine($"{indent2}}}");
            builder.AppendLine($"{indent2}public virtual void EndVisiting(global::{AstNodeFullName} node)");
            builder.AppendLine($"{indent2}{{");
            builder.AppendLine($"{indent2}}}");

            foreach (var node in nodes)
            {
                builder.AppendLine($"{indent2}public virtual void {node.VisitMethodName}({node.TypeSymbol.FullName} node)");
                builder.AppendLine($"{indent2}{{");
                builder.AppendLine($"{indent2}}}");
            }

            builder.AppendLine($"{indent}}}");
            builder.AppendLine("}");
            return builder.ToString();
        }
        private string GenerateAcceptImplementation(AstNodeInfo node)
        {
            int indentSize = 1;
            var indent1 = CodeGeneratorHelper.GetIndent(1);
            StringBuilder builder = new();

            builder.AppendLine("// <auto-generated/>");
            builder.AppendLine("#nullable enable");
            builder.AppendLine();
            builder.AppendLine($"namespace {node.Namespace}");
            builder.AppendLine("{");

            if (node.DeclaringType != null)
            {
                indentSize++;
                builder.AppendLine($"{indent1}public partial class {node.DeclaringType.Name}");
                builder.AppendLine($"{indent1}{{");
            }

            string indent = CodeGeneratorHelper.GetIndent(indentSize);
            string indent2 = CodeGeneratorHelper.GetIndent(indentSize + 1);
            string indent3 = CodeGeneratorHelper.GetIndent(indentSize + 2);
            string indent4 = CodeGeneratorHelper.GetIndent(indentSize + 3);
            string indent5 = CodeGeneratorHelper.GetIndent(indentSize + 4);
            string indent6 = CodeGeneratorHelper.GetIndent(indentSize + 5);

            builder.AppendLine($"{indent}public partial class {node.TypeName}");
            builder.AppendLine($"{indent}{{");
            builder.AppendLine($"{indent2}public override void Accept({VisitorInterfaceName} visitor, DSharpAstVisitMode visitMode = DSharpAstVisitMode.Simple)");
            builder.AppendLine($"{indent2}{{");
            builder.AppendLine($"{indent3}visitor.StartVisiting(this);");
            builder.AppendLine($"{indent3}visitor.{node.VisitMethodName}(this);");

            void WriteAccept(IPropertySymbol property)
            {
                if (InheritsFromIEnumerableAstNode(node, property.Type, true))
                {
                    builder.AppendLine($"{indent4}if (this.{property.Name} != null)");
                    builder.AppendLine($"{indent4}{{");
                    builder.AppendLine($"{indent5}foreach (var node in this.{property.Name})");
                    builder.AppendLine($"{indent5}{{");
                    builder.AppendLine($"{indent6}node?.Accept(visitor, nextVisitMode);");
                    builder.AppendLine($"{indent5}}}");
                    builder.AppendLine($"{indent4}}}");
                }
                else
                {
                    builder.AppendLine($"{indent4}this.{property.Name}?.Accept(visitor, nextVisitMode);");
                }
            }

            var properties = node.TypeSymbol.GetMembers().Union(GetAllMembers(node.TypeSymbol), SymbolEqualityComparer.Default)
                .Where(m => m is IPropertySymbol propertySymbol &&
                            !propertySymbol.IsStatic &&
                            propertySymbol.DeclaredAccessibility == Accessibility.Public &&
                            (InheritsFromAstNode(propertySymbol.Type) || InheritsFromIEnumerableAstNode(node, propertySymbol.Type)))
                .Cast<IPropertySymbol>();

            if (properties.Any())
            {
                builder.AppendLine($"{indent3}if (visitMode != DSharpAstVisitMode.Simple)");
                builder.AppendLine($"{indent3}{{");
                builder.AppendLine($"{indent4}var nextVisitMode = visitMode == DSharpAstVisitMode.Children ? DSharpAstVisitMode.Simple : DSharpAstVisitMode.Recursive;");

                foreach (var property in properties)
                {
                    WriteAccept(property);
                }

                builder.AppendLine($"{indent3}}}");
            }

            builder.AppendLine($"{indent3}visitor.EndVisiting(this);");
            builder.AppendLine($"{indent2}}}");
            builder.AppendLine($"{indent}}}");
            
            if (node.DeclaringType != null)
            {
                builder.AppendLine($"{indent1}}}");
            }

            builder.AppendLine("}");

            return builder.ToString();
        }

        private readonly record struct AstNodeInfo(
            INamedTypeSymbol TypeSymbol,
            ITypeSymbol? DeclaringType,
            Compilation Compilation,
            string Namespace,
            string TypeName,
            string FullName,
            string VisitMethodName,
            bool IsPartial);
    }
}
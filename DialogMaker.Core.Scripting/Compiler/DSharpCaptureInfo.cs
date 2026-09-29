using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Compiler
{
    /// <summary>
    /// Information about captured variables
    /// </summary>
    /// <param name="captures">List of captured variables</param>
    public class DSharpCaptureInfo(IList<DSharpCaptureInfo.Capture> captures)
    {
        /// <summary>
        /// List of captured variables
        /// </summary>
        public ReadOnlyCollection<Capture> Captures { get; } = new(captures);

        #region Controls

        /// <summary>
        /// Try to find variables capture for variable declaration
        /// </summary>
        /// <param name="variableDeclaration">Variable declaration for searching capture</param>
        /// <param name="result">Variables capture</param>
        /// <returns>Is capture was found successfully</returns>
        public bool TryFindCaptureForDeclaration(AstNode variableDeclaration, [NotNullWhen(true)] out Capture? result)
        {
            foreach (var capture in Captures)
            {
                foreach (var variable in capture.Variables.Values)
                {
                    if (variable.Declaration.Node == variableDeclaration)
                    {
                        result = capture;
                        return true;
                    }
                }
            }

            result = null;

            return false;
        }
        /// <summary>
        /// Try to find capture for specified scope.
        /// It checks captures that attached to specified scope, 
        /// then checks on containing any variable reference
        /// </summary>
        /// <param name="scope">Scope for searching capture for it</param>
        /// <param name="result">Variables capture</param>
        /// <returns>Is capture was found successfully</returns>
        public bool TryFindCaptureForScope(AstNode scope, [NotNullWhen(true)] out Capture? result)
        {
            foreach (var capture in Captures)
            {
                if (capture.Scope == scope)
                {
                    result = capture;
                    return true;
                }

                foreach (var variable in capture.Variables.Values)
                {
                    foreach (var reference in variable.References)
                    {
                        if (scope.Contains(reference))
                        {
                            result = capture;
                            return true;
                        }
                    }
                }
            }

            result = null;

            return false;
        }

        #endregion

        #region Static

        private static string? ThisKeyword
        {
            get
            {
                field ??= DSharpTokenTypeHelper.Values[DSharpTokenType.This].Keyword?.Name;
                return field;
            }
        }

        /// <summary>
        /// Try to find variables capturing in local functions or delegates
        /// </summary>
        /// <param name="body">Root method body</param>
        /// <param name="result">Variables capturing info</param>
        /// <returns>Is any variable captured and info was provided</returns>
        public static bool TryFind(AstNode body, [NotNullWhen(true)] out DSharpCaptureInfo? result)
        {
            result = null;
            DSharpVariablesVisitor variablesVisitor = new();
            AstNode rootNode = body;

            if (body.Parent is InvokableNode ||
                body.Parent is DelegateExpressionNode)
            {
                rootNode = body.Parent;
            }

            rootNode.Accept(variablesVisitor, DSharpAstVisitMode.Recursive);
            variablesVisitor.Reset(false);
            rootNode.Accept(variablesVisitor, DSharpAstVisitMode.Recursive);

            if (variablesVisitor.RootCaptures.Count == 0)
            {
                return false;
            }

            List<Capture> captures = new(variablesVisitor.RootCaptures.Count);

            foreach (var info in variablesVisitor.RootCaptures)
            {
                Capture capture = new(info.Key.Node);

                foreach (var variableInfo in info.Value.Values)
                {
                    if (variableInfo.VariableName == ThisKeyword)
                    {
                        capture.HasInstance = true;
                        continue;
                    }

                    if (!capture.Variables.TryGetValue(variableInfo.VariableName, out var variable))
                    {
                        variable = new(variableInfo.VariableName, variableInfo.Declaration);
                        capture.Variables.Add(variableInfo.VariableName, variable);
                    }

                    foreach (var reference in variableInfo.Scopes.SelectMany(i => i.Value))
                    {
                        variable.References.Add(reference);
                    }
                }

                captures.Add(capture);
            }

            int i = 0;
            captures.Sort();

            while (i < captures.Count)
            {
                var capture = captures[i];
                Capture? captureToUnion = null;

                foreach (var variable in capture.Variables.Values)
                {
                    if (captureToUnion != null)
                    {
                        break;
                    }

                    foreach (var reference in variable.References)
                    {
                        if (captureToUnion != null)
                        {
                            break;
                        }

                        for (int c = i + 1; c < captures.Count; c++)
                        {
                            var capture2 = captures[c];

                            if (capture2.Scope.Contains(reference) &&
                                capture.Scope.Contains(capture.Scope))
                            {
                                captureToUnion = capture2;
                                break;
                            }
                        }
                    }
                }

                if (captureToUnion != null)
                {
                    int indexToRemove = i;

                    if (capture.Scope.Contains(captureToUnion.Scope))
                    {
                        indexToRemove = captures.IndexOf(captureToUnion);
                        Union(capture, captureToUnion);
                    }
                    else
                    {
                        Union(captureToUnion, capture);
                    }

                    captures.RemoveAt(indexToRemove);

                    if (i >= indexToRemove)
                    {
                        continue;
                    }
                }

                i++;
            }

            static void Union(Capture main, Capture other)
            {
                main.HasInstance |= other.HasInstance;

                foreach (var variable in other.Variables.Values)
                {
                    if (main.Variables.TryGetValue(variable.Name, out var mainVariable))
                    {
                        foreach (var reference in variable.References)
                        {
                            mainVariable.References.Add(reference);
                        }
                    }
                    else
                    {
                        main.Variables.Add(variable.Name, variable);
                    }
                }
            }

            result = new(captures);

            return true;
        }

        #endregion

        #region Classes

        /// <summary>
        /// Information about captured variables
        /// </summary>
        /// <param name="scope">Scope that contains captured variables</param>
        public class Capture(AstNode scope) : IComparable
        {
            /// <summary>
            /// Scope that contains captured variables
            /// </summary>
            public AstNode Scope { get; } = scope;
            /// <summary>
            /// Captured variables
            /// </summary>
            public Dictionary<string, Variable> Variables { get; } = [];
            /// <summary>
            /// Instance was captured
            /// </summary>
            public bool HasInstance { get; set; }

            public int CompareTo(object? obj)
            {
                if (obj is int line)
                {
                    return Scope.Line.CompareTo(line);
                }
                else if (obj is AstNode node)
                {
                    return Scope.Line.CompareTo(node.Line);
                }
                else if (obj is Capture other)
                {
                    return Scope.Line.CompareTo(other.Scope.Line);
                }

                return -1;
            }
            public override string ToString()
            {
                var result = $"Capture at {Scope.Line}:{Scope.Column}{(HasInstance ? " with instance" : string.Empty)}";

                if (Variables.Count > 0)
                {
                    result += $" ({string.Join("; ", Variables.Values)})";
                }

                return result;
            }
        }
        /// <summary>
        /// Information about captured variable
        /// </summary>
        /// <param name="name">Variable name</param>
        public class Variable(string name, DSharpVariablesVisitor.CaptureDeclaration declaration)
        {
            /// <summary>
            /// Variable name
            /// </summary>
            public string Name { get; } = name;
            /// <summary>
            /// Variable declaration
            /// </summary>
            public DSharpVariablesVisitor.CaptureDeclaration Declaration { get; } = declaration;
            /// <summary>
            /// Node that references to this variable
            /// </summary>
            public HashSet<AstNode> References { get; } = [];

            public override string ToString()
            {
                var result = $"\"{Name}\"-{Declaration.Node.Line}:{Declaration.Node.Column}";

                if (References.Count > 0)
                {
                    result += $", {string.Join(", ", References.Select(r => $"{r.Line}:{r.Column}"))}";
                }

                return result;
            }
        }

        #endregion
    }
}

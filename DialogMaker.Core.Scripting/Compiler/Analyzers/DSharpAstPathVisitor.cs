using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using System.Collections.ObjectModel;

namespace DialogMaker.Core.Scripting.Compiler.Analyzers
{
    /// <summary>
    /// Path visitor that check all path on containing statements
    /// </summary>
    public class DSharpAstPathVisitor<T> : DSharpAstVisitor where T : StatementNode
    {
        public DSharpAstPathVisitor()
        {
            RequiredStatements = new(_requiredStatements);
        }

        /// <summary>
        /// Is all path contains required statements
        /// </summary>
        public bool IsSuccess { get; private set; }
        /// <summary>
        /// Statements with required type that was found.
        /// This contains statements with specified type and exception throwing
        /// </summary>
        public ReadOnlyCollection<StatementNode> RequiredStatements { get; }
        /// <summary>
        /// Extra analyzer for statements block
        /// </summary>
        public DSharpAstPathAnalyzer.StatementsBlockAnalyzer? ExtraAnalyzer { get; set; }

        private readonly List<StatementNode> _requiredStatements = [];

        #region Controls

        public override void StartVisiting(AstNode node)
        {
            base.StartVisiting(node);
            IsSuccess = false;
            _requiredStatements.Clear();
        }

        public override void VisitExpressionStatementNode(ExpressionStatementNode node)
        {
            base.VisitExpressionStatementNode(node);

            if (node.Expression is ThrowExpressionNode)
            {
                IsSuccess = true;
                _requiredStatements.Add(node);
            }
        }
        public override void VisitBlockStatementNode(BlockStatementNode node)
        {
            base.VisitBlockStatementNode(node);

            if (ExtraAnalyzer != null)
            {
                var selectedStatements = ExtraAnalyzer(node, out bool isSuccess);
                IsSuccess = isSuccess;
                _requiredStatements.AddRange(selectedStatements);
            }

            foreach (var statement in node.Statements)
            {
                if (statement is T typedStatement)
                {
                    IsSuccess = true;
                    _requiredStatements.Add(typedStatement);
                }
                else
                {
                    IsSuccess |= CheckAllPathContains(statement);
                }
            }
        }
        public override void VisitIfStatementNode(IfStatementNode node)
        {
            base.VisitIfStatementNode(node);

            if (node.Condition == null)
            {
                return;
            }

            bool thenBranchContains = CheckAllPathContains(node.ThenBranch);
            bool? booleanCondition = null;

            if (node.Condition.TrySimplifyToLiteral(out var literalCondition) &&
                literalCondition.IsBool)
            {
                booleanCondition = literalCondition.AsBool();
            }

            if (node.ElseBranch == null)
            {
                if (booleanCondition != null && booleanCondition.Value)
                {
                    IsSuccess = thenBranchContains;
                }

                return;
            }

            IsSuccess = thenBranchContains && CheckAllPathContains(node.ElseBranch);
        }
        public override void VisitUsingVariableStatementNode(UsingVariableStatementNode node)
        {
            base.VisitUsingVariableStatementNode(node);

            if (node.Body != null)
            {
                IsSuccess = CheckAllPathContains(node.Body);
            }
        }
        public override void VisitLockStatementNode(LockStatementNode node)
        {
            base.VisitLockStatementNode(node);

            if (node.Body != null)
            {
                IsSuccess = CheckAllPathContains(node.Body);
            }
        }
        public override void VisitUnsafeStatementNode(UnsafeStatementNode node)
        {
            base.VisitUnsafeStatementNode(node);

            if (node.Body != null)
            {
                IsSuccess = CheckAllPathContains(node.Body);
            }
        }
        public override void VisitTryStatementNode(TryStatementNode node)
        {
            base.VisitTryStatementNode(node);

            if (node.TryBlock == null)
            {
                return;
            }

            var tryBlockContains = CheckAllPathContains(node.TryBlock);
            var typelessCatchBlock = node.CatchBlocks.FirstOrDefault(b => b.ExceptionVariable == null);

            if (typelessCatchBlock == null ||
                typelessCatchBlock.Statements == null)
            {
                return;
            }

            var catchBlockContains = CheckAllPathContains(typelessCatchBlock.Statements);
            IsSuccess = tryBlockContains && catchBlockContains;
        }
        public override void VisitSwitchStatementNode(SwitchStatementNode node)
        {
            base.VisitSwitchStatementNode(node);

            bool allPathContains = true;
            bool hasDefault = false;

            foreach (var caseStatement in node.Cases)
            {
                if (caseStatement.Body == null)
                {
                    continue;
                }
                if (caseStatement.IsDefault)
                {
                    hasDefault = true;
                }
                if (!CheckAllPathContains(caseStatement.Body))
                {
                    allPathContains = false;
                }
            }

            IsSuccess = hasDefault && allPathContains;
        }

        private bool CheckAllPathContains(StatementNode? statement)
        {
            if (statement == null)
            {
                return false;
            }

            var allPathContains = DSharpAstPathAnalyzer.AllPathContains<T>(statement, ExtraAnalyzer, out var requiredStatements);
            _requiredStatements.AddRange(requiredStatements);

            return allPathContains;
        }

        #endregion
    }
}

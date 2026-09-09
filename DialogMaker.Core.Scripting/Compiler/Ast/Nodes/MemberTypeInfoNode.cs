using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Type info with access to member of other type
    /// </summary>
    /// <param name="token">Token that represents type</param>
    public class MemberTypeInfoNode(DSharpToken token) : TypeInfoNode(token)
    {
        /// <summary>
        /// Member access expression to current type
        /// </summary>
        public MemberAccessExpressionNode? Member { get; set; }

        #region Controls

        public override string GetSimpleFullName()
        {
            return Member?.GetName(withoutGenerics: true) ?? base.GetSimpleFullName();
        }
        protected override string GetTypeName(bool simplifyGenerics)
        {
            return Member?.GetName(simplifyGenerics) ?? base.GetTypeName(simplifyGenerics);
        }

        #endregion
    }
}

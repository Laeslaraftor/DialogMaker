using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Builders;

namespace DialogMaker.Core.Scripting.Compiler
{
    /// <summary>
    /// Class for compiling multiple scripts to single assembly
    /// </summary>
    /// <param name="assemblyBuilder">Assembly to compiling scripts</param>
    public class DSharpCompiler(DSharpAssemblyBuilder assemblyBuilder)
    {
        /// <summary>
        /// Assembly to compiling scripts
        /// </summary>
        public DSharpAssemblyBuilder Assembly { get; } = assemblyBuilder;

        private readonly Dictionary<DSharpScript, DSharpScriptCompiler> _scripts = [];

        #region Управление

        /// <summary>
        /// Clear compiled scripts and remove it
        /// </summary>
        public void Clear()
        {
            foreach (var script in _scripts.Values)
            {
                script.Reset();
            }

            _scripts.Clear();
        }
        /// <summary>
        /// Compile all scripts
        /// </summary>
        /// <param name="scripts">Scripts to compile</param>
        public void CompileTrees(params IEnumerable<DSharpScript> scripts)
        {
            List<DSharpScriptCompiler> scriptsToCompile = [];

            foreach (var treeRoot in scripts)
            {
                if (_scripts.TryGetValue(treeRoot, out var script))
                {
                    script.Reset();
                }
                else
                {
                    script = new(Assembly)
                    {
                        Script = treeRoot
                    };
                    _scripts.Add(treeRoot, script);
                }

                scriptsToCompile.Add(script);
            }
            foreach (var script in scriptsToCompile)
            {
                script.DeclareTypes();
            }
            foreach (var script in scriptsToCompile)
            {
                script.ResolveTypes();
            }
            foreach (var script in scriptsToCompile)
            {
                script.CompileCode();
            }
            foreach (var script in scriptsToCompile)
            {
                script.ApplyTypes();
            }
        }

        #endregion
    }
}

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

            List<DSharpScriptCompiler> scriptToResolveTypes = [.. scriptsToCompile];
            Dictionary<DSharpScriptCompiler, int> failedOnScriptsCount = [];

            void OnScriptTypeToSetupRequested(object sender, TypeSetupRequestEventArgs e)
            {
                if (e.SetupCompleted)
                {
                    return;
                }

                var type = e.Type;

                foreach (var script in scriptToResolveTypes)
                {
                    if (script == sender ||
                        !script.Types.Contains(type))
                    {
                        continue;
                    }

                    ResolveTypes(script);
                    e.SetupCompleted = true;
                    return;
                }
            }
            void ClearScriptRequestsSubscriptions()
            {
                foreach (var script in scriptsToCompile)
                {
                    script.TypeToSetupRequested -= OnScriptTypeToSetupRequested;
                }
            }
            void ResolveTypes(DSharpScriptCompiler script)
            {
                try
                {
                    script.ResolveTypes();
                    scriptToResolveTypes.Remove(script);
                    script.TypeToSetupRequested -= OnScriptTypeToSetupRequested;
                }
                catch (Exception error)
                {
                    if (failedOnScriptsCount.TryGetValue(script, out var scriptsCount)
                        && scriptsCount == scriptToResolveTypes.Count)
                    {
                        ClearScriptRequestsSubscriptions();
                        throw new InvalidOperationException($"Failed to compile script: {script}", error);
                    }

                    scriptToResolveTypes.Remove(script);
                    scriptToResolveTypes.Add(script);

                    if (!failedOnScriptsCount.TryAdd(script, scriptToResolveTypes.Count))
                    {
                        failedOnScriptsCount[script] = scriptToResolveTypes.Count;
                    }
                }
            }

            foreach (var script in scriptsToCompile)
            {
                try
                {
                    script.DeclareTypes();
                    script.TypeToSetupRequested += OnScriptTypeToSetupRequested;
                }
                catch (Exception error)
                {
                    ClearScriptRequestsSubscriptions();
                    throw new InvalidOperationException($"Failed to compile script: {script}", error);
                }
            }

            while (scriptToResolveTypes.Count > 0)
            {
                var script = scriptToResolveTypes[0];
                ResolveTypes(script);
            }

            foreach (var script in scriptsToCompile)
            {
                try
                {
                    script.ValidateTypes();
                }
                catch (Exception error)
                {
                    throw new InvalidOperationException($"Failed to compile script: {script}", error);
                }
            }
            foreach (var script in scriptsToCompile)
            {
                try
                {
                    script.CompileCode();
                }
                catch (Exception error)
                {
                    throw new InvalidOperationException($"Failed to compile script: {script}", error);
                }
            }
            foreach (var script in scriptsToCompile)
            {
                try
                {
                    script.ApplyTypes();
                }
                catch (Exception error)
                {
                    throw new InvalidOperationException($"Failed to compile script: {script}", error);
                }
            }
        }

        #endregion
    }
}

using DialogMaker.Core.Scripting.Compiler;
using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.ScriptingExample
{
    public static class Projects
    {
        public const string DSharpProjectPath = @"F:\Projects\DialogMaker\DialogMaker.Core.Scripting\StandardLibrary";

        public static DSharpAssemblyBuilder CompileStandardLibrary()
        {
            List<DSharpScript> scripts = [];
            DSharpAssemblyBuilder assembly = new("StandardLibrary", []);

            void ReadFiles(string folder)
            {
                foreach (var filePath in Directory.GetFiles(folder))
                {
                    if (!filePath.EndsWith(".cs"))
                    {
                        continue;
                    }

                    try
                    {
                        var script = File.ReadAllText(filePath);
                        DSharpLexer lexer = new(script);

                        lexer.Tokenize();

                        AstParser parser = new(lexer);
                        string fileName = filePath.Replace('\\', '/').Split('/')[^1][..^3];
                        var parsedScript = parser.Parse(fileName);
                        parsedScript.FilePath = filePath;

                        scripts.Add(parsedScript);
                    }
                    catch (Exception error)
                    {
                        throw new InvalidOperationException($"Unable to parse \"{filePath}\"", error);
                    }
                }
                foreach (var innerFolder in Directory.GetDirectories(folder))
                {
                    ReadFiles(innerFolder);
                }
            }

            ReadFiles(DSharpProjectPath);

            DSharpCompiler compiler = new(assembly);
            compiler.CompileTrees(scripts);

            return assembly;
        }
    }
}

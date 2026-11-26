using Microsoft.CodeAnalysis;

namespace DialogMaker.Core.Scripting.CodeAnalyzer
{
    [Generator(LanguageNames.CSharp)]
    public class EnumBasedCodeGenerator : IIncrementalGenerator
    {
        private static readonly BinaryMathInstructionExecutorsGenerator _binaryMath = new();
        private static readonly UnaryMathInstructionExecutorsGenerator _unaryMath = new();

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            _binaryMath.Initialize(context);
            _unaryMath.Initialize(context);
            ExecutorsInformationGenerator.Initialize(context);
            EnumHelperGenerator.Initialize(context);
        }
    }
}

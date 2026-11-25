using Microsoft.CodeAnalysis;
using System;

namespace DialogMaker.Core.Scripting.CodeAnalyzer
{
    [Generator(LanguageNames.CSharp)]
    public class EnumBasedCodeGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            BinaryMathInstructionExecutorsGenerator.Initialize(context);
            ExecutorsInformationGenerator.Initialize(context);
            EnumHelperGenerator.Initialize(context);
        }
    }
}

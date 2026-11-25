using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using DialogMaker.Core.Scripting.Runtime.Numbers.MathExecutors;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base class of all math operations between two fields
    /// </summary>
    public abstract unsafe class DSharpTwoFieldsMathInstructionExecutor : DSharpInstructionExecutor
    {
        public override void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            var leftField = (DSharpRuntimeFieldInfo*)context.ReplaceMember((DSharpMetadataToken*)instruction->Arguments[0]);
            var rightField = (DSharpRuntimeFieldInfo*)context.ReplaceMember((DSharpMetadataToken*)instruction->Arguments[1]);
            DSharpObject* leftInstance = null;            
            DSharpObject* rightInstance = null;
            uint offset = 0;

            if (!leftField->IsStatic)
            {
                leftInstance = GetInstance(ref context, 0, throwOnError: true);

                if (leftInstance == null)
                {
                    return;
                }

                offset++;
            }
            if (!rightField->IsStatic)
            {
                rightInstance = GetInstance(ref context, offset, throwOnError: true);

                if (rightInstance == null)
                {
                    return;
                }
            }

            var left = leftField->ReadAsNumber(leftInstance);
            var right = leftField->ReadAsNumber(rightInstance);

            var number = DSharpNumberMathExecutor.Add(ref left, ref right);

            context.Stack.Push(&number);
        }

        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            stream->Read<DSharpMetadataToken>();
            stream->Read<DSharpMetadataToken>();
            return 2;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            var leftFieldToken = stream->Read<DSharpMetadataToken>();
            var rightFieldToken = stream->Read<DSharpMetadataToken>();

            arguments[0] = (nint)typesProvider.GetField(leftFieldToken);
            arguments[1] = (nint)typesProvider.GetField(rightFieldToken);
        }
    }
}

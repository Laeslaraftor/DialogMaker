using DialogMaker.Core.Scripting.Runtime.Executor.Bytecode;
using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# method executor
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DSharpMethodExecutor
    {
        public DSharpMethodExecutor* PreviousExecutor;
        public DSharpObject* ObjectInstance;
        public DSharpRuntimeMethodInfo* MethodInfo;
        public DSharpRuntimeBytecode* Bytecode;
        public UnmanagedArray<nint> LocalVariables;
        public uint InstructionIndex;

        public DSharpMethodExecutionCallback Execute(DSharpObjectsContainer objectsContainer, DSharpThread thread)
        {
            int scopesCount = Math.Max(1, (int)Bytecode->ScopesCount);
            Span<DSharpExecutionScope> scopes = stackalloc DSharpExecutionScope[scopesCount];
            DSharpExecutionContext context = new(objectsContainer, thread, ObjectInstance, MethodInfo)
            {
                InstructionIndex = InstructionIndex
            };

            while (context.InstructionIndex < Bytecode->Instructions.Length)
            {
                var instruction = Bytecode->Instructions[(int)context.InstructionIndex];

                if (!instruction.Execute(ref context))
                {
                    break;
                }

                context.InstructionIndex++;
            }

            InstructionIndex = context.InstructionIndex;

            return default;
        }
    }
}

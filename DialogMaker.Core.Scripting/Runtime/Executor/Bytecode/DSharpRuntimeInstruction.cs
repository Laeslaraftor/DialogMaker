using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode
{
    /// <summary>
    /// Information about runtime instruction
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DSharpRuntimeInstruction
    {
        /// <summary>
        /// D# operation
        /// </summary>
        public DSharpBytecodeOperation Operation;
        /// <summary>
        /// Array of pointers to operation arguments
        /// </summary>
        public UnmanagedArray<nint> Arguments;
        /// <summary>
        /// Pointer to executor method that implements <see cref="DSharpInstructionExecutor.Execute(DSharpRuntimeInstruction*, ref DSharpExecutionContext)"/>
        /// </summary>
        public delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, DSharpMethodExecutionCallback> Executor;

        #region Static

        /// <summary>
        /// Execute instruction
        /// </summary>
        /// <param name="context">Execution context</param>
        /// <exception cref="InvalidOperationException">Executor not specified</exception>
        /// <returns>Instruction execution callback</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
#if DEBUG
            if (instruction->Executor == null)
            {
                throw new InvalidOperationException($"Executor not specified for \"{instruction->Operation}\"");
            }
#endif
            // Console.WriteLine($"{context.CurrentMethod->ToString()}: {instruction->Operation}:{context.InstructionIndex}");

            return instruction->Executor(instruction, ref context);
        }

        /// <summary>
        /// Get arguments count of current instruction
        /// </summary>
        /// <param name="typesProvider">Types provider for getting runtime type information</param>
        /// <param name="stream">Bytecode stream</param>
        /// <returns>Arguments count of current instruction</returns>
        public static int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            MemoryBuilder memoryBuilder = new(0, 0);
            return Create(typesProvider, ref memoryBuilder, stream).Arguments.Length;
        }
        /// <summary>
        /// Create base runtime information about instruction.
        /// </summary>
        /// <param name="typesProvider"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static DSharpRuntimeInstruction Create(DSharpRuntimeInformationProvider typesProvider, ref MemoryBuilder memoryBuilder, UnmanagedStream* stream)
        {
            var operation = stream->Read<DSharpBytecodeOperation>();
            int argumentsCount = 0;
            
            if (DSharpBytecodeOperationHelper.Values.TryGetValue(operation, out var info) &&
                info.ArgsCount != null)
            {
                argumentsCount = info.ArgsCount.Count;
            }
            if (!DSharpInstructionExecutor.TryGetExecutor(operation, out var executor))
            {
                throw new InvalidOperationException($"Unable to get executor for instruction: \"{operation}\"");
            }
            if (argumentsCount > 0)
            {
                int startArgumentsPosition = stream->Position;
                argumentsCount = executor.GetArgumentsCount(typesProvider, stream);
                
                if (memoryBuilder.Pointer != 0)
                {
                    stream->Position = startArgumentsPosition;
                }
            }

            DSharpRuntimeInstruction instruction = new()
            {
                Operation = operation,
                Executor = executor.GetExecutorPointer()
            };

            if (argumentsCount != 0 && memoryBuilder.Pointer != 0)
            {
                instruction.Arguments = memoryBuilder.AllocateArray<nint>(argumentsCount);
                executor.ReadArguments(typesProvider, stream, instruction.Arguments);
            }
            else
            {
                instruction.Arguments = new(0, argumentsCount);
            }

            return instruction;
        }

        #endregion
    }
}

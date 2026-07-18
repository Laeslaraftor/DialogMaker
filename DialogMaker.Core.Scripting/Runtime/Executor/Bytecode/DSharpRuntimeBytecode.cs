using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode
{
    /// <summary>
    /// Runtime bytecode information
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DSharpRuntimeBytecode
    {
        /// <summary>
        /// Bytecode variables
        /// </summary>
        public UnmanagedArray<DSharpRuntimeParameterInfo> Variables;
        /// <summary>
        /// Bytecode instructions
        /// </summary>
        public UnmanagedArray<DSharpRuntimeInstruction> Instructions;
        /// <summary>
        /// Amount of try/catch/finally blocks
        /// </summary>
        public uint TryingBlocksCount;
        /// <summary>
        /// Amount of scopes
        /// </summary>
        public uint ScopesCount;

        #region Static

        /// <summary>
        /// Parse runtime bytecode from raw data
        /// </summary>
        /// <param name="typesProvider">Types provider for getting runtime type information</param>
        /// <param name="bytecode">Raw bytecode data</param>
        /// <returns>Runtime bytecode</returns>
        public static DSharpRuntimeBytecode* Parse(DSharpRuntimeInformationProvider typesProvider, UnmanagedArray<byte> bytecode)
        {
            UnmanagedStream stream = bytecode.ToStream();
            int variablesCount = stream.Read<int>();
            Span<DSharpRuntimeParameterInfo> variables = stackalloc DSharpRuntimeParameterInfo[variablesCount];
            int parametersCount = 0;
            var streamPointer = &stream;

            for (int i = 0; i < variablesCount; i++)
            {
                variables[i] = DSharpRuntimeParameterInfo.Read(typesProvider, streamPointer);
            }

            int instructionsCount = stream.Read<int>();
            int instructionsStartPosition = stream.Position;

            while (stream.Position < stream.Length)
            {
                parametersCount += DSharpRuntimeInstruction.GetArgumentsCount(typesProvider, streamPointer);
            }

            int runtimeBytecodeSize = sizeof(DSharpRuntimeBytecode) +
                                      variablesCount * sizeof(DSharpRuntimeParameterInfo) +
                                      parametersCount * sizeof(nint) +
                                      instructionsCount * sizeof(DSharpRuntimeInstruction);
            var runtimeBytecode = (DSharpRuntimeBytecode*)Marshal.AllocHGlobal(runtimeBytecodeSize);
            MemoryBuilder builder = new((nint)runtimeBytecode, runtimeBytecodeSize);
            builder.Allocate(sizeof(DSharpRuntimeBytecode));

            runtimeBytecode->Variables = builder.AllocateArray<DSharpRuntimeParameterInfo>(variablesCount);
            runtimeBytecode->Instructions = builder.AllocateArray<DSharpRuntimeInstruction>(instructionsCount);
            runtimeBytecode->TryingBlocksCount = 0;
            runtimeBytecode->ScopesCount = 0;

            for (int i = 0; i < variablesCount; i++)
            {
                runtimeBytecode->Variables[i] = variables[i];
            }

            stream.Position = instructionsStartPosition;

            for (int i = 0; i < instructionsCount; i++)
            {
                var instruction = DSharpRuntimeInstruction.Create(typesProvider, ref builder, streamPointer);
                runtimeBytecode->Instructions[i] = instruction;

                if (instruction.Operation == DSharpBytecodeOperation.StartTrying)
                {
                    runtimeBytecode->TryingBlocksCount++;
                }
                else if (instruction.Operation == DSharpBytecodeOperation.StartScope)
                {
                    runtimeBytecode->ScopesCount++;
                }
            }

            return runtimeBytecode;
        }

        #endregion
    }
}

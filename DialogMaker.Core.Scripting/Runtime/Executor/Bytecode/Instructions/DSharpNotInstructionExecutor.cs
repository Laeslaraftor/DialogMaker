using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using Newtonsoft.Json.Linq;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Not"/> operation
    /// </summary>
    public class DSharpNotInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (CheckStackValues(instruction, context, 1, out var error))
            {
                return error;
            }

            var value = context.Stack.Peek();
            var obj = value.ReadAsObject();

            if (obj == null || obj->Type != context.TypesProvider.Boolean)
            {
                return context.ThrowExecutionException($"Unable to invert value because it is not boolean: {value.ValueType}");
            }

            var data = DSharpObject.GetData<bool>(obj);
            *data = !*data;

            return DSharpMethodExecutionCallback.Complete();
        }

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public unsafe override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            throw new NotImplementedException();
        }
        public unsafe override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Not"/> operation executor
        /// </summary>
        public static readonly DSharpNotInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

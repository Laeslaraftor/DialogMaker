using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.LoadProperty"/> operation
    /// </summary>
    public class DSharpLoadPropertyInstructionExecutor : DSharpMetadataTokenInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken metadataToken)
        {
            return Load(instruction, ref context, metadataToken, false, false);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.LoadProperty"/> operation executor
        /// </summary>
        public static readonly DSharpLoadPropertyInstructionExecutor Instance = new();

        internal static unsafe DSharpMethodExecutionCallback Load(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken metadataToken, bool isInstance, bool isBase)
        {
            DSharpObject* instance = null;
            DSharpRuntimePropertyInfo* property;

            try
            {
                property = context.TypesProvider.GetProperty(metadataToken);
            }
            catch (Exception exception)
            {
                return context.ThrowExecutionException(exception);
            }

            if (property->Getter == null)
            {
                return context.ThrowExecutionException($"Unable to get value from property \"{property->ToString()}\" because it have not getter");
            }

            int stackValues = property->Getter->ParametersType.Length;

            if (isInstance)
            {
                stackValues++;
            }
            if (CheckStackValues(instruction, context, stackValues, out var error))
            {
                return error;
            }
            if (isInstance)
            {
                stackValues--;
                instance = GetInstance(context, (uint)stackValues, out error);

                if (instance == null)
                {
                    return error;
                }
            }

            var args = DSharpCallInstructionExecutor.CreateArguments(context, property->Getter);

            return DSharpMethodExecutionCallback.Call(instance, property->Getter, args);
        }

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

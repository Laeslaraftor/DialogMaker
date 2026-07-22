using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.StoreProperty"/> operation
    /// </summary>
    public class DSharpStorePropertyInstructionExecutor : DSharpMetadataTokenInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken metadataToken)
        {
            return Store(instruction, ref context, metadataToken, false, false);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.StoreProperty"/> operation executor
        /// </summary>
        public static readonly DSharpStorePropertyInstructionExecutor Instance = new();

        internal static unsafe DSharpMethodExecutionCallback Store(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken metadataToken, bool isInstance, bool isBase)
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

            if (property->Setter == null)
            {
                return context.ThrowExecutionException($"Unable to set value to property \"{property->ToString()}\" because it have not setter");
            }

            int stackValues = property->Setter->ParametersType.Length;

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

            var args = DSharpCallInstructionExecutor.CreateArguments(context, property->Setter);

            return DSharpMethodExecutionCallback.Call(instance, property->Setter, args);
        }

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

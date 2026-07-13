using DialogMaker.Core.Scripting.Runtime.Executor.Bytecode;
using System.Reflection;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Attribute that sets D# operation executor
    /// </summary>
    /// <param name="type">Type of D# operation executor</param>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ExecutorAttribute(Type type) : Attribute
    {
        /// <summary>
        /// Type of D# operation executor
        /// </summary>
        public Type ExecutorType { get; } = type;

        #region Controls

        /// <summary>
        /// Get instance of operation executor
        /// </summary>
        /// <returns>Instance of operation executor</returns>
        public DSharpInstructionExecutor GetInstance()
        {
            var instanceField = ExecutorType.GetField(InstanceFieldName, BindingFlags.Public |
                                                                         BindingFlags.Static);

            if (instanceField != null)
            {
                if (instanceField.GetValue(null) is not DSharpInstructionExecutor executor)
                {
                    throw new InvalidOperationException($"\"{InstanceFieldName}\" field should contains type that implements {typeof(DSharpInstructionExecutor)}");
                }

                return executor;
            }
            if (Activator.CreateInstance(ExecutorType) is not DSharpInstructionExecutor newExecutorInstance)
            {
                throw new InvalidOperationException($"Specified type not implements {typeof(DSharpInstructionExecutor)}");
            }

            return newExecutorInstance;
        }

        #endregion

        #region Constants

        /// <summary>
        /// Name of static readonly field that contains global instance of executor implementation
        /// </summary>
        public const string InstanceFieldName = "Instance";

        #endregion
    }
}

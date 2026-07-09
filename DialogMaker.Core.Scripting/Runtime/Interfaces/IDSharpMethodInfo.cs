using DialogMaker.Core.Scripting.Compiler.Builders;

namespace DialogMaker.Core.Scripting.Runtime
{
    public interface IDSharpMethodInfo : IDSharpMemberInfo
    {
        /// <summary>
        /// Type of object that returns by method
        /// </summary>
        public IDSharpType? ReturnType { get; }
        /// <summary>
        /// Type of method
        /// </summary>
        public DSharpMethodType MethodType { get; }
        /// <summary>
        /// Method that was overriden by current method
        /// </summary>
        public IDSharpMethodInfo? OverrideMethod { get; }
        public bool IsVirtual { get; }
        public bool IsAbstract { get; }
        public bool IsSealed { get; }
        public bool IsExtern { get; }
        public IDSharpMethodBytecode? Bytecode { get; }

        public IDSharpParameterInfo[] GetParameters();
        public IDSharpType[] GetGenericParameters();
    }
}

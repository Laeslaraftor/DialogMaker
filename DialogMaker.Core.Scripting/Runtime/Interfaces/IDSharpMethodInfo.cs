using DialogMaker.Core.Scripting.Runtime.Builders;

namespace DialogMaker.Core.Scripting.Runtime
{
    public interface IDSharpMethodInfo : IDSharpMemberInfo
    {
        public IDSharpType? ReturnType { get; }
        public DSharpMethodType MethodType { get; }
        /// <summary>
        /// Method that was overriden by current method
        /// </summary>
        public IDSharpMethodInfo? OverrideMethod { get; }
        public bool IsVirtual { get; }
        public bool IsAbstract { get; }
        public bool IsSealed { get; }
        public bool IsExtern { get; }

        public IDSharpParameterInfo[] GetParameters();
        public IDSharpType[] GetGenericParameters();
        /// <summary>
        /// Copy bytecode of current method to specified bytecode builder
        /// </summary>
        /// <param name="builder">Bytecode builder for copying bytecode</param>
        public void CopyBytecodeTo(DSharpBytecodeBuilder builder);
    }
}

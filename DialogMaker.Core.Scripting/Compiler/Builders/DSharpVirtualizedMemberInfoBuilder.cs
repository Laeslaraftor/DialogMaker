using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public abstract class DSharpVirtualizedMemberInfoBuilder(DSharpAssemblyBuilder assembly, string name, DSharpTypeToken metadataToken)
        : DSharpMemberInfoBuilder(assembly, name, metadataToken)
    {
        public virtual bool IsAbstract { get; set; }
        public virtual bool IsSealed
        {
            get
            {
                if (DeclaringType?.IsValueType() == true)
                {
                    return true;
                }

                return field;
            }
            set;
        }
        public virtual bool IsVirtual { get; set; }
    }
}

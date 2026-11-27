using DialogMaker.Core.Scripting.Runtime;
using System.Data.Common;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class TwoParametersReferenceInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpParameterInfo firstParameter, IDSharpParameterInfo secondParameter) 
            : TwoParametersInstruction(builder, operation, firstParameter, secondParameter), IReferenceInstruction
        {
            public Instruction? ReferencedInstruction { get; set; }
            public override int SizeInBytes => base.SizeInBytes + sizeof(int);

            #region Controls

            public override void Write(Stream stream)
            {
                base.Write(stream);
                ReferenceInstruction.Write(stream, this);
            }

            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                IDSharpParameterInfo firstParameter = ParameterInstruction.ReplaceParameter(BytecodeBuilder, builder, FirstParameter);
                IDSharpParameterInfo secondParameter = ParameterInstruction.ReplaceParameter(BytecodeBuilder, builder, SecondParameter);

                return new TwoParametersReferenceInstruction(builder, Operation, firstParameter, secondParameter)
                {
                    ReferencedInstruction = ReferencedInstruction
                };
            }
            public override object[] GetArguments()
            {
                if (ReferencedInstruction == null)
                {
                    return base.GetArguments();
                }

                return [FirstParameter, SecondParameter, ReferencedInstruction];
            }

            public override string ToString()
            {
                return $"{Operation} [{FirstParameter?.Name}, {SecondParameter?.Name}, {ReferenceInstruction.ToStringReference(this)}]";
            }

            #endregion
        }
    }
}

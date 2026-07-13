using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class GenericCallingInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, DSharpMethodCallingInfo callingInfo)
            : Instruction(builder, operation)
        {
            /// <summary>
            /// Information about calling generic method
            /// </summary>
            public DSharpMethodCallingInfo CallingInfo { get; set; } = callingInfo;
            public unsafe override int SizeInBytes => base.SizeInBytes + (sizeof(DSharpMetadataToken) * (CallingInfo.GenericParameters.Count + 1));

            #region Управление

            public override void Write(Stream stream)
            {
                base.Write(stream);

                CallingInfo.Method.MetadataToken.Write(stream);

                foreach (var generic in CallingInfo.GenericParameters.Values)
                {
                    generic.MetadataToken.Write(stream);
                }
            }

            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new GenericCallingInstruction(builder, Operation, CallingInfo);
            }
            public override object[] GetArguments()
            {
                object[] arguments = new object[CallingInfo.GenericParameters.Count + 1];
                arguments[0] = CallingInfo.Method;

                int i = 1;

                foreach (var info in CallingInfo.GenericParameters)
                {
                    arguments[i] = info.Value;
                    i++;
                }

                return arguments;
            }

            public override string ToString()
            {
                return $"{Operation} [{CallingInfo.Method.ToString(CallingInfo.GenericParameters)}]";
            }

            #endregion
        }
    }
}

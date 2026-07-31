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
            public unsafe override int SizeInBytes
            {
                get
                {
                    int size = base.SizeInBytes + sizeof(DSharpMetadataToken) + sizeof(int);
                    var replacedTypes = CallingInfo.Method.GetReplacedTypesByGenericParameters(BytecodeBuilder.Method.Assembly, [.. CallingInfo.GenericParameters.Values]);
                    size += replacedTypes.Count * 2 * sizeof(DSharpMetadataToken);

                    return size;
                }
            }

            #region Управление

            public override void Write(Stream stream)
            {
                base.Write(stream);

                CallingInfo.Method.MetadataToken.Write(stream);

                var replacedTypes = CallingInfo.Method.GetReplacedTypesByGenericParameters(BytecodeBuilder.Method.Assembly, [.. CallingInfo.GenericParameters.Values]);
                stream.Write(replacedTypes.Count);

                foreach (var info in replacedTypes)
                {
                    info.Key.MetadataToken.Write(stream);
                    info.Value.MetadataToken.Write(stream);
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

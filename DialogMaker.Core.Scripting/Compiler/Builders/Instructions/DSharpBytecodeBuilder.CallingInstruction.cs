using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class CallingInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, DSharpMethodCallingInfo? callingInfo, IDSharpMemberInfo accessedMember)
            : Instruction(builder, operation)
        {
            public CallingInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, DSharpMethodCallingInfo callingInfo)
                : this(builder, operation, callingInfo, callingInfo.Method)
            {
            }
            public CallingInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpMemberInfo accessedMember)
                : this(builder, operation, null, accessedMember)
            {
            }

            /// <summary>
            /// Information about calling generic method
            /// </summary>
            public DSharpMethodCallingInfo? CallingInfo { get; set; } = callingInfo;
            /// <summary>
            /// Member that should be accessed
            /// </summary>
            public IDSharpMemberInfo AccessedMember { get; set; } = accessedMember;
            /// <summary>
            /// Is default method calling requested
            /// </summary>
            public bool RequestDefaultCalling { get; set; }
            public unsafe override int SizeInBytes
            {
                get
                {
                    int size = base.SizeInBytes + sizeof(DSharpMetadataToken) + sizeof(DSharpMethodCallingType);

                    if (CallingInfo != null)
                    {
                        var replacedTypes = CallingInfo.Method.GetReplacedTypesByGenericParameters(BytecodeBuilder.Method.Assembly, [.. CallingInfo.GenericParameters.Values]);
                        size += sizeof(int) + replacedTypes.Count * 2 * sizeof(DSharpMetadataToken);
                    }

                    return size;
                }
            }

            #region Управление

            public override void Write(Stream stream)
            {
                base.Write(stream);

                var callingMethod = AccessedMember;
                var callingInfo = CallingInfo;
                var callingType = AccessedMember.GetRequiredCallingType(RequestDefaultCalling);

                callingMethod.MetadataToken.Write(stream);
                stream.Write(callingType);
                stream.Write(callingInfo != null);

                if (callingInfo != null)
                {
                    var replacedTypes = callingInfo.Method.GetReplacedTypesByGenericParameters(BytecodeBuilder.Method.Assembly, [.. callingInfo.GenericParameters.Values]);

                    stream.Write(replacedTypes.Count);

                    foreach (var info in replacedTypes)
                    {
                        info.Key.MetadataToken.Write(stream);
                        info.Value.MetadataToken.Write(stream);
                    }
                }
            }

            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new CallingInstruction(builder, Operation, CallingInfo, AccessedMember)
                {
                    RequestDefaultCalling = RequestDefaultCalling
                };
            }
            public override object[] GetArguments()
            {
                var callingInfo = CallingInfo;

                object[] arguments = new object[(callingInfo?.GenericParameters.Count ?? 0) + 2];
                arguments[0] = AccessedMember;
                arguments[1] = AccessedMember.GetRequiredCallingType(RequestDefaultCalling);

                if (arguments.Length == 2 || callingInfo == null)
                {
                    return arguments;
                }

                int i = 2;

                foreach (var info in callingInfo.GenericParameters)
                {
                    arguments[i] = info.Value;
                    i++;
                }

                return arguments;
            }

            public override string ToString()
            {
                if (CallingInfo != null)
                {
                    return $"{Operation} [{CallingInfo.Method.ToString(CallingInfo.GenericParameters)}]";
                }

                return $"{Operation} [{AccessedMember}]";
            }

            #endregion
        }
    }
}

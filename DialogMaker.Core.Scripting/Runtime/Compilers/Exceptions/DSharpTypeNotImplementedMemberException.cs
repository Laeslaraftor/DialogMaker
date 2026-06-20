using System.Collections.ObjectModel;
using System.Text;

namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    public class DSharpTypeNotImplementedMemberException : Exception
    {
        public DSharpTypeNotImplementedMemberException(IDSharpType invalidType, IEnumerable<IDSharpMemberInfo> notImplementedMembers, string message) 
            : base(FormatMessage(message, notImplementedMembers))
        {
            InvalidType = invalidType;
            NotImplementedMembers = new([.. notImplementedMembers]);
        }
        public DSharpTypeNotImplementedMemberException(IDSharpType invalidType, IEnumerable<IDSharpMemberInfo> notImplementedMembers, string message, Exception innerException) 
            : base(FormatMessage(message, notImplementedMembers), innerException)
        {
            InvalidType = invalidType;
            NotImplementedMembers = new([.. notImplementedMembers]);
        }

        public IDSharpType InvalidType { get; }
        public ReadOnlyCollection<IDSharpMemberInfo> NotImplementedMembers { get; }

        #region Статика

        private static string FormatMessage(string message, IEnumerable<IDSharpMemberInfo> members)
        {
#if !DEBUG
            return message;
#endif
            if (!members.Any())
            {
                return message;
            }

            StringBuilder builder = new();
            builder.AppendLine(message);
            builder.AppendLine("Not implemented members:");

            foreach (var member in members)
            {
                builder.AppendLine(member.ToString());
            }

            return builder.ToString().TrimEnd();
        }

        #endregion
    }
}

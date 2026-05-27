using DialogMaker.Core.Scripting.Data;

namespace DialogMaker.Core.Scripting.Tokens
{
    public class DialogScriptNumberTokenParser : DialogScriptTokenParser
    {
        public override bool CanParse(string token)
        {
            if (token.Contains(Comma))
            {
                return false;
            }

            return int.TryParse(token, out _) || double.TryParse(token, out _) || float.TryParse(token, out _);
        }
        public override DialogScriptToken Parse(StringStream value)
        {
            var content = value.ReadWhile(IsNotSeparator).Trim();
            var number = OperandValue.AsNumber(content);
            
            return new DialogScriptNumberToken(number);
        }

        #region Статика

        public static readonly DialogScriptNumberTokenParser Instance = new();

        #endregion
    }
}
